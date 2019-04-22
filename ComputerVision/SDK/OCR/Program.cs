﻿using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImageOCR
{
    class Program
    {
        private const string subscriptionKey = "0123456789abcdef0123456789ABCDEF"; //replace with your obtained key

        private const string remoteImageUrl ="https://github.com/harishkrishnav/cognitive-services-dotnet-sdk-samples/raw/master/ComputerVision/Images/sample0.png";

        static void Main(string[] args)
        {
            ComputerVisionClient computerVision = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey));

            // You must use the same region as you used to get your subscription
            // keys. For example, if you got your subscription keys from westus,
            // replace "westcentralus" with "westus".

            // Specify the Azure region
            computerVision.Endpoint = "https://westus.api.cognitive.microsoft.com";

            Console.WriteLine("Images being analyzed ...");
            OCRFromUrlAsync(computerVision, remoteImageUrl).Wait(5000);
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }

        // Analyze a remote image
        
        private static async Task OCRFromUrlAsync(ComputerVisionClient computerVision, string imageUrl)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine(
                    "\nInvalid remote image url:\n{0} \n", imageUrl);
                return;
            }

            OcrResult analysis = await computerVision.RecognizePrintedTextAsync(true, imageUrl);
            Console.WriteLine(imageUrl);
            DisplayResults(analysis);
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