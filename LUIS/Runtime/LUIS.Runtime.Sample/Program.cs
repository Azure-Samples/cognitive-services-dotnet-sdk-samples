namespace Microsoft.Azure.CognitiveServices.Language.LUIS.Sample
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;

    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        private static string SubscriptionKey;
        private static string ApplicationId;
        private static string EndPoint;

        static void Main(string[] args)
        {
            ReadConfiguration();

            ShowIntro();

            RecognizeUserInput().Wait();
        }

        static async Task RecognizeUserInput()
        {
            while (true)
            {
                // Read the text to recognize
                Console.WriteLine("Enter the text to recognize:");
                string input = Console.ReadLine().Trim();

                if (input.ToLower() == "exit")
                {
                    // Close application if user types "exit"
                    break;
                }
                else
                {
                    if (input.Length > 0)
                    {
                        // Create client with SuscriptionKey and AzureRegion
                        var client = new LUISRuntimeClient(new ApiKeyServiceClientCredentials(SubscriptionKey));
                        client.Endpoint = EndPoint;

                        // Predict
                        try
                        {
                            var result = await client.Prediction.ResolveAsync(ApplicationId, input);

                            // Print result
                            var json = JsonConvert.SerializeObject(result, Formatting.Indented);
                            Console.WriteLine(json);
                            Console.WriteLine();

                        }
                        catch (Exception)
                        {
                            Console.WriteLine("\nSomething went wrong. Please Make sure your app is published and try again.\n");
                        }

                    }
                }
            }
        }

        static void ReadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            EndPoint = Configuration["LUIS.EndPoint"];
            if (string.IsNullOrWhiteSpace(EndPoint))
            {
                throw new ArgumentException("Missing \"LUIS.Region\" in appsettings.json");
            }

            SubscriptionKey = Configuration["LUIS.SubscriptionKey"];
            ApplicationId = Configuration["LUIS.ApplicationId"];

            if (string.IsNullOrWhiteSpace(SubscriptionKey))
            {
                throw new ArgumentException("Missing \"LUIS.SubscriptionKey\" in appsettings.json");
            }

            if (string.IsNullOrWhiteSpace(ApplicationId))
            {
                throw new ArgumentException("Missing \"LUIS.ApplicationId\" in appsettings.json");
            }
        }

        static void ShowIntro()
        {
            Console.WriteLine("Welcome to the LUIS Sample console application!");
            Console.WriteLine("To try it out enter a phrase and let us show you the different outputs for the recognized intent and entities, or just type 'exit' to leave the application.");
            Console.WriteLine();
            Console.WriteLine("Here are some examples you could try:");
            Console.WriteLine();
            Console.WriteLine("\" Search for hotel in Seattle\"");
            Console.WriteLine("\" Look for hotels in Miami\"");
            Console.WriteLine("\" Look for hotels near LAX airport\"");
            Console.WriteLine("\" Show me the reviews of the amazing bot resort\"");
            Console.WriteLine("\" What are the available options?\"");
            Console.WriteLine("\" Help!\"");
            Console.WriteLine();
        }
    }
}
