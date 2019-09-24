using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Azure.CognitiveServices.Samples.ComputerVision.GetThumbnail
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
                GetThumbnailSample.RunAsync(endpoint, subscriptionKey).Wait(5000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }
    public class GetThumbnailSample
    {
        public static async Task RunAsync(string endpoint, string key)
        {
            ComputerVisionClient computerVision = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            string localImagePath = @"Images\objects.jpg";   // See this repo's readme.md for info on how to get these images. Alternatively, you can just set the path to any appropriate image on your machine.
            string remoteImageUrl = "https://github.com/Azure-Samples/cognitive-services-sample-data-files/raw/master/ComputerVision/Images/faces.jpg";
            string localSavePath = @".";
            const int thumbnailHeight = 60;
            const int thumbnailWidth = 60;

            Console.WriteLine("Getting thumbnails ...");
            await GetThumbnailFromUrlAsync(computerVision, remoteImageUrl, thumbnailWidth, thumbnailHeight, localSavePath);
            await GetThumbnailFromStreamAsync(computerVision, localImagePath, thumbnailWidth, thumbnailHeight, localSavePath);
        }

        // Analyze a remote image
        private static async Task GetThumbnailFromUrlAsync(ComputerVisionClient computerVision, string imageUrl, int height, int width, string localSavePath)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine("\nInvalid remote image url:\n{0} \n", imageUrl);
                return;
            }

            Stream thumbnail = await computerVision.GenerateThumbnailAsync(width, height, imageUrl, smartCropping:true);

            Console.WriteLine(imageUrl);
            
            string imageName = Path.GetFileName(imageUrl);
            string thumbnailFilePath = Path.Combine(localSavePath, imageName.Insert(imageName.Length - 4, "_thumb"));

            SaveThumbnail(thumbnail, thumbnailFilePath);
        }

        // Analyze a local image
        private static async Task GetThumbnailFromStreamAsync(ComputerVisionClient computerVision, string imagePath, int height, int width, string localSavePath)
        {
            if (!File.Exists(imagePath))
            {
                Console.WriteLine("\nUnable to open or read local image path:\n{0} \n", imagePath);
                return;
            }

            using (Stream imageStream = File.OpenRead(imagePath))
            {
                Stream thumbnail = await computerVision.GenerateThumbnailInStreamAsync(width, height, imageStream, smartCropping:true);
                Console.WriteLine(imagePath);

                string imageName = Path.GetFileName(imagePath);
                string thumbnailFilePath = Path.Combine(localSavePath, imageName.Insert(imageName.Length - 4, "_thumb"));

                SaveThumbnail(thumbnail, thumbnailFilePath);
            }
        }
        private static void SaveThumbnail(Stream thumbnail, string thumbnailFilePath)
        {
            Console.WriteLine("Saving image to " + thumbnailFilePath);
            using (Stream file = File.Create(thumbnailFilePath))
            {
                    thumbnail.CopyTo(file);
            }
        }
    }
}
