using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.CognitiveServices.ContentModerator;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace TextModeration
{
    class Program
    {
        /// <summary>
        /// The name of the file that contains the text to evaluate.
        /// </summary>
        /// <remarks>You will need to create an input file and update this path
        /// accordingly. Relative paths are relative to the execution directory.</remarks>
        private static string TextFile = "TextFile.txt";

        /// <summary>
        /// The name of the file to contain the output from the evaluation.
        /// </summary>
        /// <remarks>Relative paths are relative to the execution directory.</remarks>
        private static string OutputFile = "TextModerationOutput.txt";

        static void Main(string[] args)
        {
            // Load the input text.
            string text = File.ReadAllText(TextFile);
            Console.WriteLine("Screening {0}", TextFile);

            text = text.Replace(System.Environment.NewLine, " ");

            // Save the moderation results to a file.
            using (StreamWriter outputWriter = new StreamWriter(OutputFile, false))
            {
                // Create a Content Moderator client and evaluate the text.
                using (var client = Clients.NewClient())
                {
                    // Screen the input text: check for profanity, classify the text into three categories,
                    // do autocorrect text, and check for personally identifying 
                    // information (PII)
                    outputWriter.WriteLine("Autocorrect typos, check for matching terms, PII, and classify.");
                    var screenResult =
                        client.TextModeration.ScreenText("text/plain", new MemoryStream(Encoding.UTF8.GetBytes(text)), "eng", true, true, null, true);
                    outputWriter.WriteLine(
                        JsonConvert.SerializeObject(screenResult, Formatting.Indented));
                }
                outputWriter.Flush();
                outputWriter.Close();
            }

            Console.WriteLine("Results written to {0}", OutputFile);
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
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
        /// Add your Azure Content Moderator endpoint to your environment variables.
        /// </summary>
        private static readonly string AzureBaseURL =
            Environment.GetEnvironmentVariable("CONTENT_MODERATOR_ENDPOINT");

        /// <summary>
        /// Your Content Moderator subscription key.
        /// Add your Azure Content Moderator subscription key to your environment variables.
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
