namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.Management
{
    using EasyConsole;
    using Language.LUIS.Authoring;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
    using System;

    class AppPublishPage : BasePage, IAppPage
    {
        public Guid AppId { get; set; }
        private string AuthoringKey { get; set; }

        public AppPublishPage(BaseProgram program) : base("Publish", program)
        {
            AuthoringKey = program.AuthoringKey;
        }

        public override void Display()
        {
            base.Display();

            Console.WriteLine("Preparing app to publish...");

            var versions = AwaitTask(Client.Versions.ListWithHttpMessagesAsync(AppId)).Body;

            Console.WriteLine("Select version to publish");
            var versionId = "";
            var menuVersion = new Menu();
            foreach (var version in versions)
            {
                menuVersion.Add($"v{version.Version} [{version.TrainingStatus}]", () => versionId = version.Version);
            }
            menuVersion.Display();

            var isStaging = Input.ReadString("Publish on staging? Y/N").Trim().ToLowerInvariant().StartsWith("y");

            var versionToPublish = new ApplicationPublishObject
            {
                Region = "westus",
                VersionId = versionId,
                IsStaging = isStaging
            };

            Console.WriteLine("Publishing app...");

            try
            {
                var result = AwaitTask(Client.Apps.PublishWithHttpMessagesAsync(AppId, versionToPublish)).Body;
                Console.WriteLine("Your app has been published.");
                result.EndpointUrl += "?subscription-key=" + AuthoringKey + "&q=";
                Print(result);
            }
            catch (Exception ex)
            {
                var message = (ex as ErrorResponseException)?.Body.Message ?? ex.Message;
                Console.WriteLine($"Your app is not ready to be published. Err: {message}");
            }

            WaitForGoBack();
        }
    }
}
