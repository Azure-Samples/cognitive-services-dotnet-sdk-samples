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
        // <main>
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            string location = "westus2";
            string endpoint = $"https://{location}.api.cognitive.microsoft.com";
            //This sample assumes you have created an environment variable for your key
            string key = Environment.GetEnvironmentVariable("TEXTANALYTICS_SUBSCRIPTION_KEY");
            ITextAnalyticsClient client = new TextAnalyticsClient(new ApiKeyServiceClientCredentials(key));
            
            SentimentAnalysisExample(client);
            // languageDetectionExample(client);
            // RecognizeEntitiesExample(client);
            // KeyPhraseExtractionExample(client);
            Console.ReadLine();
        }
        // </main>

        // <sentiment>
        static void SentimentAnalysisExample(ITextAnalyticsClient client){
            var result = client.Sentiment("I had the best day of my life.", "en");
            Console.WriteLine($"Sentiment Score: {result.Score:0.00}");
        }
        // <sentiment>

        // <language-detection>
        static void languageDetectionExample(ITextAnalyticsClient client){
            var result = client.DetectLanguage("This is a document written in English.");
            Console.WriteLine($"Language: {result.DetectedLanguages[0].Name}");
        }
        // </language-detection>

        // <entity-recognition>
        static void entityRecognitionExample(ITextAnalyticsClient client){
            
            var result = client.Entities("Microsoft was founded by Bill Gates and Paul Allen on April 4, 1975, to develop and sell BASIC interpreters for the Altair 8800.");
            Console.WriteLine("Entities:");
            foreach (var entity in result.Entities){
                Console.WriteLine($"\tName: {entity.Name},\tType: {entity.Type ?? "N/A"},\tSub-Type: {entity.SubType ?? "N/A"}");
                foreach (var match in entity.Matches){
                    Console.WriteLine($"\t\tOffset: {match.Offset},\tLength: {match.Length},\tScore: {match.EntityTypeScore:F3}");
                }
            }
        }
        // </entity-recognition>
    }

    // <client-class>
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
    // </client-class>
}

