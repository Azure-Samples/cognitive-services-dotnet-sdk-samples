using Microsoft.CognitiveServices.ContentModerator;
using Microsoft.CognitiveServices.ContentModerator.Models;
using ModeratorHelper;
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
        /// accordingly. Relative paths are ralative the execution directory.</remarks>
        private static string ImageUrlFile = "ImageFiles.txt";

        /// <summary>
        /// The name of the file to contain the output from the evaluation.
        /// </summary>
        /// <remarks>Relative paths are ralative the execution directory.</remarks>
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
}
