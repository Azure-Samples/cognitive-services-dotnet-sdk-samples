using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client;
using System.IO;
using System.Threading;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Collections.Generic;

namespace VideoModeratorQuickStart
{
    class Program
    {
        // declare constants and globals
        private static CloudMediaContext _context = null;
        private static CloudStorageAccount _StorageAccount = null;

        // Azure Media Services (AMS) associated Storage Account, Key, and the Container that has 
        // a list of Blobs to be processed.
        static string STORAGE_NAME = "YOUR AMS ASSOCIATED BLOB STORAGE NAME";
        static string STORAGE_KEY = "YOUR AMS ASSOCIATED BLOB STORAGE KEY";
        static string STORAGE_CONTAINER_NAME = "YOUR BLOB CONTAINER FOR VIDEO FILES";

        private static StorageCredentials _StorageCredentials = null;

        // Azure Media Services authentication
        private const string AZURE_AD_TENANT_NAME = "microsoft.onmicrosoft.com";
        private const string CLIENT_ID = "YOUR CLIENT ID";
        private const string CLIENT_SECRET = "YOUR CLIENT SECRET";

        // REST API endpoint, for example "https://accountname.restv2.westcentralus.media.azure.net/API".      
        private const string REST_API_ENDPOINT = "YOUR API ENDPOINT";

        // Content Moderator Media Processor Name
        private const string MEDIA_PROCESSOR = "Azure Media Content Moderator";

        // Input and Output files in the current directory of the executable
        private const string INPUT_FILE = "VIDEO FILE NAME";
        private const string OUTPUT_FOLDER = "";

        //a configuration file in the json format with the version number, also in the current directory
        private static readonly string CONTENT_MODERATOR_PRESET_FILE = "preset.json";
        //Example file:
        //        {
        //             "version": "2.0"
        //        }

        static void Main(string[] args)
        {
            // Create Azure Media Context
            CreateMediaContext();

            // Create Storage Context
            CreateStorageContext();

            // Use a file as the input.
            //IAsset asset = CreateAssetfromFile();
            // -- OR ---
            // Or a blob as the input
            //IAsset asset = CreateAssetfromBlob((CloudBlockBlob)GetBlobsList().First());

            // Then submit the asset to Content Moderator
            //RunContentModeratorJob(asset);

            //-- OR ----

            // Just run the content moderator on all blobs in a list (from a Blob Container)
            RunContentModeratorJobOnBlobs();

        }

        /// <summary>
        /// Creates a media context from azure credentials
        /// </summary>
        static void CreateMediaContext()
        {
            // Get Azure AD credentials
            var tokenCredentials = new AzureAdTokenCredentials(AZURE_AD_TENANT_NAME,
                       new AzureAdClientSymmetricKey(CLIENT_ID, CLIENT_SECRET),
                       AzureEnvironments.AzureCloudEnvironment);

            // Initialize an Azure AD token
            var tokenProvider = new AzureAdTokenProvider(tokenCredentials);

            // Create a media context
            _context = new CloudMediaContext(new Uri(REST_API_ENDPOINT), tokenProvider);
        }

        /// <summary>
        /// Creates a storage context from the AMS associated storage name and key
        /// </summary>
        static void CreateStorageContext()
        {
            // Get a reference to the storage account associated with a Media Services account. 
            if (_StorageCredentials == null)
            {
                _StorageCredentials = new StorageCredentials(STORAGE_NAME, STORAGE_KEY);
            }
            _StorageAccount = new CloudStorageAccount(_StorageCredentials, false);
        }

        /// <summary>
        /// Creates an Azure Media Services Asset from the video file
        /// </summary>
        /// <returns>Asset</returns>
        static IAsset CreateAssetfromFile()
        {
            return _context.Assets.CreateFromFile(INPUT_FILE, AssetCreationOptions.None); ;
        }

        /// <summary>
        /// Creates an Azure Media Services asset from your blog storage
        /// </summary>
        /// <param name="Blob"></param>
        /// <returns>Asset</returns>
        static IAsset CreateAssetfromBlob(CloudBlockBlob Blob)
        {
            // Create asset from the FIRST blob in the list and return it
            return _context.Assets.CreateFromBlob(Blob, _StorageCredentials, AssetCreationOptions.None);
        }

