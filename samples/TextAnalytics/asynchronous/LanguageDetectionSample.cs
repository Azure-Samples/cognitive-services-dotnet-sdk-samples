using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.AI.TextAnalytics.Samples
{
    public static class LanguageDetectionSample
    {
        public static async Task RunAsync(string endpoint, string apiKey)
        {
            var client = new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

            // The documents to be submitted for language detection. The ID can be any value.
            var inputDocuments = new List<DetectLanguageInput>
                        {
                            new DetectLanguageInput("1", "This is a document written in English."),
                            new DetectLanguageInput("2", "Este es un document escrito en Español."),
                            new DetectLanguageInput("3", "这是一个用中文写的文件")
                        };

            var langResults = await client.DetectLanguageBatchAsync(inputDocuments);

            // Printing detected languages
            Console.WriteLine("===== Language Detection =====\n");

            foreach (var document in langResults.Value)
            {
                Console.WriteLine($"Document ID: {document.Id} , Language: {document.PrimaryLanguage.Name}");
            }
            Console.WriteLine();
        }
    }
}
