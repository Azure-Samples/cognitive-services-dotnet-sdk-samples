using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Azure.CognitiveServices.Samples.ComputerVision.RecognizeText
{
    using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
    using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

    class Program
    {
        static void Main(string[] args)
        {
            // Add your Computer Vision subscription key and endpoint to your environment variables
            string subscriptionKey = Environment.GetEnvironmentVariable("COMPUTER_VISION_SUBSCRIPTION_KEY"); 
            string endpoint = Environment.GetEnvironmentVariable("COMPUTER_VISION_ENDPOINT");

            try
            {
                RecognizeTextSample.RunAsync(endpoint, subscriptionKey).Wait(5000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }
    public class RecognizeTextSample
    {
        public static async Task RunAsync(string endpoint, string key)
        {
            ComputerVisionClient computerVision = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };
            const int numberOfCharsInOperationId = 36;               //This is not a tune-able hyperparameter

            string localImagePath = @"Images\handwritten_text.jpg";  // See this repo's readme.md for info on how to get these images. Alternatively, you can just set the path to any appropriate image on your machine.
            string remoteImageUrl = "https://github.com/Azure-Samples/cognitive-services-sample-data-files/raw/master/ComputerVision/Images/printed_text.jpg";

            Console.WriteLine("Text being recognized ...");
            await RecognizeTextFromStreamAsync(computerVision, localImagePath, numberOfCharsInOperationId, TextRecognitionMode.Handwritten);  //Use TextRecognitionMode.Printed for printed text
            await RecognizeTextFromUrlAsync(computerVision, remoteImageUrl, numberOfCharsInOperationId, TextRecognitionMode.Printed);
        }

        // Read text from a remote image
        private static async Task RecognizeTextFromUrlAsync(ComputerVisionClient computerVision, string imageUrl, int numberOfCharsInOperationId, TextRecognitionMode textRecognitionMode)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine("\nInvalid remote image url:\n{0} \n", imageUrl);
                return;
            }

            // Start the async process to read the text
            RecognizeTextHeaders textHeaders = await computerVision.RecognizeTextAsync(imageUrl, textRecognitionMode);
            await GetTextAsync(computerVision, textHeaders.OperationLocation, numberOfCharsInOperationId);
        }

        // Recognize text from a local image
        private static async Task RecognizeTextFromStreamAsync(ComputerVisionClient computerVision, string imagePath, int numberOfCharsInOperationId, TextRecognitionMode textRecognitionMode)
        {
            if (!File.Exists(imagePath))
            {
                Console.WriteLine("\nUnable to open or read local image path:\n{0} \n", imagePath);
                return;
            }

            using (Stream imageStream = File.OpenRead(imagePath))
            {
                // Start the async process to recognize the text
                RecognizeTextInStreamHeaders textHeaders = await computerVision.RecognizeTextInStreamAsync(imageStream, textRecognitionMode);
                await GetTextAsync(computerVision, textHeaders.OperationLocation, numberOfCharsInOperationId);
            }
        }

        // Retrieve the recognized text
        private static async Task GetTextAsync(ComputerVisionClient computerVision, string operationLocation, int numberOfCharsInOperationId)
        {
            // Retrieve the URI where the recognized text will be
            // stored from the Operation-Location header
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            TextOperationResult result = await computerVision.GetTextOperationResultAsync(operationId);
            // Wait for the operation to complete
            int i = 0;
            int maxRetries = 10;
            while ((result.Status == TextOperationStatusCodes.Running ||
                    result.Status == TextOperationStatusCodes.NotStarted) && i++ < maxRetries)
            {
                Console.WriteLine("Server status: {0}, waiting {1} seconds...", result.Status, i);
                await Task.Delay(1000);
                result = await computerVision.GetTextOperationResultAsync(operationId);
            }

            // Display the results
            Console.WriteLine();
            var recResults = result.RecognitionResult;
            foreach (Line line in recResults.Lines)
            {
                foreach (Word word in line.Words)
                {
                    Console.WriteLine("{0} at location {1}, {2}, {3}, {4}", word.Text,
                        word.BoundingBox[0], word.BoundingBox[1], word.BoundingBox[2], word.BoundingBox[3]);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
