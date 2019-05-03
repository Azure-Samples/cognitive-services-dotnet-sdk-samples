using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImageOCR
{
    class OCR
    {
        private const string subscriptionKey = "<your training key here>"; //Insert your Cognitive Service subscription key here

        static void Main(string[] args)
        {
            ComputerVisionClient computerVision = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey))
            {
                // You must use the same Azure region that you generated your subscription keys for.  Free trial subscription keys are generated in the westus region. 
                Endpoint = "https://westus.api.cognitive.microsoft.com"
            };

            string localImagePath = @"Images\handwritten_text.jpg";  // See this repo's readme.md for info on how to get these images. Alternatively, you can just set the path to any appropriate image on your machine.
            string remoteImageUrl = "https://github.com/Azure-Samples/cognitive-services-sample-data-files/raw/master/ComputerVision/Images/printed_text.jpg";

            Console.WriteLine("OCR on the images");

            var t1 = OCRFromUrlAsync(computerVision, remoteImageUrl);
            var t2 = OCRFromStreamAsync(computerVision, localImagePath);

            Task.WhenAll(t1, t2).Wait(5000);

            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
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