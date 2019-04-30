// Note: Add the NuGet package Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring to your solution.
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;

using System;
using System.Threading.Tasks;

namespace LUIS1
{
    class Program
    {
        // NOTE: Replace this with a valid LUIS subscription key.
        static readonly string subscriptionKey = "INSERT SUBSCRIPTION KEY HERE";
        // NOTE: Replace this with your LUIS application's ID.
        static readonly string app_id = "INSERT APP ID HERE";
        // NOTE: Replace this with your LUIS application's version ID.
        static readonly string app_version_id = "INSERT VERSION ID HERE";

        // You must use the same region as you used to get your subscription
        // keys. For example, if you got your subscription keys from westus,
        // replace "westcentralus" with "westus".
        //
        // Free trial subscription keys are generated in the westcentralus
        // region. If you use a free trial subscription key, you shouldn't
        // need to change the region.
        static readonly string region = "westcentralus";

        // Train the application.
        async static Task<EnqueueTrainingResponse> Train_App (LUISAuthoringClient client)
        {
            return await client.Train.TrainVersionAsync(new Guid (app_id), app_version_id);
        }

        // Publish the application.
        async static Task<ProductionOrStagingEndpointInfo> Publish_App (LUISAuthoringClient client)
        {
            ApplicationPublishObject obj = new ApplicationPublishObject
            {
                VersionId = app_version_id,
                IsStaging = true
            };
            return await client.Apps.PublishAsync(new Guid(app_id), obj);
        }

        async static Task RunQuickstart ()
        {
            // Generate the credentials and create the client.
            var credentials = new Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.ApiKeyServiceClientCredentials(subscriptionKey);
            var client = new LUISAuthoringClient(credentials, new System.Net.Http.DelegatingHandler[] { })
            {
                Endpoint = "https://" + region + ".api.cognitive.microsoft.com"
            };

            var response = await Train_App(client);
            Console.WriteLine("Training status: " + response.Status);

            // Publish the application and show the endpoint URL for the published application.
            var info = await Publish_App(client);
            Console.WriteLine("Endpoint URL: " + info.EndpointUrl);
        }

        static void Main(string[] args)
        {
            Task.WaitAll (RunQuickstart());
            Console.WriteLine("Press any key to exit.");
            Console.Read();
        }
    }
}
