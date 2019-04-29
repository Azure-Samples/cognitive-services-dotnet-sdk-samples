using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;

namespace Microsoft.Azure.CognitiveServices.Samples.TextAnalytics
{
    public static class KeyPhraseExtractionSample
    {
        public static async Task RunAsync(string endpoint, string key)
        {
            var credentials = new ApiKeyServiceClientCredentials(key);
            var client = new TextAnalyticsClient(credentials)
            {
                Endpoint = endpoint
            };

            var inputDocuments = new MultiLanguageBatchInput(
                        new List<MultiLanguageInput>
                        {
                            new MultiLanguageInput("ja", "1", "猫は幸せ"),
                            new MultiLanguageInput("de", "2", "Fahrt nach Stuttgart und dann zum Hotel zu Fu."),
                            new MultiLanguageInput("en", "3", "My cat might need to see a veterinarian."),
                            new MultiLanguageInput("es", "4", "A mi me encanta el fútbol!")
                        });

            var kpResults = await client.KeyPhrasesAsync(false, inputDocuments);

            // Printing keyphrases
            foreach (var document in kpResults.Documents)
            {
                Console.WriteLine($"Document ID: {document.Id} ");

                Console.WriteLine("\t Key phrases:");

                foreach (string keyphrase in document.KeyPhrases)
                {
                    Console.WriteLine($"\t\t{keyphrase}");
                }
            }
        }
    }
}
