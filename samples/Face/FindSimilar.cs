namespace Microsoft.Azure.CognitiveServices.Samples.Face
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.Vision.Face;
    using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

    public static class FindSimilarInFaceIds
    {
        public static async Task Run(string endpoint, string key)
        {
            Console.WriteLine("Sample of finding similar faces in face ids.");

            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
            string recognitionModel = RecognitionModel.Recognition02;

            const string ImageUrlPrefix = "https://csdx.blob.core.windows.net/resources/Face/Images/";
            List<string> targetImageFileNames = new List<string>
                                                    {
                                                        "Family1-Dad1.jpg",
                                                        "Family1-Daughter1.jpg",
                                                        "Family1-Mom1.jpg",
                                                        "Family1-Son1.jpg",
                                                        "Family2-Lady1.jpg",
                                                        "Family2-Man1.jpg",
                                                        "Family3-Lady1.jpg",
                                                        "Family3-Man1.jpg"
                                                    };

            string sourceImageFileName = "findsimilar.jpg";

            IList<Guid?> targetFaceIds = new List<Guid?>();
            foreach (var targetImageFileName in targetImageFileNames)
            {
                // Detect faces from target image url.
                var faces = await Common.DetectFaces(client, $"{ImageUrlPrefix}{targetImageFileName}", recognitionModel: recognitionModel);

                // Add detected faceId to targetFaceIds.
                targetFaceIds.Add(faces[0].FaceId.Value);
            }

            // Detect faces from source image url.
            IList<DetectedFace> detectedFaces = await Common.DetectFaces(
                                                    client,
                                                    $"{ImageUrlPrefix}{sourceImageFileName}",
                                                    recognitionModel: recognitionModel);

            // Find similar example of faceId to faceIds.
            IList<SimilarFace> similarResults = await client.Face.FindSimilarAsync(
                                                    detectedFaces[0].FaceId.Value,
                                                    null,
                                                    null,
                                                    targetFaceIds);
            if (similarResults.Count == 0)
            {
                Console.WriteLine($"No similar faces to {sourceImageFileName}.");
            }

            foreach (var similarResult in similarResults)
            {
                Console.WriteLine(
                    $"Faces from {sourceImageFileName} & {similarResult.FaceId} are similar with confidence: {similarResult.Confidence}.");
            }

            Console.WriteLine();
        }
    }

    public static class FindSimilarInFaceList
    {
        public static async Task Run(string endpoint, string key)
        {
            Console.WriteLine("Sample of finding similar faces in face list.");

            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
            string recognitionModel = RecognitionModel.Recognition02;

            const string ImageUrlPrefix = "https://csdx.blob.core.windows.net/resources/Face/Images/";
            List<string> targetImageFileNames = new List<string>
                                                    {
                                                        "Family1-Dad1.jpg",
                                                        "Family1-Daughter1.jpg",
                                                        "Family1-Mom1.jpg",
                                                        "Family1-Son1.jpg",
                                                        "Family2-Lady1.jpg",
                                                        "Family2-Man1.jpg",
                                                        "Family3-Lady1.jpg",
                                                        "Family3-Man1.jpg"
                                                    };

            string sourceImageFileName = "findsimilar.jpg";

            // Create a face list.
            string faceListId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create FaceList {faceListId}.");
            await client.FaceList.CreateAsync(
                faceListId,
                "face list for FindSimilar sample",
                "face list for FindSimilar sample",
                recognitionModel: recognitionModel);

            foreach (var targetImageFileName in targetImageFileNames)
            {
                // Add face to face list.
                var faces = await client.FaceList.AddFaceFromUrlAsync(
                                faceListId,
                                $"{ImageUrlPrefix}{targetImageFileName}",
                                targetImageFileName);
                if (faces == null)
                {
                    throw new Exception($"No face detected from image `{targetImageFileName}`.");
                }

                Console.WriteLine($"Face from image {targetImageFileName} is successfully added to the face list.");
            }

            // Get persisted faces from the face list.
            List<PersistedFace> persistedFaces = (await client.FaceList.GetAsync(faceListId)).PersistedFaces.ToList();
            if (persistedFaces.Count == 0)
            {
                throw new Exception($"No persisted face in face list '{faceListId}'.");
            }

            // Detect faces from source image url.
            IList<DetectedFace> detectedFaces = await Common.DetectFaces(
                                                    client,
                                                    $"{ImageUrlPrefix}{sourceImageFileName}",
                                                    recognitionModel: recognitionModel);

            // Find similar example of faceId to face list.
            var similarResults = await client.Face.FindSimilarAsync(detectedFaces[0].FaceId.Value, faceListId);
            foreach (var similarResult in similarResults)
            {
                PersistedFace persistedFace =
                    persistedFaces.Find(face => face.PersistedFaceId == similarResult.PersistedFaceId);
                if (persistedFace == null)
                {
                    Console.WriteLine("Persisted face not found in similar result.");
                    continue;
                }

                Console.WriteLine(
                    $"Faces from {sourceImageFileName} & {persistedFace.UserData} are similar with confidence: {similarResult.Confidence}.");
            }

            // Delete the face list.
            await client.FaceList.DeleteAsync(faceListId);
            Console.WriteLine($"Delete FaceList {faceListId}.");
            Console.WriteLine();
        }
    }

    public static class FindSimilarInLargeFaceList
    {
        public static async Task Run(string endpoint, string key)
        {
            Console.WriteLine("Sample of finding similar faces in large face list.");

            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
            string recognitionModel = RecognitionModel.Recognition02;

            const string ImageUrlPrefix = "https://csdx.blob.core.windows.net/resources/Face/Images/";
            List<string> targetImageFileNames = new List<string>
                                                    {
                                                        "Family1-Dad1.jpg",
                                                        "Family1-Daughter1.jpg",
                                                        "Family1-Mom1.jpg",
                                                        "Family1-Son1.jpg",
                                                        "Family2-Lady1.jpg",
                                                        "Family2-Man1.jpg",
                                                        "Family3-Lady1.jpg",
                                                        "Family3-Man1.jpg"
                                                    };

            string sourceImageFileName = "findsimilar.jpg";

            // Create a large face list.
            string largeFaceListId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create large face list {largeFaceListId}.");
            await client.LargeFaceList.CreateAsync(
                largeFaceListId,
                "large face list for FindSimilar sample",
                "large face list for FindSimilar sample",
                recognitionModel: recognitionModel);

            foreach (var targetImageFileName in targetImageFileNames)
            {
                // Add face to the large face list.
                var faces = await client.LargeFaceList.AddFaceFromUrlAsync(
                                largeFaceListId,
                                $"{ImageUrlPrefix}{targetImageFileName}",
                                targetImageFileName);
                if (faces == null)
                {
                    throw new Exception($"No face detected from image `{targetImageFileName}`.");
                }

                Console.WriteLine(
                    $"Face from image {targetImageFileName} is successfully added to the large face list.");
            }

            // Start to train the large face list.
            Console.WriteLine($"Train large face list {largeFaceListId}.");
            await client.LargeFaceList.TrainAsync(largeFaceListId);

            // Wait until the training is completed.
            while (true)
            {
                await Task.Delay(1000);
                var trainingStatus = await client.LargeFaceList.GetTrainingStatusAsync(largeFaceListId);
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

            // Get persisted faces from the large face list.
            List<PersistedFace> persistedFaces = (await client.LargeFaceList.ListFacesAsync(largeFaceListId)).ToList();
            if (persistedFaces.Count == 0)
            {
                throw new Exception($"No persisted face in large face list '{largeFaceListId}'.");
            }

            // Detect faces from source image url.
            IList<DetectedFace> detectedFaces = await Common.DetectFaces(
                                                    client,
                                                    $"{ImageUrlPrefix}{sourceImageFileName}",
                                                    recognitionModel: recognitionModel);

            // Find similar example of faceId to large face list.
            var similarResults = await client.Face.FindSimilarAsync(
                                     detectedFaces[0].FaceId.Value,
                                     null,
                                     largeFaceListId);
            foreach (var similarResult in similarResults)
            {
                PersistedFace persistedFace =
                    persistedFaces.Find(face => face.PersistedFaceId == similarResult.PersistedFaceId);
                if (persistedFace == null)
                {
                    Console.WriteLine("Persisted face not found in similar result.");
                    continue;
                }

                Console.WriteLine(
                    $"Faces from {sourceImageFileName} & {persistedFace.UserData} are similar with confidence: {similarResult.Confidence}.");
            }

            // Delete the large face list.
            await client.LargeFaceList.DeleteAsync(largeFaceListId);
            Console.WriteLine($"Delete LargeFaceList {largeFaceListId}.");
            Console.WriteLine();
        }
    }
}
