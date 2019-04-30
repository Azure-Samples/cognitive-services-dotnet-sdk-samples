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

namespace LUIS2
{
    struct ApplicationInfo
    {
        public Guid ID;
        public string Version;
    }

    class Program
    {
        // NOTE: Replace this with a valid LUIS subscription key.
        static readonly string subscriptionKey = "INSERT SUBSCRIPTION KEY HERE";

        // NOTE: Replace this with the region for your LUIS subscription key.
        static readonly string region = "westus";

        // NOTE: To work with an existing LUIS application, add the application ID here.
        // To create a new LUIS application, leave this value empty.
        static readonly string app_id = "";

        /* Create a new LUIS application or get an existing one, depending on whether app_id
         * is specified. Return the application ID and version.
         */
        async static Task<ApplicationInfo> GetOrCreateApplication (LUISAuthoringClient client)
        {
            if (String.IsNullOrEmpty(app_id))
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
                Console.WriteLine("Created new LUIS application {0}\n with ID {1}", app_info.Name, app_id);
                Console.WriteLine("Make a note of this ID to use this application with other samples.\n");
                return new ApplicationInfo() { ID = app_id, Version = app_version };
            }
            else
            {
                var app_info = await client.Apps.GetAsync(new Guid(app_id));
                var app_version = app_info.ActiveVersion;
                Console.WriteLine("Using existing LUIS application", app_info.Name, app_version);
                return new ApplicationInfo() { ID = new Guid(app_id), Version = app_version };
            }
        }

        // Add entities to the LUIS application.
        async static Task AddEntities (LUISAuthoringClient client, ApplicationInfo app_info)
        {
            await client.Model.AddEntityAsync(app_info.ID, app_info.Version, new ModelCreateObject ()
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
        async static Task AddIntents (LUISAuthoringClient client, ApplicationInfo app_info)
        {
            await client.Model.AddIntentAsync(app_info.ID, app_info.Version, new ModelCreateObject()
            {
                Name = "FindFlights"
            });
            Console.WriteLine("Created intent FindFlights");
        }

        // Create a label to be added to an utterance.
        static EntityLabelObject CreateLabel (string utterance, string key, string value)
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
        static ExampleLabelObject CreateUtterance (string intent, string utterance, Dictionary<string, string> labels)
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

        async static Task RunQuickstart()
        {
            var credentials = new Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.ApiKeyServiceClientCredentials(subscriptionKey);
            var client = new LUISAuthoringClient(credentials, new System.Net.Http.DelegatingHandler[] { })
            {
                Endpoint = "https://" + region + ".api.cognitive.microsoft.com"
            };

            var app_info = await GetOrCreateApplication(client);
            await AddEntities(client, app_info);
            await AddIntents(client, app_info);
            await AddUtterances(client, app_info);
            Console.WriteLine("You can now train and publish this LUIS application (see Publish.cs).");
        }

        static void Main(string[] args)
        {
            Task.WaitAll (RunQuickstart());
            Console.WriteLine("Press any key to exit.");
            Console.Read();
        }
    }
}
