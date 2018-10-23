using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Microsoft.Azure.CognitiveServices.Samples.ComputerVision
{
    public static class Program
    {
        static void Main(string[] args)
        {
            // Create a client.
            string endpoint = "ENTER YOUR ENDPOINT HERE";
            string key = "ENTER YOUR KEY HERE";


            RunSampleAsync(endpoint, key).Wait();


            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }


        public static async Task RunSampleAsync(string endpoint, string key)
        {
            IComputerVisionClient client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            // Read image file.
            using (FileStream stream = new FileStream(Path.Combine("Images", "house.jpg"), FileMode.Open))
            {
                // Analyze the image.
                ImageAnalysis result = await client.AnalyzeImageInStreamAsync(
                    stream,
                    new List<VisualFeatureTypes>()
                {
                        VisualFeatureTypes.Description,
                        VisualFeatureTypes.Categories,
                        VisualFeatureTypes.Color,
                        VisualFeatureTypes.Faces,
                        VisualFeatureTypes.ImageType,
                        VisualFeatureTypes.Tags
                });

                Console.WriteLine("The image can be described as: {0}\n", result.Description.Captions.FirstOrDefault()?.Text);

                Console.WriteLine("Tags associated with this image:\nTag\t\tConfidence");
                foreach (var tag in result.Tags)
                {
                    Console.WriteLine("{0}\t\t{1}", tag.Name, tag.Confidence);
                }

                Console.WriteLine("\nThe primary colors of this image are: {0}", string.Join(", ", result.Color.DominantColors));
            }
        }
    }
}
