using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;

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
                            new LanguageInput(id: "1", text: "This is a document written in English."),
                            new LanguageInput(id: "2", text: "Este es un document escrito en Español."),
                            new LanguageInput(id: "3", text: "这是一个用中文写的文件")
                        });

            var langResults = await client.DetectLanguageAsync(false, inputDocuments);

            // Printing detected languages
            foreach (var document in langResults.Documents)
            {
                Console.WriteLine($"Document ID: {document.Id} , Language: {document.DetectedLanguages[0].Name}");
            }
        }
    }
}
