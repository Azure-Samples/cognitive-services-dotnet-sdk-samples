using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.AI.TextAnalytics.Samples
{
    public static class KeyPhraseExtractionSample
    {
        public static async Task RunAsync(string endpoint, string apiKey)
        {
            var client = new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            
            var inputDocuments = new List<TextDocumentInput>
                        {
                            new TextDocumentInput("1", "猫は幸せ")
                            {
                                Language = "ja",
                            },
                            new TextDocumentInput("2", "Fahrt nach Stuttgart und dann zum Hotel zu Fu.")
                            {
                                Language = "de",
                            },
                            new TextDocumentInput("3", "My cat might need to see a veterinarian.")
                            {
                                Language = "en",
                            },
                            new TextDocumentInput("4", "A mi me encanta el fútbol!")
                            {
                                Language = "es",
                            },
                        };

            var kpResults = await client.ExtractKeyPhrasesBatchAsync(inputDocuments);

            // Printing keyphrases
            Console.WriteLine("===== Key Phrases Extraction =====\n");

            foreach (var document in kpResults.Value)
            {
                Console.WriteLine($"Document ID: {document.Id} ");

                Console.WriteLine("\t Key phrases:");

                foreach (string keyphrase in document.KeyPhrases)
                {
                    Console.WriteLine($"\t\t{keyphrase}");
                }
            }
            Console.WriteLine();
        }
    }
}
