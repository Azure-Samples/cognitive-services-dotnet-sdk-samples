using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.CognitiveServices.ContentModerator;
using Microsoft.CognitiveServices.ContentModerator.Models;
using Newtonsoft.Json;

namespace VideoReviews
{
    class Program
    {
        /// <summary>
        /// Your Content Moderator subscription key.
        // Add your Azure Content Moderator subscription key to your environment variables.
        /// </summary>
        private static readonly string CMSubscriptionKey = 
            Environment.GetEnvironmentVariable("CONTENT_MODERATOR_SUBSCRIPTION_KEY");

        // NOTE: Replace this example team name with your Content Moderator team Id.
        /// <summary>
        /// The name of the team to assign the job to.
        /// </summary>
        /// <remarks>This must be the team name you used to create your 
        /// Content Moderator account. You can retrieve your team name from
        /// the Content Moderator web site. Your team name is the Id associated 
        /// with your subscription.</remarks>
        private const string TeamName = "YOUR REVIEW TEAM ID";

        /// <summary>
        /// The base URL fragment for Content Moderator calls.
        /// Add your Azure Content Moderator endpoint to your environment variables.
        /// </summary>
        private static readonly string AzureBaseURL =
            Environment.GetEnvironmentVariable("CONTENT_MODERATOR_ENDPOINT");

        /// <summary>
        /// The minimum amount of time, in milliseconds, to wait between calls
        /// to the Content Moderator APIs.
        /// </summary>
        private const int throttleRate = 2000;

        /// <summary>
        /// Returns a new Content Moderator client for your subscription.
        /// </summary>
        /// <returns>The new client.</returns>
        /// <remarks>The <see cref="ContentModeratorClient"/> is disposable.
        /// When you have finished using the client,
        /// you should dispose of it either directly or indirectly. </remarks>
        public static ContentModeratorClient NewClient()
        {
            return new ContentModeratorClient(new ApiKeyServiceClientCredentials(CMSubscriptionKey))
            {
                Endpoint = AzureBaseURL
            };
        }

        /// <summary>
        /// Create a video review. For more information, see the API reference:
        /// https://westus2.dev.cognitive.microsoft.com/docs/services/580519463f9b070e5c591178/operations/580519483f9b0709fc47f9c4 
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="id">The ID to assign to the video review.</param>
        /// <param name="content">The URL of the video to review.</param>
        /// <returns>The ID of the video review.</returns>
        private static string CreateReview(ContentModeratorClient client, string id, string content)
        {
            Console.WriteLine("Creating a video review.");

            List<CreateVideoReviewsBodyItem> body = new List<CreateVideoReviewsBodyItem>() {
                new CreateVideoReviewsBodyItem
                {
                    Content = content,
                    ContentId = id,
                    /* Note: to create a published review, set the Status to "Pending".
                     * However, you cannot add video frames or a transcript to a published review. */
                    Status = "Unpublished",
                }
            };

            var result = client.Reviews.CreateVideoReviews("application/json", TeamName, body);

            Thread.Sleep(throttleRate);

            // We created only one review.
            return result[0];
        }

        /// <summary>
        /// Create a video frame to add to a video review after the video review is created.
        /// </summary>
        /// <param name="url">The URL of the video frame image.</param>
        /// <returns>The video frame.</returns>
        private static VideoFrameBodyItem CreateFrameToAddToReview(string url, string timestamp_seconds)
        {
            // We generate random "adult" and "racy" scores for the video frame.
            Random rand = new Random();

            var frame = new VideoFrameBodyItem
            {
                // The timestamp is measured in milliseconds. Convert from seconds.
                Timestamp = (int.Parse(timestamp_seconds) * 1000).ToString(),
                FrameImage = url,

                Metadata = new List<VideoFrameBodyItemMetadataItem>
                {
                    new VideoFrameBodyItemMetadataItem("reviewRecommended", "true"),
                    new VideoFrameBodyItemMetadataItem("adultScore", rand.NextDouble().ToString()),
                    new VideoFrameBodyItemMetadataItem("a", "false"),
                    new VideoFrameBodyItemMetadataItem("racyScore", rand.NextDouble().ToString()),
                    new VideoFrameBodyItemMetadataItem("r", "false")
                },

                ReviewerResultTags = new List<VideoFrameBodyItemReviewerResultTagsItem>()
                {
                    new VideoFrameBodyItemReviewerResultTagsItem("tag1", "value1")
                }
            };

            return frame;
        }

