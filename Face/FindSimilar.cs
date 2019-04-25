// Note: Add the NuGet package Microsoft.Azure.CognitiveServices.Vision.Face to your solution.
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Face_CS
{
    class Program
    {
        // NOTE: Replace this with a valid Face subscription key.
        private const string subscriptionKey = "INSERT KEY HERE";

        // You must use the same region as you used to get your subscription
        // keys. For example, if you got your subscription keys from westus,
        // replace "westcentralus" with "westus".
        //
        // Free trial subscription keys are generated in the westcentralus
        // region. If you use a free trial subscription key, you shouldn't
        // need to change the region.
        // Specify the Azure region
        private const string faceEndpoint =
            "https://westcentralus.api.cognitive.microsoft.com";

        // This image should contain a single face.
        private const string remoteImageUrl_1 =
            "https://www.biography.com/.image/t_share/MTQ1MzAyNzYzOTgxNTE0NTEz/john-f-kennedy---mini-biography.jpg";

        // This image should contain several faces, at least one of which is similar to the face in
        // remoteImageUrl_1.
        private const string remoteImageUrl_2 = "https://www.jfktribute.com/wp-content/gallery/pictures-from-jfks-visit/10004989a.jpg";

        // Detect faces in a remote image.
        private static async Task<IList<DetectedFace>> DetectFaces(
            FaceClient faceClient, string imageUrl)
        {
            IList<DetectedFace> detectedFaces = new List<DetectedFace>();

            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine("\nInvalid remoteImageUrl:\n{0} \n", imageUrl);
            }
            else
            {
                try
                {
                    Console.WriteLine("Detecting faces ...");
                    var faces = await faceClient.Face.DetectWithUrlAsync(imageUrl, true);
                    // Discard faces that do not have face IDs.
                    detectedFaces = faces.Where(x => x.FaceId.HasValue).ToList();
                }
                catch (APIErrorException e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return detectedFaces;
        }

        // Find similar faces to @faceID in @faceIDs.
        private static async Task<IList<SimilarFace>> FindSimilarFaces(
            FaceClient faceClient, Guid faceID, IList<Guid> faceIDs)
        {
            IList<SimilarFace> similarFaces = new List<SimilarFace>();

            try
            {
                Console.WriteLine("Finding similar faces ...");
                var faces = await faceClient.Face.FindSimilarAsync(faceID, null, null, faceIDs.Cast<Guid?>().ToList());
                // Discard faces that do not have face IDs.
                similarFaces = faces.Where(x => x.FaceId.HasValue).ToList();
            }
            catch (APIErrorException e)
            {
                Console.WriteLine(e.Message);
            }

            return similarFaces;
        }

        // Print the IDs of a list of faces that were detected in an image.
        private static void PrintFaceIDs (IList<DetectedFace> faces)
        {
            faces.Where(x => x.FaceId.HasValue).Select(x => x.FaceId.Value).ToList().ForEach(x => Console.WriteLine(x));
        }

        private static async Task RunQuickstart(FaceClient faceClient)
        {
            // Detect a face in the first image.
            var faces_1 = await DetectFaces(faceClient, remoteImageUrl_1);
            if (false == faces_1.Any())
            {
                Console.WriteLine("No faces detected in " + remoteImageUrl_1 + ".");
            }
            else
            {
                Console.WriteLine("Face IDs of faces detected in " + remoteImageUrl_1 + ":");
                PrintFaceIDs(faces_1);
                Console.WriteLine("Using first face ID.");
                // Note DetectFaces filters out faces for which FaceId.HasValue is false.
                var faceID = faces_1.First().FaceId.Value;

                // Detect a list of faces in the second image.
                var faces_2 = await DetectFaces(faceClient, remoteImageUrl_2);
                if (false == faces_2.Any())
                {
                    Console.WriteLine("No faces detected in " + remoteImageUrl_2 + ".");
                }
                else
                {
                    Console.WriteLine("Face IDs of faces detected in " + remoteImageUrl_2 + ":");
                    PrintFaceIDs(faces_2);

                    // Search the faces detected in the second image to find a similar face to the first one.
                    var similarFaces = await FindSimilarFaces(faceClient, faceID, faces_2.Select(x => x.FaceId.Value).ToList());
                    if (false == similarFaces.Any())
                    {
                        Console.WriteLine("No similar faces found in " + remoteImageUrl_2 + ".");
                    }
                    else
                    {
                        Console.WriteLine("Similar faces found in " + remoteImageUrl_2 + ":");
                        foreach(SimilarFace face in similarFaces)
                        {
                            // Note FindSimilarFaces filters out faces for which FaceId.HasValue is false.
                            faceID = face.FaceId.Value;
                            // SimilarFace only contains a Face ID, Persisted Face ID, and confidence score.
                            // So we look up the Face ID in the list of DetectedFaces found in
                            // remoteImageUrl_2 to get the rest of the face information.
                            var faceInfo = faces_2.FirstOrDefault(x => x.FaceId.Value == faceID);
                            if (faceInfo != null)
                            {
                                Console.WriteLine("Face ID: " + faceID);
                                Console.WriteLine("Face rectangle:");
                                Console.WriteLine("Left: " + faceInfo.FaceRectangle.Left);
                                Console.WriteLine("Top: " + faceInfo.FaceRectangle.Top);
                                Console.WriteLine("Width: " + faceInfo.FaceRectangle.Width);
                                Console.WriteLine("Height: " + faceInfo.FaceRectangle.Height);
                            }
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            FaceClient faceClient = new FaceClient(
                new ApiKeyServiceClientCredentials(subscriptionKey),
                new System.Net.Http.DelegatingHandler[] { });
            faceClient.Endpoint = faceEndpoint;

            Task.WaitAll (RunQuickstart(faceClient));

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }
    }
}
