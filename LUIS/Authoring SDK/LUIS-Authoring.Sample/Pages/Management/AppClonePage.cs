namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.Management
{
    using EasyConsole;
    using Language.LUIS.Authoring;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
    using System;

    class AppClonePage : BasePage, IAppPage
    {
        public Guid AppId { get; set; }

        public AppClonePage(BaseProgram program) : base("Clone", program)
        { }

        public override void Display()
        {
            base.Display();

            Console.WriteLine("Preparing app to clone...");

            var versions = AwaitTask(Client.Versions.ListWithHttpMessagesAsync(AppId)).Body;

            Console.WriteLine("Select version to clone");
            var versionId = "";
            var menuVersion = new Menu();
            foreach (var version in versions)
            {
                menuVersion.Add($"v{version.Version} [{version.TrainingStatus}]", () => versionId = version.Version);
            }
            menuVersion.Display();

            var verName = Input.ReadString("Enter the new version tag: ").Trim();

            var versionToClone = new TaskUpdateObject
            {
                Version = verName
            };

            Console.WriteLine("Cloning app...");

            try
            {
                var result = AwaitTask(Client.Versions.CloneWithHttpMessagesAsync(AppId, versionId, versionToClone)).Body;
                Console.WriteLine("Your app has been cloned.");
                Print(result);
            }
            catch (Exception ex)
            {
                var message = (ex as ErrorResponseException)?.Body.Message ?? ex.Message;
                Console.WriteLine($"Your app is not ready to be cloned. Err: {message}");
            }

            WaitForGoBack();
        }
    }
}
