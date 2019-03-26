namespace Microsoft.Azure.CognitiveServices.Samples.Face
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.Vision.Face;
    using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

    public static class Group
    {
        public static async Task Run(string endpoint, string key)
        {
            Console.WriteLine("Sample of grouping faces.");

            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
            string recognitionModel = RecognitionModel.Recognition02;

            const string ImageUrlPrefix = "https://csdx.blob.core.windows.net/resources/Face/Images/";
            List<string> imageFileNames = new List<string>
                                              {
                                                  "Family1-Dad1.jpg",
                                                  "Family1-Dad2.jpg",
                                                  "Family3-Lady1.jpg",
                                                  "Family1-Daughter1.jpg",
                                                  "Family1-Daughter2.jpg",
                                                  "Family1-Daughter3.jpg"
                                              };
            Dictionary<string, string> faces = new Dictionary<string, string>();
            List<Guid> faceIds = new List<Guid>();

            foreach (var imageFileName in imageFileNames)
            {
                // Detect faces from image url.
                IList<DetectedFace> detectedFaces = await Common.DetectFaces(
                                                        client,
                                                        $"{ImageUrlPrefix}{imageFileName}",
                                                        recognitionModel: recognitionModel);

                // Add detected faceId to faceIds and faces.
                faceIds.Add(detectedFaces[0].FaceId.Value);
                faces.Add(detectedFaces[0].FaceId.ToString(), imageFileName);
            }

            // Call grouping, the grouping result is a group collection, each group contains similar faces.
            var groupResult = await client.Face.GroupAsync(faceIds);

            // Face groups containing faces that have similar looking.
            for (int i = 0; i < groupResult.Groups.Count; i++)
            {
                Console.Write($"Found face group {i + 1}: ");
                foreach (var faceId in groupResult.Groups[i])
                {
                    Console.Write($"{faces[faceId.ToString()]} ");
                }

                Console.WriteLine(".");
            }

            // MessyGroup contains all faces which are not similar to any other faces.
            if (groupResult.MessyGroup.Count > 0)
            {
                Console.Write("Found messy face group: ");
                foreach (var faceId in groupResult.MessyGroup)
                {
                    Console.Write($"{faces[faceId.ToString()]} ");
                }

                Console.WriteLine(".");
            }

            Console.WriteLine();
        }
    }
}
