using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.AI.TextAnalytics.Samples
{
    public static class RecognizeEntitiesSample
    {
        public static async Task RunAsync(string endpoint, string apiKey)
        {
            var client = new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

            // The documents to be submitted for entity recognition. The ID can be any value.
            var inputDocuments = new List<TextDocumentInput>
                {
                    new TextDocumentInput("1", "Microsoft was founded by Bill Gates and Paul Allen on April 4, 1975, to develop and sell BASIC interpreters for the Altair 8800.")
                    {
                     Language = "en",
                    },
                    new TextDocumentInput("2", "La sede principal de Microsoft se encuentra en la ciudad de Redmond, a 21 kilómetros de Seattle.")
                    {
                     Language = "es",
                    },
                };

            var entitiesResult = await client.RecognizeLinkedEntitiesBatchAsync(inputDocuments);

            // Printing recognized entities
            Console.WriteLine("===== Named Entity Recognition & Entity Linking =====\n");

            foreach (var document in entitiesResult.Value)
            {
                Console.WriteLine($"Document ID: {document.Id} ");

                Console.WriteLine("\t Entities:");

                foreach (var entity in document.Entities)
                {
                    Console.WriteLine(value: $"\t\tName: {entity.Name},\tLanguage: {entity.Language},\tDataSource: {entity.DataSource},\tUrl: {entity.Url},\tDataSourceEntityId: {entity.DataSourceEntityId}");
                    foreach (var match in entity.Matches)
                    {
                        Console.WriteLine($"\t\t\tText: {match.Text},\tLength: {match.Text.Length},\tScore: {match.ConfidenceScore:F3}");
                    }
                    
                }
            }
            Console.WriteLine();
        }
        
    }
}
