namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages
{
    using System;
    using EasyConsole;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;

    class CreateAppPage<T>: BasePage where T : Page, IAppVersionPage
    {
        public CreateAppPage(BaseProgram program) : base("Create", program)
        { }

        public override void Display()
        {
            base.Display();

            var defaultAppName = $"Contoso-{DateTime.UtcNow.Ticks}";
            var versionId = "0.1";

            var appName = Input.ReadString($"Enter your App's name: (default {defaultAppName})");

            if (string.IsNullOrWhiteSpace(appName))
            {
                appName = defaultAppName;
            }

            var newApp = new ApplicationCreateObject
            {
                Name = appName,
                InitialVersionId = versionId,
                Description = "New App created with LUIS SDK wizard",
                Culture = "en-us",
                Domain = "",
                UsageScenario = ""
            };

            try {
                var appId = AwaitTask(Client.Apps.AddAsync(newApp));
                Console.WriteLine($"{appName} app created with the id {appId}");


                NavigateWithInitializer<T>((page) => {
                    page.AppId = appId;
                    page.VersionId = versionId;
                });
            }
            catch (Exception)
            {
                Console.WriteLine("Something went wrong. Please enter a different name");
                Input.ReadString("Press any key to continue");
                Program.NavigateTo<CreateAppPage<T>>();
            }
        }
    }
}