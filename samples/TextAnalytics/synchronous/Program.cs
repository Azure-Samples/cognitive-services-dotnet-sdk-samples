/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for
 * license information.
*/

// <imports>
using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Rest;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
// </imports>

namespace text_analytics_quickstart
{
    class Program
    {
        // <vars>
        private static readonly string apiKey = "<paste-your-text-analytics-key-here>";
        private static readonly string endpoint = "<paste-your-text-analytics-endpoint-here>";
        // </vars>

        // <authentication>
        static TextAnalyticsClient authenticateClient()
        {
            var client = new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
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
        static void sentimentAnalysisExample(TextAnalyticsClient client)
        {
            var result = client.AnalyzeSentiment("I had the best day of my life.", "en");
            Console.WriteLine($"    Sentiment: {result.Value.Sentiment}");
            Console.WriteLine($"    Positive confidence score: {result.Value.ConfidenceScores.Positive}.");
            Console.WriteLine($"    Neutral confidence score: {result.Value.ConfidenceScores.Neutral}.");
            Console.WriteLine($"    Negative confidence score: {result.Value.ConfidenceScores.Negative}.");
        }
        // </sentiment>

        // <languageDetection>
        static void languageDetectionExample(TextAnalyticsClient client)
        {
            var result = client.DetectLanguage("This is a document written in English.");
            Console.WriteLine($"Language: {result.Value.Name}");
        }
        // </languageDetection>

        // <entityRecognition>
        static void entityRecognitionExample(TextAnalyticsClient client)
        {

            var result = client.RecognizeLinkedEntities("Microsoft was founded by Bill Gates and Paul Allen on April 4, 1975, to develop and sell BASIC interpreters for the Altair 8800.");
            Console.WriteLine("Entities:");
            foreach (var entity in result.Value)
            {
                Console.WriteLine(value: $"\t\tName: {entity.Name},\tLanguage: {entity.Language},\tDataSource: {entity.DataSource},\tUrl: {entity.Url},\tDataSourceEntityId: {entity.DataSourceEntityId}");
                foreach (var match in entity.Matches)
                {
                    Console.WriteLine($"\t\t\tText: {match.Text},\tLength: {match.Text.Length},\tScore: {match.ConfidenceScore:F3}");
                }
            }
        }
        // </entityRecognition>

        // <keyPhraseExtraction>
        static void keyPhraseExtractionExample(TextAnalyticsClient client)
        {
            var result = client.ExtractKeyPhrases("My cat might need to see a veterinarian.");

            // Printing key phrases
            Console.WriteLine("Key phrases:");

            foreach (string keyphrase in result.Value)
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

