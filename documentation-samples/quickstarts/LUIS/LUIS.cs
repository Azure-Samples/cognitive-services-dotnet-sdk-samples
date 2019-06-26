// Note: Add the NuGet package Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring to your solution.
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/*
 * This sample builds a LUIS application, entities, and intents using the LUIS .NET SDK.
 * A separate sample trains and publishes the application.
 * 
 * Be sure you understand how LUIS models work.  In particular, know what
 * intents, entities, and utterances are, and how they work together in the
 * context of a LUIS app. See the following:
 * 
 * https://www.luis.ai/welcome
 * https://docs.microsoft.com/azure/cognitive-services/luis/luis-concept-intent
 * https://docs.microsoft.com/azure/cognitive-services/luis/luis-concept-entity-types
 * https://docs.microsoft.com/azure/cognitive-services/luis/luis-concept-utterance
 */

namespace LUIS_CS
{
    struct ApplicationInfo
    {
        public Guid ID;
        public string Version;
    }

    class Program
    {
        private const string key_var = "LUIS_SUBSCRIPTION_KEY";
        private static readonly string subscription_key = Environment.GetEnvironmentVariable(key_var);

        // Note you must use the same region as you used to get your subscription key.
        private const string region_var = "LUIS_REGION";
        private static readonly string region = Environment.GetEnvironmentVariable(region_var);
        private static readonly string endpoint = "https://" + region + ".api.cognitive.microsoft.com";

        static Program()
        {
            if (null == subscription_key)
            {
                throw new Exception("Please set/export the environment variable: " + key_var);
            }
            if (null == region)
            {
                throw new Exception("Please set/export the environment variable: " + region_var);
            }
        }

        // Create a new LUIS application. Return the application ID and version.
        async static Task<ApplicationInfo> CreateApplication(LUISAuthoringClient client)
        {
            string app_version = "0.1";
            var app_info = new ApplicationCreateObject()
            {
                Name = String.Format("Contoso {0}", DateTime.Now),
                InitialVersionId = app_version,
                Description = "Flight booking app built with LUIS .NET SDK.",
                Culture = "en-us"
            };
            var app_id = await client.Apps.AddAsync(app_info);
            Console.WriteLine("Created new LUIS application {0}\n with ID {1}.", app_info.Name, app_id);
            return new ApplicationInfo() { ID = app_id, Version = app_version };
        }

        // Add entities to the LUIS application.
        async static Task AddEntities(LUISAuthoringClient client, ApplicationInfo app_info)
        {
            await client.Model.AddEntityAsync(app_info.ID, app_info.Version, new ModelCreateObject()
            {
                Name = "Destination"
            });
            await client.Model.AddHierarchicalEntityAsync(app_info.ID, app_info.Version, new HierarchicalEntityModel()
            {
                Name = "Class",
                Children = new List<string>() { "First", "Business", "Economy" }
            });
            await client.Model.AddCompositeEntityAsync(app_info.ID, app_info.Version, new CompositeEntityModel()
            {
                Name = "Flight",
                Children = new List<string>() { "Class", "Destination" }
            });
            Console.WriteLine("Created entities Destination, Class, Flight.");
        }

        // Add intents to the LUIS application.
        async static Task AddIntents(LUISAuthoringClient client, ApplicationInfo app_info)
        {
            await client.Model.AddIntentAsync(app_info.ID, app_info.Version, new ModelCreateObject()
            {
                Name = "FindFlights"
            });
            Console.WriteLine("Created intent FindFlights");
        }

        // Create a label to be added to an utterance.
        static EntityLabelObject CreateLabel(string utterance, string key, string value)
        {
            var start_index = utterance.IndexOf(value, StringComparison.InvariantCultureIgnoreCase);
            return new EntityLabelObject()
            {
                EntityName = key,
                StartCharIndex = start_index,
                EndCharIndex = start_index + value.Length
            };
        }

        // Create an utterance to be added to the LUIS application.
        static ExampleLabelObject CreateUtterance(string intent, string utterance, Dictionary<string, string> labels)
        {
            var entity_labels = labels.Select(kv => CreateLabel(utterance, kv.Key, kv.Value)).ToList();
            return new ExampleLabelObject()
            {
                IntentName = intent,
                Text = utterance,
                EntityLabels = entity_labels
            };
        }

        // Add utterances to the LUIS application.
        async static Task AddUtterances(LUISAuthoringClient client, ApplicationInfo app_info)
        {
            var utterances = new List<ExampleLabelObject>()
            {
                CreateUtterance ("FindFlights", "find flights in economy to Madrid", new Dictionary<string, string>() { {"Flight", "economy to Madrid"}, { "Destination", "Madrid" }, { "Class", "economy" } }),
                CreateUtterance ("FindFlights", "find flights to London in first class", new Dictionary<string, string>() { { "Flight", "London in first class" }, { "Destination", "London" }, { "Class", "first" } })
            };
            await client.Examples.BatchAsync(app_info.ID, app_info.Version, utterances);
        }

        // Train a LUIS application.
        async static Task Train_App(LUISAuthoringClient client, ApplicationInfo app)
        {
            var response = await client.Train.TrainVersionAsync(app.ID, app.Version);
            Console.WriteLine("Training status: " + response.Status);
        }

        // Publish a LUIS application and show the endpoint URL for the published application.
        async static Task Publish_App(LUISAuthoringClient client, ApplicationInfo app)
        {
            ApplicationPublishObject obj = new ApplicationPublishObject
            {
                VersionId = app.Version,
                IsStaging = true
            };
            var info = await client.Apps.PublishAsync(app.ID, obj);
            Console.WriteLine("Endpoint URL: " + info.EndpointUrl);
        }

        async static Task RunQuickstart()
        {
            // Generate the credentials and create the client.
            var credentials = new Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.ApiKeyServiceClientCredentials(subscription_key);
            var client = new LUISAuthoringClient(credentials, new System.Net.Http.DelegatingHandler[] { })
            {
                Endpoint = "https://" + region + ".api.cognitive.microsoft.com"
            };

            Console.WriteLine("Creating application...");
            var app = await CreateApplication(client);
            Console.WriteLine();

            Console.WriteLine("Adding entities to application...");
            await AddEntities(client, app);
            Console.WriteLine();

            Console.WriteLine("Adding intents to application...");
            await AddIntents(client, app);
            Console.WriteLine();

            Console.WriteLine("Adding utterances to application...");
            await AddUtterances(client, app);
            Console.WriteLine();

            Console.WriteLine("Training application...");
            await Train_App(client, app);
            Console.WriteLine("Waiting 30 seconds for training to complete...");
            System.Threading.Thread.Sleep(30000);
            Console.WriteLine();

            Console.WriteLine("Publishing application...");
            await Publish_App(client, app);
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            Task.WaitAll(RunQuickstart());
            Console.WriteLine("Press any key to exit.");
            Console.Read();
        }
    }
}
