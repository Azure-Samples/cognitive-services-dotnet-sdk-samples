namespace Microsoft.Azure.CognitiveServices.Samples.Face
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.Vision.Face;
    using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

    public static class IdentifyInPersonGroup
    {
        public static async Task Run(string endpoint, string key)
        {
            Console.WriteLine("Sample of Identify faces in person group.");

            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            Dictionary<string, string[]> targetImageFileDictionary = new Dictionary<string, string[]>
            {
                { "Family1-Dad", new[] { "Family1-Dad1.jpg", "Family1-Dad2.jpg" } },
                { "Family1-Mom", new[] { "Family1-Mom1.jpg", "Family1-Mom2.jpg" } },
                { "Family1-Son", new[] { "Family1-Son1.jpg", "Family1-Son2.jpg" } },
                { "Family1-Daughter", new[] { "Family1-Daughter1.jpg", "Family1-Daughter2.jpg" } },
                { "Family2-Lady", new[] { "Family2-Lady1.jpg", "Family2-Lady2.jpg" } },
                { "Family2-Man", new[] { "Family2-Man1.jpg", "Family2-Man2.jpg" } }
            };
            string sourceImageFileName = "identification1.jpg";

            // Create a person group.
            string personGroupId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create a person group ({personGroupId}).");
            await client.PersonGroup.CreateAsync(personGroupId, personGroupId);

            foreach (var targetImageFileDictionaryName in targetImageFileDictionary.Keys)
            {
                // Create a person group person.
                Person p = new Person { Name = targetImageFileDictionaryName, UserData = "Person for sample" };
                p.PersonId = (await client.PersonGroupPerson.CreateAsync(personGroupId, p.Name)).PersonId;
                Console.WriteLine($"Create a person group person '{p.Name}'.");

                foreach (var targetImageFileName in targetImageFileDictionary[targetImageFileDictionaryName])
                {
                    // Read target image file. 
                    using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileDictionary[targetImageFileDictionaryName][0]), FileMode.Open))
                    {
                        // Add face to the person group person.
                        Console.WriteLine($"Add face to the person group person({targetImageFileDictionaryName}) from image `{targetImageFileName}`.");
                        PersistedFace face = await client.PersonGroupPerson.AddFaceFromStreamAsync(personGroupId, p.PersonId, stream, targetImageFileName);

                        if (face == null)
                        {
                            throw new Exception($"No persisted face from image `{targetImageFileName}`.");
                        }
                    }
                }
            }

            // Start to train the person group.
            Console.WriteLine($"Train person group {personGroupId}.");
            await client.PersonGroup.TrainAsync(personGroupId);

            // Wait until the training is completed.
            while (true)
            {
                await Task.Delay(1000);
                var trainingStatus = await client.PersonGroup.GetTrainingStatusAsync(personGroupId);
                Console.WriteLine($"Training status is {trainingStatus.Status}.");
                if (trainingStatus.Status != TrainingStatusType.Running)
                {
                    if (trainingStatus.Status == TrainingStatusType.Failed)
                    {
                        throw new Exception($"Training failed with message {trainingStatus.Message}.");
                    }

                    break;
                }
            }

            List<Guid> sourceFaceIds = new List<Guid>();

            // Add detected faceId to sourceFaceIds.
            List<DetectedFace> detectedFaces = await Common.DetectedFace(client, Path.Combine("Images", sourceImageFileName));
            sourceFaceIds.Add(detectedFaces[0].FaceId.Value);

            // Identify example of identifying faces towards person group. 
            var identifyResults = await client.Face.IdentifyAsync(sourceFaceIds, personGroupId);
            if (identifyResults == null)
            {
                Console.WriteLine($"No people found in the group the same as the {sourceImageFileName}.");
                return;
            }

            foreach (var identifyResult in identifyResults)
            {
                Person person = await client.PersonGroupPerson.GetAsync(personGroupId, identifyResult.Candidates[0].PersonId);
                Console.WriteLine($"Person '{person.Name}' is identified for faces in {sourceImageFileName}, confidence: {identifyResult.Candidates[0].Confidence}.");
            }

            // Delete the person group.
            await client.PersonGroup.DeleteAsync(personGroupId);
            Console.WriteLine($"Delete the person group {personGroupId}.");
            Console.WriteLine();
        }
    }

    public static class IdentifyInLargePersonGroup
    {
        public static async Task Run(string endpoint, string key)
        {
            Console.WriteLine("Sample of Identify faces in large person group.");

            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            Dictionary<string, string[]> targetImageFileDictionary = new Dictionary<string, string[]>
            {
                { "Family1-Dad", new[] { "Family1-Dad1.jpg", "Family1-Dad2.jpg" } },
                { "Family1-Mom", new[] { "Family1-Mom1.jpg", "Family1-Mom2.jpg" } },
                { "Family1-Son", new[] { "Family1-Son1.jpg", "Family1-Son2.jpg" } },
                { "Family1-Daughter", new[] { "Family1-Daughter1.jpg", "Family1-Daughter2.jpg" } },
                { "Family2-Lady", new[] { "Family2-Lady1.jpg", "Family2-Lady2.jpg" } },
                { "Family2-Man", new[] { "Family2-Man1.jpg", "Family2-Man2.jpg" } }
            };
            string sourceImageFileName = "identification1.jpg";

            // Create a large person group.
            string largePersonGroupId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create a large person group ({largePersonGroupId}).");
            await client.LargePersonGroup.CreateAsync(largePersonGroupId, largePersonGroupId);

            foreach (var targetImageFileDictionaryName in targetImageFileDictionary.Keys)
            {
                // Create a large person group person.
                Person p = new Person { Name = targetImageFileDictionaryName, UserData = "Person for sample" };
                p.PersonId = (await client.LargePersonGroupPerson.CreateAsync(largePersonGroupId, p.Name)).PersonId;
                Console.WriteLine($"Create a large person group person '{p.Name}'.");

                foreach (var targetImageFileName in targetImageFileDictionary[targetImageFileDictionaryName])
                {
                    // Read target image file. 
                    using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileDictionary[targetImageFileDictionaryName][0]), FileMode.Open))
                    {
                        // Add face to the large person group person.
                        Console.WriteLine($"Add face to the large person group person({targetImageFileDictionaryName}) from image `{targetImageFileName}`.");
                        PersistedFace face = await client.LargePersonGroupPerson.AddFaceFromStreamAsync(largePersonGroupId, p.PersonId, stream, targetImageFileName);

                        if (face == null)
                        {
                            throw new Exception($"No persisted face from image `{targetImageFileName}`.");
                        }
                    }
                }
            }

            // Start to train the large person group.
            Console.WriteLine($"Train large person group {largePersonGroupId}.");
            await client.LargePersonGroup.TrainAsync(largePersonGroupId);

            // Wait until the training is completed.
            while (true)
            {
                await Task.Delay(1000);
                var trainingStatus = await client.LargePersonGroup.GetTrainingStatusAsync(largePersonGroupId);
                Console.WriteLine($"Training status is {trainingStatus.Status}.");
                if (trainingStatus.Status != TrainingStatusType.Running)
                {
                    if (trainingStatus.Status == TrainingStatusType.Failed)
                    {
                        throw new Exception($"Training failed with message {trainingStatus.Message}.");
                    }

                    break;
                }
            }

            List<Guid> sourceFaceIds = new List<Guid>();

            // Add detected faceId to sourceFaceIds.
            List<DetectedFace> detectedFaces = await Common.DetectedFace(client, Path.Combine("Images", sourceImageFileName));
            sourceFaceIds.Add(detectedFaces[0].FaceId.Value);

            // Identify example of identifying faces towards large person group. 
            var identifyResults = await client.Face.IdentifyAsync(sourceFaceIds, null, largePersonGroupId);
            if (identifyResults == null)
            {
                Console.WriteLine($"No people found in the group the same as the {sourceImageFileName}.");
                return;
            }

            foreach (var identifyResult in identifyResults)
            {
                Person person = await client.LargePersonGroupPerson.GetAsync(largePersonGroupId, identifyResult.Candidates[0].PersonId);
                Console.WriteLine($"Person '{person.Name}' is identified for faces in {sourceImageFileName}, confidence: {identifyResult.Candidates[0].Confidence}.");
            }

            // Delete the person group.
            await client.LargePersonGroup.DeleteAsync(largePersonGroupId);
            Console.WriteLine($"Delete the large person group {largePersonGroupId}.");
            Console.WriteLine();
        }
    }
}
