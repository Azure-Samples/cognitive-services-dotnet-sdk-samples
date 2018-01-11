using Microsoft.CognitiveServices.ContentModerator;
using Microsoft.CognitiveServices.ContentModerator.Models;
using ModeratorHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace CreateReviews
{
    class Program
    {
        /// <summary>
        /// Associates the review ID (assigned by the service) to the internal
        /// content ID of the item.
        /// </summary>
        public class ReviewItem
        {
            /// <summary>
            /// The media type for the item to review.
            /// </summary>
            public string Type;

            /// <summary>
            /// The URL of the item to review.
            /// </summary>
            public string Url;

            /// <summary>
            /// The internal content ID for the item to review.
            /// </summary>
            public string ContentId;

            /// <summary>
            /// The ID that the service assigned to the review.
            /// </summary>
            public string ReviewId;
        }

        /// <summary>
        /// The minimum amount of time, in milliseconds, to wait between calls
        /// to the Image List API.
        /// </summary>
        private const int throttleRate = 2000;

        /// <summary>
        /// The number of seconds to delay after a review has finished before
        /// getting the review results from the server.
        /// </summary>
        private const int latencyDelay = 45;

        /// <summary>
        /// The name of the log file to create.
        /// </summary>
        /// <remarks>Relative paths are ralative the execution directory.</remarks>
        private const string OutputFile = "OutputLog.txt";

        /// <summary>
        /// The name of the team to assign the review to.
        /// </summary>
        /// <remarks>This must be the team name you used to create your 
        /// Content Moderator account. You can retrieve your team name from
        /// the Conent Moderator web site. Your team name is the Id associated 
        /// with your subscription.</remarks>
        private const string TeamName = "testreview6";

        /// <summary>
        /// The optional name of the subteam to assign the review to.
        /// </summary>
        private const string Subteam = null;

        /// <summary>
        /// The callback endpoint for completed reviews.
        /// </summary>
        /// <remarks>Revies show up for reviewers on your team. 
        /// As reviewers complete reviews, results are sent to the
        /// callback endpoint using an HTTP POST request.</remarks>
        private const string CallbackEndpoint = "https://requestb.in/vxke1mvx";

        /// <summary>
        /// The media type for the item to review.
        /// </summary>
        /// <remarks>Valid values are "image", "text", and "video".</remarks>
        private const string MediaType = "image";

        /// <summary>
        /// The URLs of the images to create review jobs for.
        /// </summary>
        private static readonly string[] ImageUrls = new string[] {
            "https://moderatorsampleimages.blob.core.windows.net/samples/sample5.png"
        };

        /// <summary>
        /// The metadata key to initially add to each review item.
        /// </summary>
        private const string MetadataKey = "sc";

        /// <summary>
        /// The metadata value to initially add to each review item.
        /// </summary>
        private const string MetadataValue = "true";

        /// <summary>
        /// A static reference to the text writer to use for logging.
        /// </summary>
        private static TextWriter writer;

        /// <summary>
        /// The cached review information, associating a local content ID
        /// to the created review ID for each item.
        /// </summary>
        private static List<ReviewItem> reviewItems =
            new List<ReviewItem>();

        static void Main(string[] args)
        {
            using (TextWriter outputWriter = new StreamWriter(OutputFile, false))
            {
                writer = outputWriter;
                using (var client = Clients.NewClient())
                {
                    CreateReviews(client);
                    GetReviewDetails(client);

                    Console.WriteLine();
                    Console.WriteLine("Perform manual reviews on the Content Moderator site.");
                    Console.WriteLine("Then, press any key to continue.");
                    Console.ReadKey();

                    Console.WriteLine();
                    Console.WriteLine($"Waiting {latencyDelay} seconds for results to propigate.");
                    Thread.Sleep(latencyDelay * 1000);

                    GetReviewDetails(client);
                }

                writer = null;
                outputWriter.Flush();
                outputWriter.Close();
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Writes a message to the log file, and optionally to the console.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="echo">if set to <c>true</c>, write the message to the console.</param>
        private static void WriteLine(string message = null, bool echo = false)
        {
            writer.WriteLine(message ?? String.Empty);

            if (echo)
            {
                Console.WriteLine(message ?? String.Empty);
            }
        }

        /// <summary>
        /// Create the reviews using the fixed list of images.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        private static void CreateReviews(ContentModeratorClient client)
        {
            WriteLine(null, true);
            WriteLine("Creating reviews for the following images:", true);

            // Create the structure to hold the request body information.
            List<CreateReviewBodyItem> requestInfo =
                new List<CreateReviewBodyItem>();

            // Create some standard metadata to add to each item.
            List<CreateReviewBodyItemMetadataItem> metadata =
                new List<CreateReviewBodyItemMetadataItem>(
                    new CreateReviewBodyItemMetadataItem[] {
                        new CreateReviewBodyItemMetadataItem(
                            MetadataKey, MetadataValue)
                    });

            // Populate the request body information and the initial cached review information.
            for (int i = 0; i < ImageUrls.Length; i++)
            {
                // Cache the local information with which to create the review.
                var itemInfo = new ReviewItem()
                {
                    Type = MediaType,
                    ContentId = i.ToString(),
                    Url = ImageUrls[i],
                    ReviewId = null
                };

                WriteLine($" - {itemInfo.Url}; with id = {itemInfo.ContentId}.", true);

                // Add the item informaton to the request information.
                requestInfo.Add(new CreateReviewBodyItem(
                    itemInfo.Type, itemInfo.Url, itemInfo.ContentId,
                    CallbackEndpoint, metadata));

                // Cache the review creation information.
                reviewItems.Add(itemInfo);
            }

            var reviewResponse = client.Reviews.CreateReviewsWithHttpMessagesAsync(
                "application/json", TeamName, requestInfo);

            // Update the local cache to associate the created review IDs with
            // the associated content.
            var reviewIds = reviewResponse.Result.Body;
            for (int i = 0; i < reviewIds.Count; i++)
            {
                Program.reviewItems[i].ReviewId = reviewIds[i];
            }

            WriteLine(JsonConvert.SerializeObject(
                reviewIds, Formatting.Indented));

            Thread.Sleep(throttleRate);
        }

        /// <summary>
        /// Gets the review details from the server.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        private static void GetReviewDetails(ContentModeratorClient client)
        {
            WriteLine(null, true);
            WriteLine("Getting review details:", true);
            foreach (var item in reviewItems)
            {
                var reviewDetail = client.Reviews.GetReviewWithHttpMessagesAsync(
                    TeamName, item.ReviewId);

                WriteLine(
                    $"Review {item.ReviewId} for item ID {item.ContentId} is " +
                    $"{reviewDetail.Result.Body.Status}.", true);
                WriteLine(JsonConvert.SerializeObject(
                    reviewDetail.Result.Body, Formatting.Indented));

                Thread.Sleep(throttleRate);
            }
        }
    }
}
