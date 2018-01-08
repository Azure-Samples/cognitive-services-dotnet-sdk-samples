using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Azure.CognitiveServices.Vision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace ComputerVisionSample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a client.
            string apiKey = "ENTER YOUR KEY HERE";
            IComputerVisionAPI client = new ComputerVisionAPI(new ApiKeyServiceClientCredentials(apiKey));
            client.AzureRegion = AzureRegions.Westcentralus;

            // Read image file.
            using (FileStream stream = new FileStream(Path.Combine("Images", "house.jpg"), FileMode.Open))
            {
                // Analyze the image.
                ImageAnalysis result = client.AnalyzeImageInStreamAsync(
                    stream,
                    new List<VisualFeatureTypes>()
                {
                        VisualFeatureTypes.Description,
                        VisualFeatureTypes.Categories,
                        VisualFeatureTypes.Color,
                        VisualFeatureTypes.Faces,
                        VisualFeatureTypes.ImageType,
                        VisualFeatureTypes.Tags
                }).Result;

                Console.WriteLine("The image can be described as: {0}\n", result.Description.Captions.FirstOrDefault()?.Text);

                Console.WriteLine("Tags associated with this image:\nTag\t\tConfidence");
                foreach(var tag in result.Tags)
                {
                    Console.WriteLine("{0}\t\t{1}", tag.Name, tag.Confidence);
                }

                Console.WriteLine("\nThe primary colors of this image are: {0}", string.Join(", ", result.Color.DominantColors));
            }

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }
}
