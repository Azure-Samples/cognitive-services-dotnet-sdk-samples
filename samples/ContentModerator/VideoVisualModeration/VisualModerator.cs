/*
 * Copyright (c) 2019
 * Released under the MIT license
 * http://opensource.org/licenses/mit-license.php
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.IdentityModel.Clients.ActiveDirectory;


namespace Microsoft.ContentModerator.VideoContentModerator
{
    public class VisualModerator
    {
        #region Static parameters
        const string VisualModerationTransformName = "VisualModerationTransform";
        const string TransformNameWithEncodingSuffix = "WithEncoding";
        const string JobNamePrefix = "job-";
        const string InputAssetNamePrefix = "asset-input-";
        const string VideoOutputAssetNamePrefix = "asset-output-video-";
        const string AnalyticsOutputAssetNamePrefix = "asset-output-analytics-";
        const string VideoStreamingLocatorPrefix = "streaminglocator-video-";
        const string AnalyticsStreamingLocatorPrefix = "streaminglocator-analysis-";
        const string JobInputLabel = "InputVideo";
        const string JobVideoOutputLabel = "OutputVideo";
        const string JobAnalyticsOutputLabel = "OutputAnalytics";
        #endregion

        private VisualModeratorConfig visualModeratorConfig;

        public VisualModerator(ConfigWrapper config)
        {
            this.visualModeratorConfig = config.visualModerator;
        }

        public async Task<VisualModerationResult> ModerateVideo(string videoPath)
        {
            string resourceGroup = this.visualModeratorConfig.ResourceGroup;
            string accountName = this.visualModeratorConfig.AccountName;

            IAzureMediaServicesClient client = await CreateMediaServicesClientAsync();
            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 2;
            VisualModerationResult result = null;

            try
            {
                string uniqueness = Guid.NewGuid().ToString();
                string jobName = JobNamePrefix + uniqueness;
                string streamingOutputAssetName = VideoOutputAssetNamePrefix + uniqueness;
                string analyticsOutputAssetName = AnalyticsOutputAssetNamePrefix + uniqueness;

                JobInput jobInput = null;
                List<Asset> outputAssetList = new List<Asset>();
                Transform videoAnalyzerTransform = EnsureTransformExists(client, resourceGroup, accountName, VisualModerationTransformName);

                if (videoPath.StartsWith("http://") || videoPath.StartsWith("https://"))
                {
                    Uri inputVideoUri = new Uri(videoPath);
                    string baseUri = inputVideoUri.Scheme + "://" + inputVideoUri.Host;
                    for (int i = 0; i < inputVideoUri.Segments.Length - 1; i++)
                    {
                        baseUri = baseUri + inputVideoUri.Segments[i];
                    }
                    jobInput = new JobInputHttp(
                        baseUri: baseUri,
                        files: new List<String> { inputVideoUri.Segments[inputVideoUri.Segments.Length - 1] },
                        label: JobInputLabel
                    );
                }
                else
                {
                    string inputAssetName = InputAssetNamePrefix + uniqueness;
                    CreateInputAssetWithUploading(client, resourceGroup, accountName, inputAssetName, videoPath).Wait();
                    jobInput = new JobInputAsset(assetName: inputAssetName, label: JobInputLabel);
                }

                List<JobOutput> jobOutputList = new List<JobOutput>();
                Asset analyticsOutputAsset = CreateOutputAsset(client, resourceGroup, accountName, analyticsOutputAssetName);
                outputAssetList.Add(analyticsOutputAsset);
                jobOutputList.Add(new JobOutputAsset(assetName: analyticsOutputAsset.Name, label: JobAnalyticsOutputLabel));
                if (this.visualModeratorConfig.EnableStreamingVideo)
                {
                    Asset streamingOutputAsset = CreateOutputAsset(client, resourceGroup, accountName, streamingOutputAssetName);
                    outputAssetList.Add(streamingOutputAsset);
                    jobOutputList.Add(new JobOutputAsset(assetName: streamingOutputAsset.Name, label: JobVideoOutputLabel));
                }

                JobOutput[] jobOutputs = jobOutputList.ToArray();
                Job job = SubmitJob(client, resourceGroup, accountName, videoAnalyzerTransform.Name, jobName, jobInput, jobOutputs);
                job = WaitForJobToFinish(client, resourceGroup, accountName, videoAnalyzerTransform.Name, jobName);

                if (job.State == JobState.Finished)
                {
                    Console.WriteLine("\nAMSv3 Job finished.");
                    List<StreamingLocator> locators = PublishAssets(client, resourceGroup, accountName, jobOutputList);
                    result = CreateVisualModeratorResult(client, resourceGroup, accountName, videoPath, locators);

                    using (var webClient = new WebClient())
                    {
                        webClient.Encoding = Encoding.UTF8;
                        result.VisualModerationJson = webClient.DownloadString(result.StreamingUrlDetails.ContentModerationJsonUrl);
                        result.OcrJson = webClient.DownloadString(result.StreamingUrlDetails.OcrJsonUrl);
                    }
                }
                else if (job.State == JobState.Error)
                {
                    Console.WriteLine($"ERROR: Job finished with error message: {job.Outputs[0].Error.Message}");
                    Console.WriteLine($"ERROR:                   error details: {job.Outputs[0].Error.Details[0].Message}");
                }

            }
            catch (ApiErrorException ex)
            {
                string code = ex.Body.Error.Code;
                string message = ex.Body.Error.Message;

                Console.WriteLine("ERROR:API call failed with error code: {0} and message: {1}", code, message);
            }

            return result;
        }

        #region private methods for Azure Media Services v3 functions

        private async Task<IAzureMediaServicesClient> CreateMediaServicesClientAsync()
        {
            ClientCredential clientCredential = new ClientCredential(this.visualModeratorConfig.AadClientId, this.visualModeratorConfig.AadClientSecret);
            var credentials = await ApplicationTokenProvider.LoginSilentAsync(this.visualModeratorConfig.AadTenantId, clientCredential, ActiveDirectoryServiceSettings.Azure);

            return new AzureMediaServicesClient(this.visualModeratorConfig.ArmEndpoint, credentials)
            {
                SubscriptionId = this.visualModeratorConfig.SubscriptionId
            };
        }

        private Transform EnsureTransformExists(IAzureMediaServicesClient client, string resourceGroupName, string accountName, string transformName)
        {
            if (this.visualModeratorConfig.EnableStreamingVideo)
            {
                transformName = transformName + TransformNameWithEncodingSuffix;
            }
            Transform transform = client.Transforms.Get(resourceGroupName, accountName, transformName);

            if (transform == null)
            {
                List<TransformOutput> outputList = new List<TransformOutput>();
                outputList.Add(new TransformOutput(new VideoAnalyzerPreset()));
                if(this.visualModeratorConfig.EnableStreamingVideo)
                {
                    outputList.Add(new TransformOutput(new BuiltInStandardEncoderPreset(EncoderNamedPreset.AdaptiveStreaming)));
                }

                TransformOutput[] outputs = outputList.ToArray();
                transform = client.Transforms.CreateOrUpdate(resourceGroupName, accountName, transformName, outputs);
                Console.WriteLine("AMSv3 Transform has been created: {0}", transformName);
            }
            else
            {
                Console.WriteLine("Using generated AMSv3 Transform: {0}", transformName);
            }
            return transform;
        }

        private async Task<Asset> CreateInputAssetWithUploading(IAzureMediaServicesClient client, string resourceGroupName, string accountName, string assetName, string fileToUpload)
        {
            string filename = Path.GetFileName(fileToUpload);
            Console.WriteLine("Uploading file: {0}", filename);

            Asset asset = client.Assets.CreateOrUpdate(resourceGroupName, accountName, assetName, new Asset());
            ListContainerSasInput input = new ListContainerSasInput()
            {
                Permissions = AssetContainerPermission.ReadWrite,
                ExpiryTime = DateTime.Now.AddHours(2).ToUniversalTime()
            };
            var response = client.Assets.ListContainerSasAsync(resourceGroupName, accountName, assetName, input.Permissions, input.ExpiryTime).Result;
            string uploadSasUrl = response.AssetContainerSasUrls.First();
            var sasUri = new Uri(uploadSasUrl);

            CloudBlobContainer container = new CloudBlobContainer(sasUri);
            var blob = container.GetBlockBlobReference(filename);
            blob.Properties.ContentType = "video/mp4";
            Console.WriteLine("Uploading File to AMSv3 asset container: {0}", sasUri);
            await blob.UploadFromFileAsync(fileToUpload);

            return asset;
        }

        private Asset CreateOutputAsset(IAzureMediaServicesClient client, string resourceGroupName, string accountName, string assetName)
        {
            return client.Assets.CreateOrUpdate(resourceGroupName, accountName, assetName, new Asset());
        }

        private Job SubmitJob(IAzureMediaServicesClient client, string resourceGroupName, string accountName, string transformName, string jobName, JobInput jobInput, JobOutput[] jobOutputs)
        {
            Job job = client.Jobs.Create(
                resourceGroupName,
                accountName,
                transformName,
                jobName,
                new Job
                {
                    Input = jobInput,
                    Outputs = jobOutputs
                });

            Console.WriteLine("AMSv3 Job has been submitted.");
            return job;
        }

        private Job WaitForJobToFinish(IAzureMediaServicesClient client, string resourceGroupName, string accountName, string transformName, string jobName)
        {
            const int SleepInterval = 10 * 1000;

            Job job = null;
            bool exit = false;

            Console.Write("AMSv3 Job is running");
            do
            {
                job = client.Jobs.Get(resourceGroupName, accountName, transformName, jobName);

                if (job.State == JobState.Finished || job.State == JobState.Error || job.State == JobState.Canceled)
                {
                    exit = true;
                }
                else
                {
                    for (int i = 0; i < job.Outputs.Count; i++)
                    {
                        JobOutput output = job.Outputs[i];
                        if (output.State == JobState.Processing)
                        {
                            Console.Write(".");
                        }
                    }
                    System.Threading.Thread.Sleep(SleepInterval);
                }
            }
            while (!exit);

            return job;
        }

        private List<StreamingLocator> PublishAssets(IAzureMediaServicesClient client, string resourceGroup, string accountName, List<JobOutput> jobOutputAssetList)
        {
            List<StreamingLocator> locators = new List<StreamingLocator>();
            foreach(var jobOutput in jobOutputAssetList)
            {
                JobOutputAsset jobOutputAsset = (JobOutputAsset)jobOutput;
                if (jobOutputAsset.Label == JobVideoOutputLabel)
                {
                    Guid newGuid = Guid.NewGuid();
                    locators.Add(PublishStreamingAsset(client, resourceGroup, accountName, jobOutputAsset.AssetName, newGuid));
                }
                else if (jobOutputAsset.Label == JobAnalyticsOutputLabel)
                {
                    Guid newGuid = Guid.NewGuid();
                    locators.Add(PublishDownloadAsset(client, resourceGroup, accountName, jobOutputAsset.AssetName, newGuid));
                }
            }
            return locators;
        }

        private StreamingLocator PublishStreamingAsset(IAzureMediaServicesClient client, string resourceGroup, string accountName, string assetName, Guid streamingLocatorId)
        {
            string streamingLocatorName = VideoStreamingLocatorPrefix + streamingLocatorId.ToString();
            StreamingLocator locator = new StreamingLocator(
                assetName: assetName,
                streamingLocatorId: streamingLocatorId,
                streamingPolicyName: PredefinedStreamingPolicy.ClearStreamingOnly
            );
            return client.StreamingLocators.Create(resourceGroup, accountName, streamingLocatorName, locator);
        }

        private StreamingLocator PublishDownloadAsset(IAzureMediaServicesClient client, string resourceGroup, string accountName, string assetName, Guid streamingLocatorId)
        {
            string streamingLocatorName = AnalyticsStreamingLocatorPrefix + streamingLocatorId.ToString();
            StreamingLocator locator = new StreamingLocator(
                assetName: assetName,
                streamingLocatorId: streamingLocatorId,
                streamingPolicyName: PredefinedStreamingPolicy.DownloadOnly
            );
            return client.StreamingLocators.Create(resourceGroup, accountName, streamingLocatorName, locator);
        }

        private VisualModerationResult CreateVisualModeratorResult(IAzureMediaServicesClient client, string resourceGroup, string accountName, string videoPath, List<StreamingLocator> locators)
        {
            VisualModerationResult result = new VisualModerationResult();
            result.StreamingUrlDetails = new PublishedUrlDetails();
            result.VideoFilePath = videoPath;

            var streamingEndpoint = client.StreamingEndpoints.Get(resourceGroup, accountName, "default");
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = "https";
            uriBuilder.Host = streamingEndpoint.HostName;

            foreach (var locator in locators)
            {
                if (locator.Name.StartsWith(VideoStreamingLocatorPrefix))
                {
                    var paths = client.StreamingLocators.ListPaths(resourceGroup, accountName, locator.Name);
                    for (int i = 0; i < paths.StreamingPaths.Count; i++)
                    {
                        if (paths.StreamingPaths[i].Paths.Count > 0)
                        {
                            if (paths.StreamingPaths[i].StreamingProtocol == StreamingPolicyStreamingProtocol.SmoothStreaming)
                            {
                                uriBuilder.Path = paths.StreamingPaths[i].Paths[0];
                                result.StreamingUrlDetails.SmoothUri = uriBuilder.ToString();
                            }
                            else if (paths.StreamingPaths[i].StreamingProtocol == StreamingPolicyStreamingProtocol.Dash)
                            {
                                uriBuilder.Path = paths.StreamingPaths[i].Paths[0];
                                result.StreamingUrlDetails.MpegDashUri = uriBuilder.ToString();
                            }
                            else if (paths.StreamingPaths[i].StreamingProtocol == StreamingPolicyStreamingProtocol.Hls)
                            {
                                uriBuilder.Path = paths.StreamingPaths[i].Paths[0];
                                result.StreamingUrlDetails.HlsUri = uriBuilder.ToString();
                            }
                        }
                    }
                    result.VideoName = locator.Name;
                    result.AccessToken = null;
                }
                else if (locator.Name.StartsWith(AnalyticsStreamingLocatorPrefix))
                {
                    var dlpaths = client.StreamingLocators.ListPaths(resourceGroup, accountName, locator.Name);
                    for (int i = 0; i < dlpaths.DownloadPaths.Count; i++)
                    {
                        if (dlpaths.DownloadPaths[i].EndsWith("transcript.vtt"))
                        {
                            uriBuilder.Path = dlpaths.DownloadPaths[i];
                            result.StreamingUrlDetails.VttUrl = uriBuilder.ToString();
                        }
                        else if (dlpaths.DownloadPaths[i].EndsWith("contentmoderation.json"))
                        {
                            uriBuilder.Path = dlpaths.DownloadPaths[i];
                            result.StreamingUrlDetails.VttUrl = uriBuilder.ToString();
                        }
                        else if (dlpaths.DownloadPaths[i].EndsWith("ocr.json"))
                        {
                            uriBuilder.Path = dlpaths.DownloadPaths[i];
                            result.StreamingUrlDetails.VttUrl = uriBuilder.ToString();
                        }
                    }
                }
            }
            return result;
        }

        #endregion
    }
}
