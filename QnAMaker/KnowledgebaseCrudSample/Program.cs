namespace Microsoft.Azure.CognitiveServices.Samples.QnAMaker
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            // API details
            string apiKey = "64b3360653434bc09da533cc35349390";
            string endpoint = "https://westus.api.cognitive.microsoft.com";

            // Run Sample
            KnowledgebaseCrudSample.Run(apiKey, endpoint).Wait();

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }
}
