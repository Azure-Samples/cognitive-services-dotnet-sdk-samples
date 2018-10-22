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

            VerifyFaceToFace.Run(endpoint, apiKey).Wait();

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }
}
