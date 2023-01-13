using System;

namespace Azure.AI.TextAnalytics.Samples
{
    public static class Program
    {
        private const string SubscriptionKey = "";

        //Replace 'westus' with the correct region for your Text Analytics subscription
        private const string Endpoint = "https://westus.api.cognitive.microsoft.com";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            SentimentAnalysisSample.RunAsync(Endpoint, SubscriptionKey).Wait();
            LanguageDetectionSample.RunAsync(Endpoint, SubscriptionKey).Wait();
            RecognizeEntitiesSample.RunAsync(Endpoint, SubscriptionKey).Wait();
            KeyPhraseExtractionSample.RunAsync(Endpoint, SubscriptionKey).Wait();

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }
}
