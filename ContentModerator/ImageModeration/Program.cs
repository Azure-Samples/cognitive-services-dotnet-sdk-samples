using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.CognitiveServices.ContentModerator;
using Microsoft.CognitiveServices.ContentModerator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ImageModeration
{
    class Program
    {
        /// <summary>
        /// Contains the image moderation results for an image, including 
        /// text and face detection results.
        /// </summary>
        public class EvaluationData
        {
            /// <summary>
            /// The URL of the evaluated image.
            /// </summary>
            public string ImageUrl;

            /// <summary>
            /// The image moderation results.
            /// </summary>
            public Evaluate ImageModeration;

            /// <summary>
            /// The text detection results.
            /// </summary>
            public OCR TextDetection;

            /// <summary>
            /// The face detection results;
            /// </summary>
            public FoundFaces FaceDetection;
        }

        /// <summary>
        /// The name of the file that contains the image URLs to evaluate.
        /// </summary>
        /// <remarks>You will need to create an input file and update this path
        /// accordingly. Relative paths are relative to the execution directory.</remarks>
        private static string ImageUrlFile = "ImageFiles.txt";

        /// <summary>
        /// The name of the file to contain the output from the evaluation.
        /// </summary>
        /// <remarks>Relative paths are relative to the execution directory.</remarks>
        private static string OutputFile = "ModerationOutput.json";

        static void Main(string[] args)
        {
            // Create an object in which to store the image moderation results.
            List<EvaluationData> evaluationData = new List<EvaluationData>();

            // Create an instance of the Content Moderator API wrapper.
            using (var client = Clients.NewClient())
            {
                // Read image URLs from the input file and evaluate each one.
                using (StreamReader inputReader = new StreamReader(ImageUrlFile))
                {
                    while (!inputReader.EndOfStream)
                    {
                        string line = inputReader.ReadLine().Trim();
                        if (line != String.Empty)
                        {
                            Console.WriteLine("Evaluating {0}", line);
                            EvaluationData imageData = EvaluateImage(client, line);
                            evaluationData.Add(imageData);
                        }
                    }
                }
            }

            // Save the moderation results to a file.
            using (StreamWriter outputWriter = new StreamWriter(OutputFile, false))
            {
                outputWriter.WriteLine(JsonConvert.SerializeObject(
                    evaluationData, Formatting.Indented));

                outputWriter.Flush();
                outputWriter.Close();
            }

            Console.WriteLine("Results written to {0}", OutputFile);
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }

        /// <summary>
        /// Evaluates an image using the Image Moderation APIs.
        /// </summary>
        /// <param name="client">The Content Moderator API wrapper to use.</param>
        /// <param name="imageUrl">The URL of the image to evaluate.</param>
        /// <returns>Aggregated image moderation results for the image.</returns>
        /// <remarks>This method throttles calls to the API.
        /// Your Content Moderator service key will have a requests per second (RPS)
        /// rate limit, and the SDK will throw an exception with a 429 error code 
        /// if you exceed that limit. A free tier key has a 1 RPS rate limit.
        /// </remarks>
        private static EvaluationData EvaluateImage(
            ContentModeratorClient client, string imageUrl)
        {
            var url = new BodyModel("URL", imageUrl.Trim());

            var imageData = new EvaluationData();

            imageData.ImageUrl = url.Value;

            // Evaluate for adult and racy content.
            imageData.ImageModeration =
                client.ImageModeration.EvaluateUrlInput("application/json", url, true);
            Thread.Sleep(1000);

            // Detect and extract text.
            imageData.TextDetection =
                client.ImageModeration.OCRUrlInput("eng", "application/json", url, true);
            Thread.Sleep(1000);

            // Detect faces.
            imageData.FaceDetection =
                client.ImageModeration.FindFacesUrlInput("application/json", url, true);
            Thread.Sleep(1000);

            return imageData;
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
        //  Add your Azure Content Moderator endpoint to your environment variables.
        /// </summary>
        private static readonly string AzureBaseURL =
            Environment.GetEnvironmentVariable("CONTENT_MODERATOR_ENDPOINT");

        /// <summary>
        /// Your Content Moderator subscription key.
        /// Add your Azure Content Moderator subscription key to your environment variables.
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
