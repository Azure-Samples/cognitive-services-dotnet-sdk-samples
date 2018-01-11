using Microsoft.CognitiveServices.ContentModerator;
using ModeratorHelper;
using Newtonsoft.Json;
using System.IO;
using System.Threading;

namespace TextModeration
{
    class Program
    {
        /// <summary>
        /// The name of the file that contains the text to evaluate.
        /// </summary>
        /// <remarks>You will need to create an input file and update this path
        /// accordingly. Relative paths are ralative the execution directory.</remarks>
        private static string TextFile = "TextFile.txt";

        /// <summary>
        /// The name of the file to contain the output from the evaluation.
        /// </summary>
        /// <remarks>Relative paths are ralative the execution directory.</remarks>
        private static string OutputFile = "TextModerationOutput.txt";

        static void Main(string[] args)
        {
            // Load the input text.
            string text = File.ReadAllText(TextFile);

            text = text.Replace(System.Environment.NewLine, " ");

            // Save the moderation results to a file.
            using (StreamWriter outputWriter = new StreamWriter(OutputFile, false))
            {
                // Create a Content Moderator client and evaluate the text.
                using (var client = Clients.NewClient())
                {
                    // Screen the input text: check for profanity, 
                    // do autocorrect text, and check for personally identifying 
                    // information (PII)
                    outputWriter.WriteLine("Normalize text and autocorrect typos.");
                    var screenResult =
                        client.TextModeration.ScreenText("eng", "text/plain", text, true, true);
                    outputWriter.WriteLine(
                        JsonConvert.SerializeObject(screenResult, Formatting.Indented));
                }
                outputWriter.Flush();
                outputWriter.Close();
            }
        }
    }
}
