namespace Microsoft.Azure.CognitiveServices.Samples.Face
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.Vision.Face;
    using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

    public static class VerifyFaceToFace
    {
        public static async Task Run(string endpoint, string key)
        {
            Console.WriteLine("Sample of verify face to face.");

            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            List<string> targetImageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg" };
            string sourceImageFileName1 = "Family1-Dad3.jpg";
            string sourceImageFileName2 = "Family1-Son1.jpg";

            List<Guid> targetFaceIds = new List<Guid>();
            foreach (var imageFileName in targetImageFileNames)
            {
                // Detect faces from target image file.
                targetFaceIds.Add((await Common.DetectedFace(client, Path.Combine("Images", imageFileName)))[0].FaceId.Value);
            }

            // Detect faces from source image file 1.
            Guid sourceFaceId1 = ((await Common.DetectedFace(client, Path.Combine("Images", sourceImageFileName1)))[0].FaceId.Value);

            // Detect faces from source image file 2.
            Guid sourceFaceId2 = ((await Common.DetectedFace(client, Path.Combine("Images", sourceImageFileName2)))[0].FaceId.Value);

            // Verification example for faces of the same person.
            VerifyResult verifyResult1 = await client.Face.VerifyFaceToFaceAsync(sourceFaceId1, targetFaceIds[0]);
            Console.WriteLine(
                verifyResult1.IsIdentical
                    ? $"Faces from {sourceImageFileName1} & {targetImageFileNames[0]} are of the same (Positive) person, similarity confidence: {verifyResult1.Confidence}."
                    : $"Faces from {sourceImageFileName1} & {targetImageFileNames[0]} are of different (Negative) persons, similarity confidence: {verifyResult1.Confidence}.");

            // Verification example for faces of different persons.
            VerifyResult verifyResult2 = await client.Face.VerifyFaceToFaceAsync(sourceFaceId2, targetFaceIds[0]);
            Console.WriteLine(
                verifyResult2.IsIdentical
                    ? $"Faces from {sourceImageFileName2} & {targetImageFileNames[0]} are of the same (Negative) person, similarity confidence: {verifyResult2.Confidence}."
                    : $"Faces from {sourceImageFileName2} & {targetImageFileNames[0]} are of different (Positive) persons, similarity confidence: {verifyResult2.Confidence}.");

            Console.WriteLine();
        }
    }

    public static class VerifyInPersonGroup
    {
        public static async Task Run(string endpoint, string key)
        {
            Console.WriteLine("Sample of verify face to person group.");

            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            List<string> targetImageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg" };
            string sourceImageFileName1 = "Family1-Dad3.jpg";

            // Create a person group.
            string personGroupId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create a person group ({personGroupId}).");
            await client.PersonGroup.CreateAsync(personGroupId, personGroupId);

            // Create a person group person.
            Person p = new Person { Name = "Dad", UserData = "Person for sample" };
            Console.WriteLine($"Create a person group person '{p.Name}'.");
            p.PersonId = (await client.PersonGroupPerson.CreateAsync(personGroupId, p.Name)).PersonId;

            foreach (var targetImageFileName in targetImageFileNames)
            {
                // Read target image files.
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open))
                {
                    // Add face to the person group. 
                    Console.WriteLine($"Add face to the person group person({p.Name}) from image `{targetImageFileName}`.");
                    PersistedFace faces = await client.PersonGroupPerson.AddFaceFromStreamAsync(personGroupId, p.PersonId, stream, targetImageFileName);

                    if (faces == null)
                    {
                        throw new Exception($"No persisted face from image `{targetImageFileName}`.");
                    }
                }
            }

            List<Guid> faceIds = new List<Guid>();

            // Add detected faceId to faceIds.
            faceIds.Add((await Common.DetectedFace(client, Path.Combine("Images", sourceImageFileName1)))[0].FaceId.Value);

            // Verification example for faces of the same person.
            VerifyResult verifyResult = await client.Face.VerifyFaceToPersonAsync(faceIds[0], p.PersonId, personGroupId);
            Console.WriteLine(
                verifyResult.IsIdentical
                    ? $"Faces from {sourceImageFileName1} & {p.Name} are of the same (Positive) person, similarity confidence: {verifyResult.Confidence}."
                    : $"Faces from {sourceImageFileName1} & {p.Name} are of different (Negative) persons, similarity confidence: {verifyResult.Confidence}.");

            // Delete the person group.
            Console.WriteLine($"Delete the person group ({personGroupId}).");
            await client.PersonGroup.DeleteAsync(personGroupId);

            Console.WriteLine();
        }
    }

    public static class VerifyInLargePersonGroup
    {
        public static async Task Run(string endpoint, string key)
        {
            Console.WriteLine("Sample of verify face to large person group.");

            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            List<string> targetImageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg" };
            string sourceImageFileName1 = "Family1-Dad3.jpg";

            // Create a large person group.
            string largePersonGroupId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create a large person group ({largePersonGroupId}).");
            await client.LargePersonGroup.CreateAsync(largePersonGroupId, largePersonGroupId);

            // Create a large person group person.
            Person p = new Person { Name = "Dad", UserData = "Person for sample" };
            Console.WriteLine($"Create a large person group person '{p.Name}'.");
            p.PersonId = (await client.LargePersonGroupPerson.CreateAsync(largePersonGroupId, p.Name)).PersonId;

            foreach (var targetImageFileName in targetImageFileNames)
            {
                // Read target image files.
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open))
                {
                    // Add face to the large person group. 
                    Console.WriteLine($"Add face to the large person group person({p.Name}) from image {targetImageFileName}.");
                    PersistedFace faces = await client.LargePersonGroupPerson.AddFaceFromStreamAsync(largePersonGroupId, p.PersonId, stream, targetImageFileName);

                    if (faces == null)
                    {
                        throw new Exception($"No persisted face from image `{targetImageFileName}`.");
                    }
                }
            }

            List<Guid> faceIds = new List<Guid>();

            // Add detected faceId to faceIds.
            faceIds.Add((await Common.DetectedFace(client, Path.Combine("Images", sourceImageFileName1)))[0].FaceId.Value);

            // Verification example for faces of the same person.
            VerifyResult verifyResult = await client.Face.VerifyFaceToPersonAsync(faceIds[0], p.PersonId, null, largePersonGroupId);
            Console.WriteLine(
                verifyResult.IsIdentical
                    ? $"Faces from {sourceImageFileName1} & {p.Name} are of the same (Positive) person, similarity confidence: {verifyResult.Confidence}."
                    : $"Faces from {sourceImageFileName1} & {p.Name} are of different (Negative) persons, similarity confidence: {verifyResult.Confidence}.");

            // Delete the large person group.
            Console.WriteLine($"Delete the large person group ({largePersonGroupId}).");
            await client.LargePersonGroup.DeleteAsync(largePersonGroupId);

            Console.WriteLine();
        }
    }
}
