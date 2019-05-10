using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Azure.CognitiveServices.Samples.ComputerVision.ExtractText
{
    using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
    using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

    class Program
    {
        public const string subscriptionKey = "<your training key here>"; //Insert your Cognitive Services subscription key here
        public const string endpoint = "https://westus.api.cognitive.microsoft.com"; // You must use the same Azure region that you generated your subscription keys for.  Free trial subscription keys are generated in the westus region. 

        static void Main(string[] args)
        {
            try
            {
                ExtractTextSample.RunAsync(endpoint, subscriptionKey).Wait(5000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }
    public class ExtractTextSample
    {
        public static async Task RunAsync(string endpoint, string key)
        {
            ComputerVisionClient computerVision = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };
            const int numberOfCharsInOperationId = 36;

            string localImagePath = @"Images\handwritten_text.jpg";  // See this repo's readme.md for info on how to get these images. Alternatively, you can just set the path to any appropriate image on your machine.
            string remoteImageUrl = "https://github.com/Azure-Samples/cognitive-services-sample-data-files/raw/master/ComputerVision/Images/printed_text.jpg";

            Console.WriteLine("Text being extracted ...");
            await ExtractTextFromUrlAsync(computerVision, remoteImageUrl, numberOfCharsInOperationId);
            await ExtractTextFromStreamAsync(computerVision, localImagePath, numberOfCharsInOperationId);
        }

        // Read text from a remote image
        private static async Task ExtractTextFromUrlAsync(ComputerVisionClient computerVision, string imageUrl, int numberOfCharsInOperationId)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine("\nInvalid remote image url:\n{0} \n", imageUrl);
                return;
            }

            // For handwritten text, change to TextRecognitionMode.Handwritten
            TextRecognitionMode textRecognitionMode = TextRecognitionMode.Printed;

            // Start the async process to read the text
            BatchReadFileHeaders textHeaders = await computerVision.BatchReadFileAsync(imageUrl, textRecognitionMode);
            await GetTextAsync(computerVision, textHeaders.OperationLocation, numberOfCharsInOperationId);
        }

        // Recognize text from a local image
        private static async Task ExtractTextFromStreamAsync(ComputerVisionClient computerVision, string imagePath, int numberOfCharsInOperationId)
        {
            if (!File.Exists(imagePath))
            {
                Console.WriteLine("\nUnable to open or read local image path:\n{0} \n", imagePath);
                return;
            }

            // For printed text, change to TextRecognitionMode.Printed
            TextRecognitionMode textRecognitionMode = TextRecognitionMode.Handwritten;

            using (Stream imageStream = File.OpenRead(imagePath))
            {
                // Start the async process to recognize the text
                BatchReadFileInStreamHeaders textHeaders = await computerVision.BatchReadFileInStreamAsync(imageStream, textRecognitionMode);
                await GetTextAsync(computerVision, textHeaders.OperationLocation, numberOfCharsInOperationId);
            }
        }

        // Retrieve the recognized text
        private static async Task GetTextAsync(ComputerVisionClient computerVision, string operationLocation, int numberOfCharsInOperationId)
        {
            // Retrieve the URI where the recognized text will be
            // stored from the Operation-Location header
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            ReadOperationResult result = await computerVision.GetReadOperationResultAsync(operationId);

            // Wait for the operation to complete
            int i = 0;
            int maxRetries = 10;
            while ((result.Status == TextOperationStatusCodes.Running ||
                    result.Status == TextOperationStatusCodes.NotStarted) && i++ < maxRetries)
            {
                Console.WriteLine("Server status: {0}, waiting {1} seconds...", result.Status, i);
                await Task.Delay(1000);
                result = await computerVision.GetReadOperationResultAsync(operationId);
            }

            // Display the results
            Console.WriteLine();
            var recResults = result.RecognitionResults;
            foreach (TextRecognitionResult recResult in recResults)
            {
                foreach (Line line in recResult.Lines)
                {
                    Console.WriteLine(line.Text);
                }
            }
            Console.WriteLine();
        }
    }
}