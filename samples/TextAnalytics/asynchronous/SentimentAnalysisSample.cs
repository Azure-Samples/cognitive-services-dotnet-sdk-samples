using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.AI.TextAnalytics.Samples
{
    public static class SentimentAnalysisSample
    {
        public static async Task RunAsync(string endpoint, string apiKey)
        {
            var client = new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

            // The documents to be analyzed. Add the language of the document. The ID can be any value.
            var inputDocuments = new List<TextDocumentInput>
                {
                    new TextDocumentInput("1", "I had the best day of my life.")
                     {
                        Language = "en",
                     },
                    new TextDocumentInput("2", "This was a waste of my time. The speaker put me to sleep.")
                     {
                        Language = "en",
                     },
                    new TextDocumentInput("3", "No tengo dinero ni nada que dar...")
                     {
                        Language = "es",
                     },
                    new TextDocumentInput("4", "L'hotel veneziano era meraviglioso. È un bellissimo pezzo di architettura.")
                    {
                        Language = "it",
                     },
                };

            var result = await client.AnalyzeSentimentBatchAsync(inputDocuments);

            // Printing sentiment results
            Console.WriteLine("===== Sentiment Analysis =====\n");

            foreach (var document in result.Value)
            {
                Console.WriteLine($"Document ID: {document.Id} , Sentiment: {document.DocumentSentiment.Sentiment}");
                Console.WriteLine($"    Positive score: {document.DocumentSentiment.ConfidenceScores.Positive}.");
                Console.WriteLine($"    Neutral score: {document.DocumentSentiment.ConfidenceScores.Neutral}.");
                Console.WriteLine($"    Negative score: {document.DocumentSentiment.ConfidenceScores.Negative}.");
            }
            Console.WriteLine();
        }
    }
}
