// <imports>
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Microsoft.Rest;
// </imports>

namespace text_analytics_quickstart
{
    class Program
    {
        // <vars>
        private const string key_var = "TEXT_ANALYTICS_SUBSCRIPTION_KEY";
        private static readonly string key = Environment.GetEnvironmentVariable(key_var);

        private const string endpoint_var = "TEXT_ANALYTICS_ENDPOINT";
        private static readonly string endpoint = Environment.GetEnvironmentVariable(endpoint_var);
        // </vars>

        static Program()
        {
            if (null == key)
            {
                throw new Exception("Please set/export the environment variable: " + key_var);
            }
            if (null == endpoint)
            {
                throw new Exception("Please set/export the environment variable: " + endpoint_var);
            }
        }

        // <authentication>
        static TextAnalyticsClient authenticateClient()
        {
            ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(key);
            TextAnalyticsClient client = new TextAnalyticsClient(credentials)
            {
                Endpoint = endpoint
            };
            return client;
        }
        // </authentication>

        // <main>
        static void Main(string[] args)
        {
            var client = authenticateClient();

            sentimentAnalysisExample(client);
            languageDetectionExample(client);
            entityRecognitionExample(client);
            keyPhraseExtractionExample(client);
            Console.Write("Press any key to exit.");
            Console.ReadKey();
        }
        // </main>

        // <sentiment>
        static void sentimentAnalysisExample(ITextAnalyticsClient client)
        {
            var result = client.Sentiment("I had the best day of my life.", "en");
            Console.WriteLine($"Sentiment Score: {result.Score:0.00}");
        }
        // </sentiment>

        // <languageDetection>
        static void languageDetectionExample(ITextAnalyticsClient client)
        {
            var result = client.DetectLanguage("This is a document written in English.");
            Console.WriteLine($"Language: {result.DetectedLanguages[0].Name}");
        }
        // </languageDetection>

        // <entityRecognition>
        static void entityRecognitionExample(ITextAnalyticsClient client)
        {

            var result = client.Entities("Microsoft was founded by Bill Gates and Paul Allen on April 4, 1975, to develop and sell BASIC interpreters for the Altair 8800.");
            Console.WriteLine("Entities:");
            foreach (var entity in result.Entities)
            {
                Console.WriteLine($"\tName: {entity.Name},\tType: {entity.Type ?? "N/A"},\tSub-Type: {entity.SubType ?? "N/A"}");
                foreach (var match in entity.Matches)
                {
                    Console.WriteLine($"\t\tOffset: {match.Offset},\tLength: {match.Length},\tScore: {match.EntityTypeScore:F3}");
                }
            }
        }
        // </entityRecognition>

        // <keyPhraseExtraction>
        static void keyPhraseExtractionExample(TextAnalyticsClient client)
        {
            var result = client.KeyPhrases("My cat might need to see a veterinarian.");

            // Printing key phrases
            Console.WriteLine("Key phrases:");

            foreach (string keyphrase in result.KeyPhrases)
            {
                Console.WriteLine($"\t{keyphrase}");
            }
        }
        // </keyPhraseExtraction>
    }

    // <clientClass>
    class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        private readonly string apiKey;

        public ApiKeyServiceClientCredentials(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            request.Headers.Add("Ocp-Apim-Subscription-Key", this.apiKey);
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }
    // </clientClass>
}

