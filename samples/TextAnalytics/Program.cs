    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
    using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
    using Microsoft.Rest;

namespace Microsoft.Azure.CognitiveServices.Samples.TextAnalytics
{
    /// <summary>
    /// Allows authentication to the API using a basic apiKey mechanism
    /// </summary>
    class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        private readonly string subscriptionKey;

        /// <summary>
        /// Creates a new instance of the ApiKeyServiceClientCredentails class
        /// </summary>
        /// <param name="subscriptionKey">The subscription key to authenticate and authorize as</param>
        public ApiKeyServiceClientCredentials(string subscriptionKey)
        {
            this.subscriptionKey = subscriptionKey;
        }

        /// <summary>
        /// Add the Basic Authentication Header to each outgoing request
        /// </summary>
        /// <param name="request">The outgoing request</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            request.Headers.Add("Ocp-Apim-Subscription-Key", this.subscriptionKey);

            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }

    public static class Program
    {
        private const string SubscriptionKey = "";

        static void Main(string[] args)
        {
            var credentials = new ApiKeyServiceClientCredentials(SubscriptionKey);
            var client = new TextAnalyticsClient(credentials)
            {
                //Replace 'westus' with the correct region for your Text Analytics subscription
                Endpoint = "https://westus.api.cognitive.microsoft.com"
            };

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            SentimentAnalysisExample(client).Wait();
            DetectLanguageExample(client).Wait();
            RecognizeEntitiesExample(client).Wait();
            KeyPhraseExtractionExample(client).Wait();
            Console.ReadLine();
        }

        public static async Task SentimentAnalysisExample(TextAnalyticsClient client)
        {

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

        public static async Task DetectLanguageExample(TextAnalyticsClient client)
        {
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

        public static async Task RecognizeEntitiesExample(TextAnalyticsClient client)
        {
            // The documents to be submitted for entity recognition. The ID can be any value.
            var inputDocuments = new MultiLanguageBatchInput(
                new List<MultiLanguageInput>
                {
                    new MultiLanguageInput("en", "1", "Microsoft was founded by Bill Gates and Paul Allen on April 4, 1975, to develop and sell BASIC interpreters for the Altair 8800."),
                    new MultiLanguageInput("es", "2", "La sede principal de Microsoft se encuentra en la ciudad de Redmond, a 21 kilómetros de Seattle.")
                });

            var entitiesResult = await client.EntitiesAsync(false, inputDocuments);

            // Printing recognized entities
            foreach (var document in entitiesResult.Documents)
            {
                Console.WriteLine($"Document ID: {document.Id} ");

                Console.WriteLine("\t Entities:");

                foreach (var entity in document.Entities)
                {
                    Console.WriteLine($"\t\tName: {entity.Name},\tType: {entity.Type ?? "N/A"},\tSub-Type: {entity.SubType ?? "N/A"}");
                    foreach (var match in entity.Matches)
                    {
                        Console.WriteLine($"\t\t\tOffset: {match.Offset},\tLength: {match.Length},\tScore: {match.EntityTypeScore:F3}");
                    }
                }
            }
        }

        public static async Task KeyPhraseExtractionExample(TextAnalyticsClient client)
        {
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
