namespace Microsoft.Azure.CognitiveServices.Samples.Face
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            // Create a client.
            string apiKey = "ENTER YOUR KEY HERE";
            string endpoint = "ENTER YOUR ENDPOINT HERE";

            // Detect sample.
            Detection.Run(endpoint, apiKey).Wait();

            // FindSimilar samples.
            FindSimilarInFaceIds.Run(endpoint, apiKey).Wait();
            FindSimilarInFaceList.Run(endpoint, apiKey).Wait();
            FindSimilarInLargeFaceList.Run(endpoint, apiKey).Wait();

            // Group sample.
            Group.Run(endpoint, apiKey).Wait();

            // Identify sample.
            IdentifyInPersonGroup.Run(endpoint, apiKey).Wait();
            IdentifyInLargePersonGroup.Run(endpoint, apiKey).Wait();

            // Verify samples.
            VerifyFaceToFace.Run(endpoint, apiKey).Wait();
            VerifyInPersonGroup.Run(endpoint, apiKey).Wait();
            VerifyInLargePersonGroup.Run(endpoint, apiKey).Wait();

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }
}
