using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.CognitiveServices.ContentModerator;
using Microsoft.CognitiveServices.ContentModerator.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;

namespace ImageReviewJobs
{
    class Program
    {
        /// <summary>
        /// The moderation job will use this workflow that you should have defined earlier.
        /// See the quickstart article to learn how to setup this workflows.
        /// </summary>
        private const string WorkflowName = "OCR";

        /// <summary>
        /// The name of the team to assign the job to.
        /// </summary>
        /// <remarks>This must be the team name you used to create your 
        /// Content Moderator account. You can retrieve your team name from
        /// the Content Moderator web site. Your team name is the Id associated 
        /// with your subscription.</remarks>
        private const string TeamName = "YOUR REVIEW TEAM ID";

        /// <summary>
        /// The URL of the image to create a review job for.
        /// </summary>
        private const string ImageUrl =
            "https://moderatorsampleimages.blob.core.windows.net/samples/sample2.jpg";

        /// <summary>
        /// The name of the log file to create.
        /// </summary>
        /// <remarks>Relative paths are relative to the execution directory.</remarks>
        private const string OutputFile = "OutputLog.txt";

        /// <summary>
        /// The number of seconds to delay after a review has finished before
        /// getting the review results from the server.
        /// </summary>
        private const int latencyDelay = 45;

        /// <summary>
        /// The callback endpoint for completed reviews.
        /// </summary>
        /// <remarks>Reviews show up for reviewers on your team. 
        /// As reviewers complete reviews, results are sent to the
        /// callback endpoint using an HTTP POST request.</remarks>
        private const string CallbackEndpoint = "https://webhook.site/0dce0d0d-41ef-4ae9-abb8-f1bc42b264f4";

        static void Main(string[] args)
        {
            using (TextWriter writer = new StreamWriter(OutputFile, false))
            {
                using (var client = Clients.NewClient())
                {
                    writer.WriteLine("Create moderation job for an image.");
                    var content = new Content(ImageUrl);

                    // The WorkflowName contains the name of the workflow defined in the online review tool.
                    // See the quickstart article to learn more.
                    var jobResult = client.Reviews.CreateJobWithHttpMessagesAsync(
                        TeamName, "image", "contentID", WorkflowName, "application/json", content, CallbackEndpoint);

                    // Record the job ID.
                    var jobId = jobResult.Result.Body.JobIdProperty;

                    // Log just the response body from the returned task.
                    writer.WriteLine(JsonConvert.SerializeObject(
                        jobResult.Result.Body, Formatting.Indented));

                    Thread.Sleep(2000);
                    writer.WriteLine();

                    writer.WriteLine("Get job status before review.");
                    var jobDetails = client.Reviews.GetJobDetailsWithHttpMessagesAsync(
                        TeamName, jobId);

                    // Log just the response body from the returned task.
                    writer.WriteLine(JsonConvert.SerializeObject(
                        jobDetails.Result.Body, Formatting.Indented));

                    Console.WriteLine();
                    Console.WriteLine("Perform manual reviews on the Content Moderator site.");
                    Console.WriteLine("Then, press any key to continue.");
                    Console.ReadKey();

                    Console.WriteLine();
                    Console.WriteLine($"Waiting {latencyDelay} seconds for results to propagate.");
                    Thread.Sleep(latencyDelay * 1000);

                    writer.WriteLine("Get job status after review.");
                    jobDetails = client.Reviews.GetJobDetailsWithHttpMessagesAsync(
                        TeamName, jobId);

                    // Log just the response body from the returned task.
                    writer.WriteLine(JsonConvert.SerializeObject(
                        jobDetails.Result.Body, Formatting.Indented));
                }
                writer.Flush();
                writer.Close();
            }
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Wraps the creation and configuration of a Content Moderator client.
    /// </summary>
    /// <remarks>This class library contains insecure code. If you adapt this 
    /// code for use in production, use a secure method of storing and using
    /// your Content Moderator subscription key.</remarks>
    public static class Clients
    {
        // TODO We could make team name a static property on this class, to move all of the subscription information into one project.
        
        /// <summary>
        /// The base URL fragment for Content Moderator calls.
        /// Add your Azure Content Moderator endpoint to your environment variables
        /// </summary>
        private static readonly string AzureBaseURL =
            Environment.GetEnvironmentVariable("CONTENT_MODERATOR_ENDPOINT");

        /// <summary>
        /// Your Content Moderator subscription key.
        /// Add your Azure Content Moderator subscription key to your environment variables
        /// </summary>
        private static readonly string CMSubscriptionKey = 
            Environment.GetEnvironmentVariable("CONTENT_MODERATOR_SUBSCRIPTION_KEY");

        /// <summary>
        /// Returns a new Content Moderator client for your subscription.
        /// </summary>
        /// <returns>The new client.</returns>
        /// <remarks>The <see cref="ContentModeratorClient"/> is disposable.
        /// When you have finished using the client,
        /// you should dispose of it either directly or indirectly. </remarks>
        public static ContentModeratorClient NewClient()
        {
            // Create and initialize an instance of the Content Moderator API wrapper.
            ContentModeratorClient client = new ContentModeratorClient(new ApiKeyServiceClientCredentials(CMSubscriptionKey));

            client.Endpoint = AzureBaseURL;
            return client;
        }
    }
}
