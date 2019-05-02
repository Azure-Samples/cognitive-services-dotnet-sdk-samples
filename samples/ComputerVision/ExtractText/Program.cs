using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

using System;
using System.IO;
using System.Threading.Tasks;

namespace ExtractText
{
    class ExtractText
    {
        private const string subscriptionKey = "<your training key here>"; //Insert your Cognitive Service subscription key here

        // For printed text, change to TextRecognitionMode.Printed
        private const TextRecognitionMode textRecognitionMode = TextRecognitionMode.Handwritten;

        private const int numberOfCharsInOperationId = 36;

        static void Main(string[] args)
        {
            ComputerVisionClient computerVision = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey));

            // You must use the same Azure region that you generated your subscription keys for.  Free trial subscription keys are generated in the westus region. 
            computerVision.Endpoint = "https://westus.api.cognitive.microsoft.com";

            string localImagePath = @"Images\handwritten_text.jpg";  // localImagePath = @"C:\Documents\LocalImage.jpg"
            // See this repo's readme.md for info on how to get these images. Alternatively, you can just set the path to any appropriate image on your machine.
            string remoteImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/d/dd/Cursive_Writing_on_Notebook_paper.jpg/800px-Cursive_Writing_on_Notebook_paper.jpg";

            Console.WriteLine("Text being extracted ...");
            var t1 = ExtractTextFromUrlAsync(computerVision, remoteImageUrl);
            var t2 = ExtractTextFromStreamAsync(computerVision, localImagePath);

            Task.WhenAll(t1, t2).Wait(5000);
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }

        // Read text from a remote image
        private static async Task ExtractTextFromUrlAsync(ComputerVisionClient computerVision, string imageUrl)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine("\nInvalid remote image url:\n{0} \n", imageUrl);
                return;
            }

            // Start the async process to read the text
            BatchReadFileHeaders textHeaders = await computerVision.BatchReadFileAsync(imageUrl, textRecognitionMode);
            await GetTextAsync(computerVision, textHeaders.OperationLocation);
        }

        // Recognize text from a local image
        private static async Task ExtractTextFromStreamAsync(ComputerVisionClient computerVision, string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                Console.WriteLine("\nUnable to open or read local image path:\n{0} \n", imagePath);
                return;
            }

            using (Stream imageStream = File.OpenRead(imagePath))
            {
                // Start the async process to recognize the text
                BatchReadFileInStreamHeaders textHeaders = await computerVision.BatchReadFileInStreamAsync(imageStream, textRecognitionMode);
                await GetTextAsync(computerVision, textHeaders.OperationLocation);
            }
        }

        // Retrieve the recognized text
        private static async Task GetTextAsync(ComputerVisionClient computerVision, string operationLocation)
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