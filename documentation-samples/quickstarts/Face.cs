/*
 * # This sample does the following tasks:
 * - Create a person group.
 * - Add a person to a person group.
 * - Add a face to a person in a person group.
 * - Train a person group.
 * - Use a person group to identify a face in a remote image.
 * - Find similar faces in two remote images.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Note: Add the NuGet package Microsoft.Azure.CognitiveServices.Vision.Face to your solution.
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace Face_CS
{
    class Program
    {
        private const string key_var = "FACE_SUBSCRIPTION_KEY";
        private static readonly string subscription_key = Environment.GetEnvironmentVariable(key_var);

        // Note you must use the same region as you used to get your subscription key.
        private const string region_var = "FACE_REGION";
        private static readonly string region = Environment.GetEnvironmentVariable(region_var);
        private static readonly string endpoint = "https://" + region + ".api.cognitive.microsoft.com";

        // The person group should have a unique name, such as "jfk_group_12345".
        // Use only numbers, lowercase English letters, '-', and '_'. Maximum length is 64.
        private const string person_group_ID_var = "FACE_PERSON_GROUP_ID";
        private static readonly string person_group_ID = Environment.GetEnvironmentVariable(person_group_ID_var);

        // This image should contain a single face.
        private const string single_face_image_url = "https://www.biography.com/.image/t_share/MTQ1MzAyNzYzOTgxNTE0NTEz/john-f-kennedy---mini-biography.jpg";

        // This image should contain several faces, at least one of which is similar to the face in single_face_image_url.
        private const string multi_face_image_url = "https://www.biography.com/.image/t_share/MTQ1NDY3OTIxMzExNzM3NjE3/john-f-kennedy---debating-richard-nixon.jpg";

        static Program ()
        {
            if (null == subscription_key)
            {
                throw new Exception("Please set/export the environment variable: " + key_var);
            }
            if (null == region)
            {
                throw new Exception("Please set/export the environment variable: " + region_var);
            }
            if (null == person_group_ID)
            {
                throw new Exception("Please set/export the environment variable: " + person_group_ID_var);
            }
        }

        // Detect faces in a remote image.
        private static async Task<IList<DetectedFace>> DetectFaces(FaceClient client, string image_url)
        {
            IList<DetectedFace> detectedFaces = new List<DetectedFace>();

            if (!Uri.IsWellFormedUriString(image_url, UriKind.Absolute))
            {
                throw new Exception("Invalid remote image URL: " + image_url);
            }
            Console.WriteLine("Detecting faces in: " + image_url);
            var faces = await client.Face.DetectWithUrlAsync(image_url, true);
            // Discard faces that do not have face IDs.
            return faces.Where(x => x.FaceId.HasValue).ToList();
        }

        // Find similar faces to @faceID in @faceIDs.
        private static async Task<IList<SimilarFace>> FindSimilarFaces(FaceClient client, Guid face_ID, IList<Guid> face_IDs)
        {
            IList<SimilarFace> similarFaces = new List<SimilarFace>();

            Console.WriteLine("Finding similar faces ...");
            var faces = await client.Face.FindSimilarAsync(face_ID, null, null, face_IDs.Cast<Guid?>().ToList());
            // Discard faces that do not have face IDs.
            return faces.Where(x => x.FaceId.HasValue).ToList();
        }

        // Print the IDs of a list of faces that were detected in an image.
        private static void PrintFaceIDs(IList<DetectedFace> faces)
        {
            faces.Where(x => x.FaceId.HasValue).Select(x => x.FaceId.Value).ToList().ForEach(x => Console.WriteLine(x));
        }

        // Check to see whether the person group already exists.
        private static async Task<bool> PersonGroupExists(FaceClient client, string person_group_ID)
        {
            var person_group_list = await client.PersonGroup.ListAsync();
            var person_group_IDs = person_group_list.Select(x => x.PersonGroupId);
            return person_group_IDs.Contains(person_group_ID);
        }

        // Create the person group if it does not already exist.
        private static async Task CreatePersonGroup(FaceClient client, string person_group_ID)
        {
            Console.WriteLine("Creating group...");
            if (true == await PersonGroupExists(client, person_group_ID))
            {
                Console.WriteLine("Group already exists.");
            }
            else
            {
                // Note the name parameter is optional, but if it is not provided, the method returns an error.
                await client.PersonGroup.CreateAsync(person_group_ID, "display name");
                Console.WriteLine("Group created.");
            }
        }

        // Add a person to the person group, and add a face to the person using the specified remote image.
        private static async Task AddPersonToGroup(FaceClient client, string person_group_ID, string image_url)
        {
            if (!Uri.IsWellFormedUriString(image_url, UriKind.Absolute))
            {
                throw new Exception("Invalid remote image URL: " + image_url);
            }
            Console.WriteLine("Adding person to group...");
            // Note the name parameter is optional, but if it is not provided, the method returns an error.
            var personGroupPerson = await client.PersonGroupPerson.CreateAsync(person_group_ID, "display name");
            var personID = personGroupPerson.PersonId;
            Console.WriteLine("Person added to group.");
            Console.WriteLine("Adding face to person...");
            await client.PersonGroupPerson.AddFaceFromUrlAsync(person_group_ID, personID, image_url);
            Console.WriteLine("Face added to person.");
        }

        // Train the person group.
        private static async Task TrainGroup(FaceClient client, string person_group_ID)
        {
            Console.WriteLine("Training group...");
            await client.PersonGroup.TrainAsync(person_group_ID);

            bool done = false;
            while (false == done)
            {
                Console.WriteLine("Waiting for training to finish...");
                System.Threading.Thread.Sleep(1000);
                var result = await client.PersonGroup.GetTrainingStatusAsync(person_group_ID);
                if (result.Status == TrainingStatusType.Failed)
                {
                    throw new Exception("Training failed: " + result.Message);
                }
                else
                {
                    done = (result.Status == TrainingStatusType.Succeeded);
                }
            }
            Console.WriteLine("Group trained.");
        }

        // Identify the faces in the person group using @faceIDs.
        private static async Task IdentifyFaces(FaceClient client, string person_group_ID)
        {
            Console.WriteLine("Identifying faces ...");

            var faces = await DetectFaces(client, single_face_image_url);
            if (false == faces.Any())
            {
                Console.WriteLine("No faces detected in " + single_face_image_url + ".");
            }
            else
            {
                var face_IDs = faces.Select(x => x.FaceId.Value).ToList();
                Console.WriteLine("Face IDs of faces detected in " + single_face_image_url + ":");
                face_IDs.ForEach(x => Console.WriteLine(x));

                await CreatePersonGroup(client, person_group_ID);
                await AddPersonToGroup(client, person_group_ID, multi_face_image_url);
                await TrainGroup(client, person_group_ID);

                var identify_results = await client.Face.IdentifyAsync(face_IDs, person_group_ID);
                var identified_face_IDs = identify_results.Select(x => x.FaceId).ToList();

                if (false == identified_face_IDs.Any())
                {
                    Console.WriteLine("No faces identified in " + multi_face_image_url + ".");
                }
                else
                {
                    Console.WriteLine("Face IDs identified in " + multi_face_image_url + ":");
                    identified_face_IDs.ToList().ForEach(x => Console.WriteLine(x));
                }
            }
        }

        private static async Task FindSimilar(FaceClient client)
        {
            // Detect a face in the first image.
            var faces_1 = await DetectFaces(client, single_face_image_url);
            if (false == faces_1.Any())
            {
                Console.WriteLine("No faces detected in " + single_face_image_url + ".");
            }
            else
            {
                Console.WriteLine("Face IDs of faces detected in " + single_face_image_url + ":");
                PrintFaceIDs(faces_1);
                Console.WriteLine("Using first face ID.");
                // Note DetectFaces filters out faces for which FaceId.HasValue is false.
                var faceID = faces_1.First().FaceId.Value;

                // Detect a list of faces in the second image.
                var faces_2 = await DetectFaces(client, multi_face_image_url);
                if (false == faces_2.Any())
                {
                    Console.WriteLine("No faces detected in " + multi_face_image_url + ".");
                }
                else
                {
                    Console.WriteLine("Face IDs of faces detected in " + multi_face_image_url + ":");
                    PrintFaceIDs(faces_2);

                    // Search the faces detected in the second image to find a similar face to the first one.
                    var similarFaces = await FindSimilarFaces(client, faceID, faces_2.Select(x => x.FaceId.Value).ToList());
                    if (false == similarFaces.Any())
                    {
                        Console.WriteLine("No similar faces found in " + multi_face_image_url + ".");
                    }
                    else
                    {
                        Console.WriteLine("Similar faces found in " + multi_face_image_url + ":");
                        foreach (SimilarFace face in similarFaces)
                        {
                            // Note FindSimilarFaces filters out faces for which FaceId.HasValue is false.
                            faceID = face.FaceId.Value;
                            // SimilarFace only contains a Face ID, Persisted Face ID, and confidence score.
                            // So we look up the Face ID in the list of DetectedFaces found in
                            // multi_face_image_url to get the rest of the face information.
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

        private static async Task RunQuickstart(FaceClient client, string person_group_ID)
        {
            await IdentifyFaces(client, person_group_ID);
            Console.WriteLine();
            await FindSimilar(client);

        }

        static void Main(string[] args)
        {
            FaceClient client = new FaceClient(
                new ApiKeyServiceClientCredentials(subscription_key),
                new System.Net.Http.DelegatingHandler[] { })
            {
                Endpoint = endpoint
            };

            Task.WaitAll(RunQuickstart(client, person_group_ID));

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }
    }
}
