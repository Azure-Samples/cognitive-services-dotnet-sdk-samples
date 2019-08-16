using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;

namespace Microsoft.Azure.CognitiveServices.Samples.TextAnalytics
{
    public static class LanguageDetectionSample
    {
        public static async Task RunAsync(string endpoint, string key)
        {
            var credentials = new ApiKeyServiceClientCredentials(key);
            var client = new TextAnalyticsClient(credentials)
            {
                Endpoint = endpoint
            };

            // The documents to be submitted for language detection. The ID can be any value.
            var inputDocuments = new LanguageBatchInput(
                    new List<LanguageInput>
                        {
                            new LanguageInput("1", "This is a document written in English."),
                            new LanguageInput("2", "Este es un document escrito en Español."),
                            new LanguageInput("3", "这是一个用中文写的文件")
                        });

            var langResults = await client.DetectLanguageBatchAsync(inputDocuments);

            // Printing detected languages
            Console.WriteLine("===== Language Detection =====\n");

            foreach (var document in langResults.Documents)
            {
                Console.WriteLine($"Document ID: {document.Id} , Language: {document.DetectedLanguages[0].Name}");
            }
            Console.WriteLine();
        }
    }
}
