using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.CognitiveServices.ContentModerator;
using Microsoft.CognitiveServices.ContentModerator.Models;
using Newtonsoft.Json;
using System.Text;

namespace VideoTranscriptReviews
{
    class Program
    {   
        /// <summary>
        /// Your Content Moderator subscription key.
        /// Add your Azure Face subscription key to your environment variables.
        /// </summary>
        private static readonly string CMSubscriptionKey = 
            Environment.GetEnvironmentVariable("FACE_SUBSCRIPTION_KEY");

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
        /// Add a transcript to the indicated video review.
        /// The transcript must be in the WebVTT format.
        /// For more information, see the API reference:
        /// https://westus2.dev.cognitive.microsoft.com/docs/services/580519463f9b070e5c591178/operations/59e7b8b2e7151f0b10d451fe
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="review_id">The video review ID.</param>
        /// <param name="transcript">The video transcript.</param>
        static void AddTranscript(ContentModeratorClient client, string review_id, string transcript)
        {
            Console.WriteLine("Adding a transcript to the review with ID {0}.", review_id);

            client.Reviews.AddVideoTranscript(TeamName, review_id, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(transcript)));

            Thread.Sleep(throttleRate);
        }

        /// <summary>
        /// Add the results of moderating a video transcript to the indicated video review.
        /// For more information, see the API reference:
        /// https://westus2.dev.cognitive.microsoft.com/docs/services/580519463f9b070e5c591178/operations/59e7b93ce7151f0b10d451ff
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="review_id">The video review ID.</param>
        /// <param name="transcript">The video transcript.</param>
        static void AddTranscriptModerationResult(ContentModeratorClient client, string review_id, string transcript)
        {
            Console.WriteLine("Adding a transcript moderation result to the review with ID {0}.", review_id);

            // Screen the transcript using the Text Moderation API. For more information, see:
            // https://westus2.dev.cognitive.microsoft.com/docs/services/57cf753a3f9b070c105bd2c1/operations/57cf753a3f9b070868a1f66f
            Screen screen = client.TextModeration.ScreenText("text/plain", new MemoryStream(Encoding.UTF8.GetBytes(transcript)), "eng");

            // Map the term list returned by ScreenText into a term list we can pass to AddVideoTranscriptModerationResult.
            List<TranscriptModerationBodyItemTermsItem> terms = new List<TranscriptModerationBodyItemTermsItem>();
            if (null != screen.Terms)
            {
                foreach (var term in screen.Terms)
                {
                    if (term.Index.HasValue)
                    {
                        terms.Add(new TranscriptModerationBodyItemTermsItem(term.Index.Value, term.Term));
                    }
                }
            }

            List<TranscriptModerationBodyItem> body = new List<TranscriptModerationBodyItem>()
            {
                new TranscriptModerationBodyItem()
                {
                    Timestamp = "0",
                    Terms = terms
                }
            };

            client.Reviews.AddVideoTranscriptModerationResult("application/json", TeamName, review_id, body);

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

                var transcript = @"WEBVTT

                01:01.000 --> 02:02.000
                First line with a crap word in a transcript.

                02:03.000 --> 02:25.000
                This is another line in the transcript.
                ";

                AddTranscript(client, review_id, transcript);

                AddTranscriptModerationResult(client, review_id, transcript);

                // Publish the review
                PublishReview(client, review_id);

                Console.WriteLine("Open your Content Moderator Dashboard and select Review > Video to see the review.");
                Console.WriteLine("Press any key to close the application.");
                Console.ReadKey();
            }
        }
    }
}
