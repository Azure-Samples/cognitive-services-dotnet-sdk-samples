using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Azure.CognitiveServices.Samples.ComputerVision.OCR
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
                OCRSample.RunAsync(endpoint, subscriptionKey).Wait(5000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }

    public class OCRSample
    {
        public static async Task RunAsync(string endpoint, string key)
        {
            ComputerVisionClient computerVision = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            string localImagePath = @"Images\handwritten_text.jpg";  // See this repo's readme.md for info on how to get these images. Alternatively, you can just set the path to any appropriate image on your machine.
            string remoteImageUrl = "https://github.com/Azure-Samples/cognitive-services-sample-data-files/raw/master/ComputerVision/Images/printed_text.jpg";

            Console.WriteLine("OCR on the images");

            await OCRFromUrlAsync(computerVision, remoteImageUrl);
            await OCRFromStreamAsync(computerVision, localImagePath);
        }

        // Analyze a remote image
        private static async Task OCRFromUrlAsync(ComputerVisionClient computerVision, string imageUrl)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine("\nInvalid remote image url:\n{0} \n", imageUrl);
                return;
            }

            OcrResult analysis = await computerVision.RecognizePrintedTextAsync(true, imageUrl);
            Console.WriteLine(imageUrl);
            DisplayResults(analysis);
        }

        private static async Task OCRFromStreamAsync(ComputerVisionClient computerVision, string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                Console.WriteLine("\nUnable to open or read local image path:\n{0} \n", imagePath);
                return;
            }

            using (Stream imageStream = File.OpenRead(imagePath))
            {
                OcrResult analysis = await computerVision.RecognizePrintedTextInStreamAsync(true, imageStream);
                Console.WriteLine(imagePath);
                DisplayResults(analysis);
            }
        }

        private static void DisplayResults(OcrResult analysis)
        {
            //text
            Console.WriteLine("Text:");
            Console.WriteLine("Language: " + analysis.Language);
            Console.WriteLine("Text Angle: " + analysis.TextAngle);
            Console.WriteLine("Orientation: " + analysis.Orientation);
            Console.WriteLine("Text regions: ");
            foreach (var region in analysis.Regions)
            {
                Console.WriteLine("Region bounding box: " + region.BoundingBox);
                foreach (var line in region.Lines)
                {
                    Console.WriteLine("Line bounding box: " + line.BoundingBox);

                    foreach (var word in line.Words)
                    {
                        Console.WriteLine("Word bounding box: " + word.BoundingBox);
                        Console.WriteLine("Text: " + word.Text);
                    }
                    Console.WriteLine("\n");
                }
                Console.WriteLine("\n \n");
            }
        }
    }
}