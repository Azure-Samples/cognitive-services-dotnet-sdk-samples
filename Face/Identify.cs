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

        // Set this to a unique name, such as "jfk_group_12345".
        // Use only numbers, lowercase English letters, '-', and '_'. Maximum length is 64.
        private const string personGroupID = "INSERT GROUP NAME HERE";

        private const string remoteImageUrl_1 =
            "https://www.biography.com/.image/t_share/MTQ1MzAyNzYzOTgxNTE0NTEz/john-f-kennedy---mini-biography.jpg";

        private const string remoteImageUrl_2 = "https://www.biography.com/.image/t_share/MTQ1NDY3OTIxMzExNzM3NjE3/john-f-kennedy---debating-richard-nixon.jpg";

        // Detect faces in a remote image.
        private static async Task<IList<Guid>> GetFaceIDs(
            FaceClient faceClient, string imageUrl)
        {
            IList<Guid> faceIDs = new List<Guid>();

            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine("\nInvalid remoteImageUrl:\n{0} \n", imageUrl);
            }
            else
            {
                try
                {
                    Console.WriteLine("Detecting faces ...");
                    IList<DetectedFace> faceList = await faceClient.Face.DetectWithUrlAsync(imageUrl, true);
                    faceIDs = faceList.Where(x => x.FaceId.HasValue).Select(x => x.FaceId.Value).ToList();
                }
                catch (APIErrorException e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return faceIDs;
        }

        // Check to see whether the person group already exists.
        private static async Task<bool> PersonGroupExists (FaceClient faceClient)
        {
            try
            {
                var personGroupList = await faceClient.PersonGroup.ListAsync();
                var personGroupIDs = personGroupList.Select(x => x.PersonGroupId);
                return personGroupIDs.Contains(personGroupID);
            }
            catch (APIErrorException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        // Create the person group if it does not already exist.
        private static async Task<bool> CreatePersonGroup (FaceClient faceClient)
        {
            Console.WriteLine("Creating group...");
            var exists = await PersonGroupExists(faceClient);
            if (true == exists)
            {
                Console.WriteLine("Group already exists.");
                return true;
            }
            else
            {
                try
                {
                    // Note the name parameter is optional, but if it is not provided, the method returns an error.
                    await faceClient.PersonGroup.CreateAsync(personGroupID, "display name");
                    Console.WriteLine("Group created.");
                    return true;
                }
                catch (APIErrorException e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
        }

        // Add a person to the person group, and add a face to the person using the specified remote image.
        private static async Task<bool> AddPersonToGroup(FaceClient faceClient, string imageUrl)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine("\nInvalid remoteImageUrl:\n{0} \n", imageUrl);
                return false;
            }
            else
            {
                try
                {
                    Console.WriteLine("Adding person to group...");
                    // Note the name parameter is optional, but if it is not provided, the method returns an error.
                    var personGroupPerson = await faceClient.PersonGroupPerson.CreateAsync(personGroupID, "display name");
                    var personID = personGroupPerson.PersonId;
                    Console.WriteLine("Person added to group.");
                    Console.WriteLine("Adding face to person...");
                    await faceClient.PersonGroupPerson.AddFaceFromUrlAsync(personGroupID, personID, imageUrl);
                    Console.WriteLine("Face added to person.");
                    return true;
                }
                catch (APIErrorException e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
        }

        // Train the person group.
        private static async Task<bool> TrainGroup(FaceClient faceClient)
        {
            try
            {
                Console.WriteLine("Training group...");
                await faceClient.PersonGroup.TrainAsync(personGroupID);

                bool trainingFinished = false;
                while (false == trainingFinished)
                {
                    Console.WriteLine("Waiting for training to finish...");
                    System.Threading.Thread.Sleep(1000);
                    var result = await faceClient.PersonGroup.GetTrainingStatusAsync(personGroupID);
                    if (result.Status == TrainingStatusType.Failed) {
                        throw new Exception("Training failed: " + result.Message);
                    }
                    else
                    {
                        trainingFinished = (result.Status == TrainingStatusType.Succeeded);
                    }
                }

                Console.WriteLine("Group trained.");
                return true;
            }
            catch (APIErrorException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        // Identify the faces in the person group using @faceIDs.
        private static async Task<IList<Guid>> IdentifyFaces(
            FaceClient faceClient, IList<Guid> faceIDs)
        {
            IList<Guid> identifiedFaceIDs = new List<Guid>();

            try
            {
                Console.WriteLine("Identifying faces ...");
                var identifyResults = await faceClient.Face.IdentifyAsync(faceIDs, personGroupID);
                identifiedFaceIDs = identifyResults.Select(x => x.FaceId).ToList();
            }
            catch (APIErrorException e)
            {
                Console.WriteLine(e.Message);
            }

            return identifiedFaceIDs;
        }

        private static async Task RunQuickstart(FaceClient faceClient)
        {
            var faceIDs = await GetFaceIDs(faceClient, remoteImageUrl_1);
            if (false == faceIDs.Any())
            {
                Console.WriteLine("No faces detected in " + remoteImageUrl_1 + ".");
            }
            else
            {
                Console.WriteLine("Face IDs of faces detected in " + remoteImageUrl_1 + ":");
                faceIDs.ToList().ForEach(x => Console.WriteLine(x));

                if (true == await CreatePersonGroup(faceClient))
                {
                    if (true == await AddPersonToGroup(faceClient, remoteImageUrl_2))
                    {
                        if (true == await TrainGroup(faceClient))
                        {
                            var identifiedFaceIDs = await IdentifyFaces(faceClient, faceIDs);
                            if (false == identifiedFaceIDs.Any())
                            {
                                Console.WriteLine("No faces identified in " + remoteImageUrl_2 + ".");
                            }
                            else
                            {
                                Console.WriteLine("Face IDs identified in " + remoteImageUrl_2 + ":");
                                identifiedFaceIDs.ToList().ForEach(x => Console.WriteLine(x));
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
