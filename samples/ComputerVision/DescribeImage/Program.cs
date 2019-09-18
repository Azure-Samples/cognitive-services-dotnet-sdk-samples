using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Azure.CognitiveServices.Samples.ComputerVision.DescribeImage
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
                DescribeImageSample.RunAsync(endpoint, subscriptionKey).Wait(5000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }
    public class DescribeImageSample
    {
        public static async Task RunAsync(string endpoint, string key)
        {
            ComputerVisionClient computerVision = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            string localImagePath = @"Images\objects.jpg";   // See this repo's readme.md for info on how to get these images. Alternatively, you can just set the path to any appropriate image on your machine.
            string remoteImageUrl = "https://github.com/Azure-Samples/cognitive-services-sample-data-files/raw/master/ComputerVision/Images/celebrities.jpg";

            Console.WriteLine("Image description");
            await DescribeImageFromUrlAsync(computerVision, remoteImageUrl);
            await DescribeImageFromStreamAsync(computerVision, localImagePath);
        }

        // Analyze a remote image
        private static async Task DescribeImageFromUrlAsync(ComputerVisionClient computerVision, string imageUrl)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine("\nInvalid remote image url:\n{0} \n", imageUrl);
                return;
            }

            ImageDescription descriptions = await computerVision.DescribeImageAsync(imageUrl);

            Console.WriteLine(imageUrl);
            DisplayDescriptions(descriptions);
        }

        // Analyze a local image
        private static async Task DescribeImageFromStreamAsync(ComputerVisionClient computerVision, string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                Console.WriteLine("\nUnable to open or read local image path:\n{0} \n", imagePath);
                return;
            }

            using (Stream imageStream = File.OpenRead(imagePath))
            {
                ImageDescription descriptions = await computerVision.DescribeImageInStreamAsync(imageStream);
                Console.WriteLine(imagePath);
                DisplayDescriptions(descriptions);
            }
        }
        private static void DisplayDescriptions(ImageDescription descriptions)
        {
            foreach (var description in descriptions.Captions)
            {
                Console.WriteLine("{0}\t\t with confidence {1}", description.Text, description.Confidence);
            }
            Console.WriteLine("\n");
        }
    }
}
