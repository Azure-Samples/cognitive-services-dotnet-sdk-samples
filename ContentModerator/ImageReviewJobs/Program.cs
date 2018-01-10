using Microsoft.CognitiveServices.ContentModerator.Models;
using ModeratorHelper;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;

namespace ImageReviewJobs
{
    class Program
    {
        /// <summary>
        /// The moderation job will use this workflow that you defined earlier.
        /// See the quickstart article to learn how to setup custom workflows.
        /// </summary>
        private const string WorkflowName = "OCR";

        /// <summary>
        /// The name of the team to assign the job to.
        /// </summary>
        /// <remarks>This must be the team name you used to create your 
        /// Content Moderator account. You can retrieve your team name from
        /// the Conent Moderator web site. Your team name is the Id associated 
        /// with your subscription.</remarks>
        private const string TeamName = "testreview6";

        /// <summary>
        /// The URL of the image to create a review job for.
        /// </summary>
        private const string ImageUrl =
            "https://moderatorsampleimages.blob.core.windows.net/samples/sample2.jpg";

        /// <summary>
        /// The name of the log file to create.
        /// </summary>
        /// <remarks>Relative paths are ralative the execution directory.</remarks>
        private const string OutputFile = "OutputLog.txt";

        /// <summary>
        /// The number of seconds to delay after a review has finished before
        /// getting the review results from the server.
        /// </summary>
        private const int latencyDelay = 45;

        /// <summary>
        /// The callback endpoint for completed reviews.
        /// </summary>
        /// <remarks>Revies show up for reviewers on your team. 
        /// As reviewers complete reviews, results are sent to the
        /// callback endpoint using an HTTP POST request.</remarks>
        private const string CallbackEndpoint = "https://requestb.in/vxke1mvx";

        static void Main(string[] args)
        {
            using (TextWriter writer = new StreamWriter(OutputFile, false))
            {
                using (var client = Clients.NewClient())
                {
                    writer.WriteLine("Create moderation job for an image.");
                    var content = new Content(ImageUrl);

                    // The WorkflowName contains the nameof the workflow defined in the online review tool.
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
        }
    }
}
