using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Azure.CognitiveServices.Samples.ComputerVision.TagImage
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
                TagImageSample.RunAsync(endpoint, subscriptionKey).Wait(5000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }
    public class TagImageSample
    {
        public static async Task RunAsync(string endpoint, string key)
        {
            ComputerVisionClient computerVision = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            string localImagePath = @"Images\objects.jpg";   // See this repo's readme.md for info on how to get these images. Alternatively, you can just set the path to any appropriate image on your machine.
            string remoteImageUrl = "https://github.com/Azure-Samples/cognitive-services-sample-data-files/raw/master/ComputerVision/Images/faces.jpg";

            Console.WriteLine("Image tags");
            await TagImageFromUrlAsync(computerVision, remoteImageUrl);
            await TagImageFromStreamAsync(computerVision, localImagePath);
        }

        // Analyze a remote image
        private static async Task TagImageFromUrlAsync(ComputerVisionClient computerVision, string imageUrl)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine("\nInvalid remote image url:\n{0} \n", imageUrl);
                return;
            }
            
            TagResult tags = await computerVision.TagImageAsync(imageUrl);

            Console.WriteLine(imageUrl);
            DisplayTags(tags);
        }

        // Analyze a local image
        private static async Task TagImageFromStreamAsync(ComputerVisionClient computerVision, string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                Console.WriteLine("\nUnable to open or read local image path:\n{0} \n", imagePath);
                return;
            }

            using (Stream imageStream = File.OpenRead(imagePath))
            {
                TagResult tags = await computerVision.TagImageInStreamAsync(imageStream);
                Console.WriteLine(imagePath);
                DisplayTags(tags);
            }
        }
        private static void DisplayTags(TagResult tags)
        { 
            foreach (var tag in tags.Tags)
            {
                Console.WriteLine("{0}\twith confidence {1}", tag.Name, tag.Confidence);
            }
            Console.WriteLine("\n");
        }
    }
}
