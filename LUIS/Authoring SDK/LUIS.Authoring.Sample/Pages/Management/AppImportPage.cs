namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.Management
{
    using EasyConsole;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
    using Newtonsoft.Json;
    using System;
    using System.IO;

    class AppImportPage : BasePage
    {
        public AppImportPage(BaseProgram program) : base("Import App", program)
        { }

        public override void Display()
        {
            base.Display();

            var defaultAppName = $"Contoso-{DateTime.UtcNow.Ticks}";

            var appName = Input.ReadString($"Enter your App's name: (default {defaultAppName})");
            if (string.IsNullOrWhiteSpace(appName))
            {
                appName = defaultAppName;
            }

            var path = Input.ReadString("Enter the path to the file to import: ").Trim();

            LuisApp app = null;
            try
            {
                var import = File.ReadAllText(path);
                app = JsonConvert.DeserializeObject<LuisApp>(import);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading the import file. Err: {ex.Message}");
            }

            if (app != null)
            {
                Console.WriteLine("Importing app...");

                try
                {
                    var result = AwaitTask(Client.Apps.ImportAsync(app, appName));
                    Console.WriteLine("Your app has been imported.");
                    Print(result);
                }
                catch (Exception ex)
                {
                    var message = (ex as ErrorResponseException)?.Body.Message ?? ex.Message;
                    Console.WriteLine($"Error importing the application. Err: {message}");
                }
            }

            WaitForGoBack();
        }
    }
}
