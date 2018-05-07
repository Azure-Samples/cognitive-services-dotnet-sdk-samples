using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace CustomVisionSample
{
    class Program
    {
        private static List<string> hemlockImages;
        private static List<string> japaneseCherryImages;
        private static MemoryStream testImage;

        static void Main(string[] args)
        {
            string trainingKey = "<your training key here>";
            string predictionKey = "<your prediction key here>";

            // Create the Api, passing in the training key
            TrainingApi trainingApi = new TrainingApi() { ApiKey = trainingKey };

            // Create a new project
            Console.WriteLine("Creating new project:");
            var project = trainingApi.CreateProjectAsync("My New Project").Result;

            // Make two tags in the new project
            var hemlockTag = trainingApi.CreateTagAsync(project.Id, "Hemlock").Result;
            var japaneseCherryTag = trainingApi.CreateTagAsync(project.Id, "Japanese Cherry").Result;

            // Add some images to the tags
            Console.WriteLine("\tUploading images");
            LoadImagesFromDisk();

            // Images can be uploaded one at a time
            foreach (var image in hemlockImages)
            {
                using (var stream = new MemoryStream(File.ReadAllBytes(image)))
                {
                    trainingApi.CreateImagesFromDataAsync(project.Id, stream, new List<string>() { hemlockTag.Id.ToString() }).Wait();
                }
            }

            // Or uploaded in a single batch 
            var imageFiles = japaneseCherryImages.Select(img => new ImageFileCreateEntry(Path.GetFileName(img), File.ReadAllBytes(img))).ToList();
            trainingApi.CreateImagesFromFilesAsync(project.Id, new ImageFileCreateBatch(imageFiles, new List<Guid>() { japaneseCherryTag.Id })).Wait();

            // Now there are images with tags start training the project
            Console.WriteLine("\tTraining");
            var iteration = trainingApi.TrainProjectAsync(project.Id).Result;

            // The returned iteration will be in progress, and can be queried periodically to see when it has completed
            while (iteration.Status == "Training")
            {
                Thread.Sleep(1000);

                // Re-query the iteration to get it's updated status
                iteration = trainingApi.GetIterationAsync(project.Id, iteration.Id).Result;
            }

            // The iteration is now trained. Make it the default project endpoint
            iteration.IsDefault = true;
            trainingApi.UpdateIterationAsync(project.Id, iteration.Id, iteration).Wait();
            Console.WriteLine("Done!\n");

            // Now there is a trained endpoint, it can be used to make a prediction

            // Create a prediction endpoint, passing in obtained prediction key
            PredictionEndpoint endpoint = new PredictionEndpoint() { ApiKey = predictionKey };

            // Make a prediction against the new project
            Console.WriteLine("Making a prediction:");
            var result = endpoint.PredictImageAsync(project.Id, testImage).Result;

            // Loop over each prediction and write out the results
            foreach (var c in result.Predictions)
            {
                Console.WriteLine($"\t{c.Tag}: {c.Probability:P1}");
            }
            Console.ReadKey();
        }

        private static void LoadImagesFromDisk()
        {
            // this loads the images to be uploaded from disk into memory
            hemlockImages = Directory.GetFiles(Path.Combine("Images", "Hemlock")).ToList();
            japaneseCherryImages = Directory.GetFiles(Path.Combine("Images", "Japanese Cherry")).ToList();
            testImage = new MemoryStream(File.ReadAllBytes(Path.Combine("Images", "Test\\test_image.jpg")));
        }
    }
}
