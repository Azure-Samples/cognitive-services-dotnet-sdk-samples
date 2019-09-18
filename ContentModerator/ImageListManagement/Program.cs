using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.CognitiveServices.ContentModerator;
using Microsoft.CognitiveServices.ContentModerator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ImageLists
{
    class Program
    {
        /// <summary>
        /// The minimum amount of time, im milliseconds, to wait between calls
        /// to the Image List API.
        /// </summary>
        private const int throttleRate = 3000;

        /// <summary>
        /// The number of minutes to delay after updating the search index before
        /// performing image match operations against the list.
        /// </summary>
        private const double latencyDelay = 0.5;

        /// <summary>
        /// Define constants for the labels to apply to the image list.
        /// </summary>
        private class Labels
        {
            public const string Sports = "Sports";
            public const string Swimsuit = "Swimsuit";
        }

        /// <summary>
        /// Define input data for images for this sample.
        /// </summary>
        private class Images
        {
            /// <summary>
            /// Represents a group of images that all share the same label.
            /// </summary>
            public class Data
            {
                /// <summary>
                /// The label for the images.
                /// </summary>
                public string Label;

                /// <summary>
                /// The URLs of the images.
                /// </summary>
                public string[] Urls;
            }

            /// <summary>
            /// The initial set of images to add to the list with the sports label.
            /// </summary>
            public static readonly Data Sports = new Data()
            {
                Label = Labels.Sports,
                Urls = new string[] {
                "https://moderatorsampleimages.blob.core.windows.net/samples/sample4.png",
                "https://moderatorsampleimages.blob.core.windows.net/samples/sample6.png",
                "https://moderatorsampleimages.blob.core.windows.net/samples/sample9.png"
            }
            };

            /// <summary>
            /// The initial set of images to add to the list with the swimsuit label.
            /// </summary>
            /// <remarks>We're adding sample16.png (image of a puppy), to simulate
            /// an improperly added image that we will later remove from the list.
            /// Note: each image can have only one entry in a list, so sample4.png
            /// will throw an exception when we try to add it with a new label.</remarks>
            public static readonly Data Swimsuit = new Data()
            {
                Label = Labels.Swimsuit,
                Urls = new string[] {
                "https://moderatorsampleimages.blob.core.windows.net/samples/sample1.jpg",
                "https://moderatorsampleimages.blob.core.windows.net/samples/sample3.png",
                "https://moderatorsampleimages.blob.core.windows.net/samples/sample4.png",
                "https://moderatorsampleimages.blob.core.windows.net/samples/sample16.png"
            }
            };

            /// <summary>
            /// The set of images to subsequently remove from the list.
            /// </summary>
            public static readonly string[] Corrections = new string[] {
            "https://moderatorsampleimages.blob.core.windows.net/samples/sample16.png"
        };
        }

        /// <summary>
        /// The images to match against the image list.
        /// </summary>
        /// <remarks>Samples 1 and 4 should scan as matches; samples 5 and 16 should not.</remarks>
        private static readonly string[] ImagesToMatch = new string[] {
        "https://moderatorsampleimages.blob.core.windows.net/samples/sample1.jpg",
        "https://moderatorsampleimages.blob.core.windows.net/samples/sample4.png",
        "https://moderatorsampleimages.blob.core.windows.net/samples/sample5.png",
        "https://moderatorsampleimages.blob.core.windows.net/samples/sample16.png"
    };

        /// <summary>
        /// A dictionary that tracks the ID assigned to each image URL when 
        /// the image is added to the list.
        /// </summary>
        /// <remarks>Indexed by URL.</remarks>
        private static readonly Dictionary<string, int> ImageIdMap =
            new Dictionary<string, int>();

        /// <summary>
        /// The name of the file to contain the output from the list management operations.
        /// </summary>
        /// <remarks>Relative paths are relative to the execution directory.</remarks>
        private static string OutputFile = "ListOutput.log";

        /// <summary>
        /// A static reference to the text writer to use for logging.
        /// </summary>
        private static TextWriter writer;

        /// <summary>
        /// A copy of the list details.
        /// </summary>
        /// <remarks>Used to initially create the list, and later to update the
        /// list details.</remarks>
        private static Body listDetails;

        static void Main(string[] args)
        {
            // Create the text writer to use for logging, and cache a static reference to it.
            using (StreamWriter outputWriter = new StreamWriter(OutputFile))
            {
                writer = outputWriter;

                // Create a Content Moderator client.
                using (var client = Clients.NewClient())
                {
                    // Create a custom image list and record the ID assigned to it.
                    var creationResult = CreateCustomList(client);
                    if (creationResult.Id.HasValue)
                    {
                        // Cache the ID of the new image list.
                        int listId = creationResult.Id.Value;

                        // Perform various operations using the image list.
                        AddImages(client, listId, Images.Sports.Urls, Images.Sports.Label);
                        AddImages(client, listId, Images.Swimsuit.Urls, Images.Swimsuit.Label);
                        
                        GetAllImageIds(client, listId);
                        UpdateListDetails(client, listId);
                        GetListDetails(client, listId);

                        // Be sure to refresh search index
                        RefreshSearchIndex(client, listId);

                        // WriteLine();
                        WriteLine($"Waiting {latencyDelay} minutes to allow the server time to propagate the index changes.", true);
                        Thread.Sleep((int)(latencyDelay * 60 * 1000));

                        // Match images against the image list.
                        MatchImages(client, listId, ImagesToMatch);

                        // Remove images
                        RemoveImages(client, listId, Images.Corrections);

                        // Be sure to refresh search index
                        RefreshSearchIndex(client, listId);

                        WriteLine();
                        WriteLine($"Waiting {latencyDelay} minutes to allow the server time to propagate the index changes.", true);
                        Thread.Sleep((int)(latencyDelay * 60 * 1000));

                        // Match images again against the image list. The removed image should not get matched.
                        MatchImages(client, listId, ImagesToMatch);

                        // Delete all images from the list.
                        DeleteAllImages(client, listId);

                        // Delete the image list.
                        DeleteCustomList(client, listId);

                        // Verify that the list was deleted.
                        GetAllListIds(client);
                    }
                }

                writer.Flush();
                writer.Close();
                writer = null;
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Creates the custom list.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <returns>The response object from the operation.</returns>
        private static ImageList CreateCustomList(ContentModeratorClient client)
        {
            // Create the request body.
            Dictionary<string, string> Metadata = new Dictionary<string, string>();
            Metadata.Add("Acceptable", "Potentially racy");
            listDetails = new Body("MyList", "A sample list", Metadata);

            WriteLine($"Creating list {listDetails.Name}.", true);

            var result = client.ListManagementImageLists.Create(
                "application/json", listDetails);
            Thread.Sleep(throttleRate);

            WriteLine("Response:");
            WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));

            return result;
        }

        /// <summary>
        /// Adds images to an image list.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="listId">The list identifier.</param>
        /// <param name="imagesToAdd">The images to add.</param>
        /// <param name="label">The label to apply to each image.</param>
        /// <remarks>Images are assigned content IDs when they are added to the list.
        /// Track the content ID assigned to each image.</remarks>
        private static void AddImages(
            ContentModeratorClient client, int listId,
            IEnumerable<string> imagesToAdd, string label)
        {
            foreach (var imageUrl in imagesToAdd)
            {
                WriteLine();
                WriteLine($"Adding {imageUrl} to list {listId} with label {label}.", true);
                try
                {
                    var result = client.ListManagementImage.AddImageUrlInput(
                        listId.ToString(), "application/json", new BodyModel("URL", imageUrl), null, label);

                    ImageIdMap.Add(imageUrl, Int32.Parse(result.ContentId));

                    WriteLine("Response:");
                    WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
                }
                catch (Exception ex)
                {
                    WriteLine($"Unable to add image to list. Caught {ex.GetType().FullName}: {ex.Message}", true);
                }
                finally
                {
                    Thread.Sleep(throttleRate);
                }
            }
        }

        /// <summary>
        /// Removes images from an image list.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="listId">The list identifier.</param>
        /// <param name="imagesToRemove">The images to remove.</param>
        /// <remarks>Images are assigned content IDs when they are added to the list.
        /// Use the content ID to remove the image.</remarks>
        private static void RemoveImages(
            ContentModeratorClient client, int listId,
            IEnumerable<string> imagesToRemove)
        {
            foreach (var imageUrl in imagesToRemove)
            {
                if (!ImageIdMap.ContainsKey(imageUrl)) continue;
                int imageId = ImageIdMap[imageUrl];

                WriteLine();
                WriteLine($"Removing entry for {imageUrl} (ID = {imageId}) from list {listId}.", true);

                var result = client.ListManagementImage.DeleteImage(
                    listId.ToString(), imageId.ToString());
                Thread.Sleep(throttleRate);

                ImageIdMap.Remove(imageUrl);

                WriteLine("Response:");
                WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            }
        }

        /// <summary>
        /// Gets all image IDs in an image list.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="listId">The list identifier.</param>
        /// <returns>The response object from the operation.</returns>
        private static ImageIds GetAllImageIds(
            ContentModeratorClient client, int listId)
        {
            WriteLine();
            WriteLine($"Getting all image IDs for list {listId}.", true);

            var result = client.ListManagementImage.GetAllImageIds(listId.ToString());
            Thread.Sleep(throttleRate);

            WriteLine("Response:");
            WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));

            return result;
        }

        /// <summary>
        /// Updates the details of an image list.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="listId">The list identifier.</param>
        /// <returns>The response object from the operation.</returns>
        private static ImageList UpdateListDetails(
            ContentModeratorClient client, int listId)
        {
            WriteLine();
            WriteLine($"Updating details for list {listId}.", true);

            listDetails.Name = "Swimsuits and sports";

            var result = client.ListManagementImageLists.Update(
                listId.ToString(), "application/json", listDetails);
            Thread.Sleep(throttleRate);

            WriteLine("Response:");
            WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));

            return result;
        }

        /// <summary>
        /// Gets the details for an image list.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="listId">The list identifier.</param>
        /// <returns>The response object from the operation.</returns>
        private static ImageList GetListDetails(
            ContentModeratorClient client, int listId)
        {
            WriteLine();
            WriteLine($"Getting details for list {listId}.", true);

            var result = client.ListManagementImageLists.GetDetails(listId.ToString());
            Thread.Sleep(throttleRate);

            WriteLine("Response:");
            WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));

            return result;
        }

        /// <summary>
        /// Refreshes the search index for an image list.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="listId">The list identifier.</param>
        /// <returns>The response object from the operation.</returns>
        private static RefreshIndex RefreshSearchIndex(
            ContentModeratorClient client, int listId)
        {
            WriteLine();
            WriteLine($"Refreshing the search index for list {listId}.", true);

            var result = client.ListManagementImageLists.RefreshIndexMethod(listId.ToString());
            Thread.Sleep(throttleRate);

            WriteLine("Response:");
            WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));

            return result;
        }

        /// <summary>
        /// Matches images against an image list.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="listId">The list identifier.</param>
        /// <param name="imagesToMatch">The images to screen.</param>
        private static void MatchImages(
            ContentModeratorClient client, int listId,
            IEnumerable<string> imagesToMatch)
        {
            foreach (var imageUrl in imagesToMatch)
            {
                WriteLine();
                WriteLine($"Matching image {imageUrl} against list {listId}.", true);

                var result = client.ImageModeration.MatchUrlInput(
                    "application/json", new BodyModel("URL", imageUrl), listId.ToString());
                Thread.Sleep(throttleRate);

                WriteLine("Response:");
                WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            }
        }

        /// <summary>
        /// Deletes all images from an image list.
        /// </summary>
        /// <param name="client">The Content Modertor client.</param>
        /// <param name="listId">The list identifier.</param>
        private static void DeleteAllImages(
            ContentModeratorClient client, int listId)
        {
            WriteLine();
            WriteLine($"Deleting all images from list {listId}.", true);

            var result = client.ListManagementImage.DeleteAllImages(listId.ToString());
            Thread.Sleep(throttleRate);

            WriteLine("Response:");
            WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        /// <summary>
        /// Deletes an image list.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="listId">The list identifier.</param>
        private static void DeleteCustomList(
            ContentModeratorClient client, int listId)
        {
            WriteLine();
            WriteLine($"Deleting list {listId}.", true);

            var result = client.ListManagementImageLists.Delete(listId.ToString());
            Thread.Sleep(throttleRate);

            WriteLine("Response:");
            WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        /// <summary>
        /// Gets all list identifiers for the client.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <returns>The response object from the operation.</returns>
        private static IList<ImageList> GetAllListIds(ContentModeratorClient client)
        {
            WriteLine();
            WriteLine($"Getting all image list IDs.", true);

            var result = client.ListManagementImageLists.GetAllImageLists();
            Thread.Sleep(throttleRate);

            WriteLine("Response:");
            WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));

            return result;
        }

        /// <summary>
        /// Writes a message to the log file, and optionally to the console.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="echo">if set to <c>true</c>, write the message to the console.</param>
        private static void WriteLine(string message = null, bool echo = false)
        {
            writer.WriteLine(message ?? String.Empty);

            if (echo)
            {
                Console.WriteLine(message ?? String.Empty);
            }
        }
    }

    /// <summary>
    /// Wraps the creation and configuration of a Content Moderator client.
    /// </summary>
    /// <remarks>This class library contains insecure code. If you adapt this 
    /// code for use in production, use a secure method of storing and using
    /// your Content Moderator subscription key.</remarks>
    public static class Clients
    {
        // TODO We could make team name a static property on this class, to move all of the subscription information into one project.

        /// <summary>
        /// The base URL fragment for Content Moderator calls.
        /// Add your Azure Face endpoint to your environment variables.
        /// </summary>
        private static readonly string AzureBaseURL =
            Environment.GetEnvironmentVariable("CONTENT_MODERATOR_ENDPOINT");

        /// <summary>
        /// Your Content Moderator subscription key.
        /// Add your Azure Face subscription key to your environment variables.
        /// </summary>
        private static readonly string CMSubscriptionKey = 
            Environment.GetEnvironmentVariable("CONTENT_MODERATOR_SUBSCRIPTION_KEY");

        /// <summary>
        /// Returns a new Content Moderator client for your subscription.
        /// </summary>
        /// <returns>The new client.</returns>
        /// <remarks>The <see cref="ContentModeratorClient"/> is disposable.
        /// When you have finished using the client,
        /// you should dispose of it either directly or indirectly. </remarks>
        public static ContentModeratorClient NewClient()
        {
            // Create and initialize an instance of the Content Moderator API wrapper.
            ContentModeratorClient client = new ContentModeratorClient(new ApiKeyServiceClientCredentials(CMSubscriptionKey));

            client.Endpoint = AzureBaseURL;
            return client;
        }
    }
}
