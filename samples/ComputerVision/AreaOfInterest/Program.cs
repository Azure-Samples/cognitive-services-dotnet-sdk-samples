using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Azure.CognitiveServices.Samples.ComputerVision.AreaOfInterest
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
                AreaOfInterestSample.RunAsync(endpoint, subscriptionKey).Wait(5000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }
    public class AreaOfInterestSample
    {
        public static async Task RunAsync(string endpoint, string key)
        {
            ComputerVisionClient computerVision = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            // See this repo's README.md on how to get these images. 
            // Alternatively, you can just set the path to any appropriate image on your machine.
            string localImagePath = @"Images\objects.jpg";   
            string remoteImageUrl = "https://github.com/Azure-Samples/cognitive-services-sample-data-files/raw/master/ComputerVision/Images/faces.jpg";

            Console.WriteLine("area of interest being found ...");
            await GetAreaOfInterestFromUrlAsync(computerVision, remoteImageUrl);
            await GetAreaOfInterestFromStreamAsync(computerVision, localImagePath);
        }

        // Analyze a remote image
        private static async Task GetAreaOfInterestFromUrlAsync(ComputerVisionClient computerVision, string imageUrl)  
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine("\nInvalid remote image url:\n{0} \n", imageUrl);
                return;
            }

            AreaOfInterestResult analysis = await computerVision.GetAreaOfInterestAsync(imageUrl);

            Console.WriteLine(imageUrl);
            DisplayAreaOfInterest(analysis);
        }

        // Analyze a local image
        private static async Task GetAreaOfInterestFromStreamAsync(ComputerVisionClient computerVision, string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                Console.WriteLine("\nUnable to open or read local image path:\n{0} \n", imagePath);
                return;
            }

            using (Stream imageStream = File.OpenRead(imagePath))
            {
                AreaOfInterestResult analysis = await computerVision.GetAreaOfInterestInStreamAsync(imageStream);
                Console.WriteLine(imagePath);
                DisplayAreaOfInterest(analysis);
            }
        }
        private static void DisplayAreaOfInterest(AreaOfInterestResult analysis)
        {
            Console.WriteLine("The Area of Interest is at location {0},{1},{2},{3}",
                analysis.AreaOfInterest.X, analysis.AreaOfInterest.X+analysis.AreaOfInterest.W,
                analysis.AreaOfInterest.Y, analysis.AreaOfInterest.Y+analysis.AreaOfInterest.H);
            Console.WriteLine("\n");
        }
    }
}
