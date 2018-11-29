﻿namespace Microsoft.Azure.CognitiveServices.Samples.QnAMaker
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            // API details
            string apiKey = "API KEY HERE";
            string endpoint = "https://westus.api.cognitive.microsoft.com";

            // Run Sample
            var kbSample = new KnowledgebaseCrudSample(apiKey, endpoint);
            kbSample.Run().Wait();

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }
}