        /// <summary>
        /// Add a video frame to the indicated video review. For more information, see the API reference:
        /// https://westus2.dev.cognitive.microsoft.com/docs/services/580519463f9b070e5c591178/operations/59e7b76ae7151f0b10d451fd
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="review_id">The video review ID.</param>
        /// <param name="url">The URL of the video frame image.</param>
        static void AddFrame(ContentModeratorClient client, string review_id, string url, string timestamp_seconds)
        {
            Console.WriteLine("Adding a frame to the review with ID {0}.", review_id);

            var frames = new List<VideoFrameBodyItem>()
            {
                CreateFrameToAddToReview(url, timestamp_seconds)
            };
            
            client.Reviews.AddVideoFrameUrl("application/json", TeamName, review_id, frames);

            Thread.Sleep(throttleRate);
        }

        /// <summary>
        /// Get the video frames assigned to the indicated video review.  For more information, see the API reference:
        /// https://westus2.dev.cognitive.microsoft.com/docs/services/580519463f9b070e5c591178/operations/59e7ba43e7151f0b10d45200
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="review_id">The video review ID.</param>
        static void GetFrames(ContentModeratorClient client, string review_id)
        {
            Console.WriteLine("Getting frames for the review with ID {0}.", review_id);

            Frames result = client.Reviews.GetVideoFrames(TeamName, review_id);
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));

            Thread.Sleep(throttleRate);
        }

        /// <summary>
        /// Get the information for the indicated video review. For more information, see the reference API:
        /// https://westus2.dev.cognitive.microsoft.com/docs/services/580519463f9b070e5c591178/operations/580519483f9b0709fc47f9c2
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="review_id">The video review ID.</param>
        private static void GetReview(ContentModeratorClient client, string review_id)
        {
            Console.WriteLine("Getting the status for the review with ID {0}.", review_id);

            var result = client.Reviews.GetReview(TeamName, review_id);
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));

            Thread.Sleep(throttleRate);
        }

        /// <summary>
        /// Publish the indicated video review. For more information, see the reference API:
        /// https://westus2.dev.cognitive.microsoft.com/docs/services/580519463f9b070e5c591178/operations/59e7bb29e7151f0b10d45201
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="review_id">The video review ID.</param>
        private static void PublishReview(ContentModeratorClient client, string review_id)
        {
            Console.WriteLine("Publishing the review with ID {0}.", review_id);

            client.Reviews.PublishVideoReview(TeamName, review_id);

            Thread.Sleep(throttleRate);
        }

        static void Main(string[] args)
        {
            using (ContentModeratorClient client = NewClient())
            {
                // Create a review with the content pointing to a streaming endpoint (manifest)
                var streamingcontent = "https://amssamples.streaming.mediaservices.windows.net/91492735-c523-432b-ba01-faba6c2206a2/AzureMediaServicesPromo.ism/manifest";
                string review_id = CreateReview(client, "review1", streamingcontent);

                var frame1_url = "https://blobthebuilder.blob.core.windows.net/sampleframes/ams-video-frame1-00-17.PNG";
                var frame2_url = "https://blobthebuilder.blob.core.windows.net/sampleframes/ams-video-frame-2-01-04.PNG";
                var frame3_url = "https://blobthebuilder.blob.core.windows.net/sampleframes/ams-video-frame-3-02-24.PNG";

                // Add the frames from 17, 64, and 144 seconds.
                AddFrame(client, review_id, frame1_url, "17");
                AddFrame(client, review_id, frame2_url, "64");
                AddFrame(client, review_id, frame3_url, "144");

                // Get frames information and show
                GetFrames(client, review_id);
                GetReview(client, review_id);

                // Publish the review
                PublishReview(client, review_id);

                Console.WriteLine("Open your Content Moderator Dashboard and select Review > Video to see the review.");
                Console.WriteLine("Press any key to close the application.");
                Console.ReadKey();
            }
        }
    }
}
