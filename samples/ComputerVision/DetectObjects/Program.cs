using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DetectObjects
{
    class DetectObjects
    {
        private const string subscriptionKey = "<your training key here>"; //Insert your Cognitive Service subscription key here

        static void Main(string[] args)
        {
            ComputerVisionClient computerVision = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey));

            // You must use the same Azure region that you generated your subscription keys for.  Free trial subscription keys are generated in the westus region. 
            computerVision.Endpoint = "https://westus.api.cognitive.microsoft.com";

            string localImagePath = @"Images\objects.jpg";   // See this repo's readme.md for info on how to get these images. Alternatively, you can just set the path to any appropriate image on your machine.
            string remoteImageUrl = "https://github.com/Azure-Samples/cognitive-services-sample-data-files/raw/master/ComputerVision/Images/faces.jpg";

            Console.WriteLine("Objects being detected ...");
            var t1 = DetectObjectsFromUrlAsync(computerVision, remoteImageUrl);
            var t2 = DetectObjectsFromStreamAsync(computerVision, localImagePath);

            Task.WhenAll(t1, t2).Wait(5000);
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }

        // Analyze a remote image
        private static async Task DetectObjectsFromUrlAsync(ComputerVisionClient computerVision, string imageUrl)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine("\nInvalid remote image url:\n{0} \n", imageUrl);
                return;
            }

            DetectResult analysis = await computerVision.DetectObjectsAsync(imageUrl);
            
            Console.WriteLine(imageUrl);
            DisplayObjects(analysis);
        }

        // Analyze a local image
        private static async Task DetectObjectsFromStreamAsync(ComputerVisionClient computerVision, string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                Console.WriteLine("\nUnable to open or read local image path:\n{0} \n", imagePath);
                return;
            }

            using (Stream imageStream = File.OpenRead(imagePath))
            {
                DetectResult analysis = await computerVision.DetectObjectsInStreamAsync(imageStream);
                Console.WriteLine(imagePath);
                DisplayObjects(analysis);
            }
        }
        private static void DisplayObjects(DetectResult analysis)
        {
            Console.WriteLine("Objects:");
            foreach (var obj in analysis.Objects)
            {
                Console.WriteLine("{0} with confidence {1} at location {2},{3},{4},{5}",
                    obj.ObjectProperty, obj.Confidence,
                    obj.Rectangle.X, obj.Rectangle.X + obj.Rectangle.W,
                    obj.Rectangle.Y, obj.Rectangle.Y + obj.Rectangle.H);
            }
            Console.WriteLine("\n");
        }
    }
}