        /// <summary>
        /// Runs the Content Moderator Job on all Blobs in a given container name
        /// </summary>
        static void RunContentModeratorJobOnBlobs()
        {

            // Get the reference to the list of Blobs 
            var blobList = GetBlobsList();

            // Iterate over the Blob list items or work on specific ones as needed
            foreach (var sourceBlob in blobList)
            {
                // Create an Asset
                IAsset asset = _context.Assets.CreateFromBlob((CloudBlockBlob)sourceBlob,
                                                            _StorageCredentials,
                                                            AssetCreationOptions.None);
                asset.Update();

                // Submit to Content Moderator
                RunContentModeratorJob(asset);
            }
        }

        /// <summary>
        /// Get all blobs in your container
        /// </summary>
        /// <returns></returns>
        static IEnumerable<IListBlobItem> GetBlobsList()
        {
            // Get a reference to the Container within the Storage Account
            // that has the files (blobs) for moderation
            CloudBlobClient CloudBlobClient = _StorageAccount.CreateCloudBlobClient();
            CloudBlobContainer MediaBlobContainer = CloudBlobClient.GetContainerReference(STORAGE_CONTAINER_NAME);

            // Get the reference to the list of Blobs 
            var blobList = MediaBlobContainer.ListBlobs();
            return blobList;
        }

        /// <summary>
        /// Run the Content Moderator job on the designated Asset from local file or blob storage
        /// </summary>
        /// <param name="asset"></param>
        static void RunContentModeratorJob(IAsset asset)
        {
            // Grab the presets
            string configuration = File.ReadAllText(CONTENT_MODERATOR_PRESET_FILE);

            // grab instance of Azure Media Content Moderator MP
            IMediaProcessor mp = _context.MediaProcessors.GetLatestMediaProcessorByName(MEDIA_PROCESSOR);

            // create Job with Content Moderator task
            IJob job = _context.Jobs.Create(String.Format("Content Moderator {0}",
                asset.AssetFiles.First() + "_" + Guid.NewGuid()));

            ITask contentModeratorTask = job.Tasks.AddNew("Adult and racy classifier task",
                mp, configuration,
                TaskOptions.None);
            contentModeratorTask.InputAssets.Add(asset);
            contentModeratorTask.OutputAssets.AddNew("Adult and racy classifier output",
            AssetCreationOptions.None);

            job.Submit();


            // Create progress printing and querying tasks
            Task progressPrintTask = new Task(() =>
            {
                IJob jobQuery = null;
                do
                {
                    var progressContext = _context;
                    jobQuery = progressContext.Jobs
                    .Where(j => j.Id == job.Id)
                    .First();
                    Console.WriteLine(string.Format("{0}\t{1}",
                    DateTime.Now,
                    jobQuery.State));
                    Thread.Sleep(10000);
                }
                while (jobQuery.State != JobState.Finished &&
                jobQuery.State != JobState.Error &&
                jobQuery.State != JobState.Canceled);
            });
            progressPrintTask.Start();


            Task progressJobTask = job.GetExecutionProgressTask(
            CancellationToken.None);
            progressJobTask.Wait();

            // If job state is Error, the event handling 
            // method for job progress should log errors.  Here we check 
            // for error state and exit if needed.
            if (job.State == JobState.Error)
            {
                ErrorDetail error = job.Tasks.First().ErrorDetails.First();
                Console.WriteLine(string.Format("Error: {0}. {1}",
                error.Code,
                error.Message));
            }

            DownloadAsset(job.OutputMediaAssets.First(), OUTPUT_FOLDER);
        }

        /// <summary>
        /// Download the given asset to the output directory
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="outputDirectory"></param>
        static void DownloadAsset(IAsset asset, string outputDirectory)
        {
            foreach (IAssetFile file in asset.AssetFiles)
            {
                file.Download(Path.Combine(outputDirectory, file.Name));
            }
        }

        /// <summary>
        /// Event handler for job state to log job progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void StateChanged(object sender, JobStateChangedEventArgs e)
        {
            Console.WriteLine("Job state changed event:");
            Console.WriteLine("  Previous state: " + e.PreviousState);
            Console.WriteLine("  Current state: " + e.CurrentState);
            switch (e.CurrentState)
            {
                case JobState.Finished:
                    Console.WriteLine();
                    Console.WriteLine("Job finished.");
                    break;
                case JobState.Canceling:
                case JobState.Queued:
                case JobState.Scheduled:
                case JobState.Processing:
                    Console.WriteLine("Please wait...\n");
                    break;
                case JobState.Canceled:
                    Console.WriteLine("Job is canceled.\n");
                    break;
                case JobState.Error:
                    Console.WriteLine("Job failed.\n");
                    break;
                default:
                    break;
            }
        }
    }

}
