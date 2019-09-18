using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Azure.CognitiveServices.Samples.ComputerVision.DomainSpecificContent
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
                DomainSpecificContentSample.RunAsync(endpoint, subscriptionKey).Wait(5000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }
    public class DomainSpecificContentSample
    {
        public static async Task RunAsync(string endpoint, string key)
        {
            ComputerVisionClient computerVision = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            string localImagePath = @"Images\celebrities.jpg";   // See this repo's readme.md for info on how to get these images. Alternatively, you can just set the path to any appropriate image on your machine.
            string remoteImageUrl = "https://github.com/Azure-Samples/cognitive-services-sample-data-files/raw/master/ComputerVision/Images/landmark.jpg";

            Console.WriteLine("Domain Specific Content being analyzed ...");
            await RecognizeDomainSpecificContentFromUrlAsync(computerVision, remoteImageUrl, "landmarks");
            await RecognizeDomainSpecificContentFromStreamAsync(computerVision, localImagePath, "celebrities");
        }

        // Analyze a remote image
        private static async Task RecognizeDomainSpecificContentFromUrlAsync(ComputerVisionClient computerVision, string imageUrl, string specificDomain)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine("\nInvalid remote image url:\n{0} \n", imageUrl);
                return;
            }
            
            DomainModelResults analysis = await computerVision.AnalyzeImageByDomainAsync(specificDomain, imageUrl);  //change the first parameter to "landmarks" if that is the domain you are interested in

            Console.WriteLine(imageUrl);
            DisplayResults(analysis); 
        }

        // Analyze a local image
        private static async Task RecognizeDomainSpecificContentFromStreamAsync(ComputerVisionClient computerVision, string imagePath, string specificDomain)
        {
            if (!File.Exists(imagePath))
            {
                Console.WriteLine("\nUnable to open or read local image path:\n{0} \n", imagePath);
                return;
            }

            using (Stream imageStream = File.OpenRead(imagePath))
            {
                DomainModelResults analysis = await computerVision.AnalyzeImageByDomainInStreamAsync(specificDomain, imageStream);  //change "celebrities" to "landmarks" if that is the domain you are interested in
                Console.WriteLine(imagePath);
                DisplayResults(analysis);
            }
        } 
        private static void DisplayResults(DomainModelResults analysis)
        { 
            Console.WriteLine("Results:");
            Console.WriteLine(analysis.Result); 
        }
    }
}
