using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client;
using System.IO;
using System.Threading;

namespace VideoModeratorQuickStart
{
    class Program
    {
        // declare constants and globals
        private static CloudMediaContext _context = null;
    
        // Azure Media Services authentication - see the quick start article to learn where to get these.
        private const string AZURE_AD_TENANT_NAME = "microsoft.onmicrosoft.com";
        private const string CLIENT_ID = "**************";
        private const string CLIENT_SECRET = "**************";

        // REST API endpoint, for example "https://accountname.restv2.westcentralus.media.azure.net/API".      
        private const string REST_API_ENDPOINT = "*********************";

        // Content Moderator Media Processor Name
        private const string MEDIA_PROCESSOR = "Azure Media Content Moderator";

        // Input and Output files in the current directory of the executable
        private const string INPUT_FILE = "*****";
        private const string OUTPUT_FOLDER = "";

        //a configuration file in the json format with the version number, also in the current directory
        private static readonly string CONTENT_MODERATOR_PRESET_FILE = "preset.json";
        //Example file:
        //        {
        //             "version": "2.0"
        //        }

        static void Main(string[] args)
        {
           
            // Read the preset settings
            string configuration = File.ReadAllText(CONTENT_MODERATOR_PRESET_FILE);

            // Get Azure AD credentials
            var tokenCredentials = new AzureAdTokenCredentials(AZURE_AD_TENANT_NAME,
                       new AzureAdClientSymmetricKey(CLIENT_ID, CLIENT_SECRET),
                       AzureEnvironments.AzureCloudEnvironment);

            // Initialize an Azure AD token
            var tokenProvider = new AzureAdTokenProvider(tokenCredentials);

            // Create a media context
            _context = new CloudMediaContext(new Uri(REST_API_ENDPOINT), tokenProvider);

            RunContentModeratorJob(INPUT_FILE, OUTPUT_FOLDER, configuration);
        }

        static void RunContentModeratorJob(string inputFilePath, string output, string configuration)
        {
            // create asset with input file
            IAsset asset = _context.Assets.CreateFromFile(inputFilePath, AssetCreationOptions.None);

            // grab instance of Azure Media Content Moderator MP
            IMediaProcessor mp = _context.MediaProcessors.GetLatestMediaProcessorByName(MEDIA_PROCESSOR);

            // create Job with Content Moderator task
            IJob job = _context.Jobs.Create(String.Format("Content Moderator {0}",
                Path.GetFileName(inputFilePath) + "_" + Guid.NewGuid()));

            ITask contentModeratorTask = job.Tasks.AddNew("Adult and Racy classifier task",
                mp, configuration,
                TaskOptions.None);
            contentModeratorTask.InputAssets.Add(asset);
            contentModeratorTask.OutputAssets.AddNew("Adult and Racy classifier output",
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

            DownloadAsset(job.OutputMediaAssets.First(), output);
        }

        static void DownloadAsset(IAsset asset, string outputDirectory)
        {
            foreach (IAssetFile file in asset.AssetFiles)
            {
                file.Download(Path.Combine(outputDirectory, file.Name));
            }
        }

        // event handler for Job State
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
