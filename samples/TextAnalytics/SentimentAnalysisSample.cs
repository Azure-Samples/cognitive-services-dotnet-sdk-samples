using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;

namespace Microsoft.Azure.CognitiveServices.Samples.TextAnalytics
{
    public static class SentimentAnalysisSample
    {
        public static async Task RunAsync(string endpoint, string key)
        {
            var credentials = new ApiKeyServiceClientCredentials(key);
            var client = new TextAnalyticsClient(credentials)
            {
                Endpoint = endpoint
            };

            // The documents to be analyzed. Add the language of the document. The ID can be any value.
            var inputDocuments = new MultiLanguageBatchInput(
                new List<MultiLanguageInput>
                {
                    new MultiLanguageInput("en", "1", "I had the best day of my life."),
                    new MultiLanguageInput("en", "2", "This was a waste of my time. The speaker put me to sleep."),
                    new MultiLanguageInput("es", "3", "No tengo dinero ni nada que dar..."),
                    new MultiLanguageInput("it", "4", "L'hotel veneziano era meraviglioso. È un bellissimo pezzo di architettura."),
                });

            var result = await client.SentimentAsync(false, inputDocuments);

            // Printing sentiment results
            foreach (var document in result.Documents)
            {
                Console.WriteLine($"Document ID: {document.Id} , Sentiment Score: {document.Score:0.00}");
            }
        }
    }
}
