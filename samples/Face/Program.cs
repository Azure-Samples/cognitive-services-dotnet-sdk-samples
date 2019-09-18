namespace Microsoft.Azure.CognitiveServices.Samples.Face
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            // Add your Azure Computer Vision subscription key and endpoint to your environment variables
            string subscriptionKey = Environment.GetEnvironmentVariable("COMPUTER_VISION_SUBSCRIPTION_KEY");
            string endpoint = Environment.GetEnvironmentVariable("COMPUTER_VISION_ENDPOINT");

            // Detect sample.
            Detection.Run(endpoint, subscriptionKey).Wait();

            // FindSimilar samples.
            FindSimilarInFaceIds.Run(endpoint, subscriptionKey).Wait();
            FindSimilarInFaceList.Run(endpoint, subscriptionKey).Wait();
            FindSimilarInLargeFaceList.Run(endpoint, subscriptionKey).Wait();

            // Group sample.
            Group.Run(endpoint, subscriptionKey).Wait();

            // Identify sample.
            IdentifyInPersonGroup.Run(endpoint, subscriptionKey).Wait();
            IdentifyInLargePersonGroup.Run(endpoint, subscriptionKey).Wait();

            // Verify samples.
            VerifyFaceToFace.Run(endpoint, subscriptionKey).Wait();
            VerifyInPersonGroup.Run(endpoint, subscriptionKey).Wait();
            VerifyInLargePersonGroup.Run(endpoint, subscriptionKey).Wait();

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }
}
