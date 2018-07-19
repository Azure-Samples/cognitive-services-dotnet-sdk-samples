namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.Management
{
    using EasyConsole;
    using Language.LUIS.Authoring;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    class AppTrainPage : BasePage, IAppPage
    {
        public Guid AppId { get; set; }

        public AppTrainPage(BaseProgram program) : base("Train", program)
        { }

        private string[] trainedStatus = new string[] { "UpToDate", "Success" };

        public override void Display()
        {
            base.Display();

            Console.WriteLine("Preparing app to train...");

            var versions = AwaitTask(Client.Versions.ListWithHttpMessagesAsync(AppId)).Body;

            Console.WriteLine("Select version to train");
            var versionId = "";
            var menuVersion = new Menu();
            foreach (var version in versions)
            {
                menuVersion.Add($"v{version.Version} [{version.TrainingStatus}]", () => versionId = version.Version);
            }
            menuVersion.Display();

            AwaitTask(Task.Run(async () => {
                var isTrained = false;
                var result = await Client.Train.TrainVersionWithHttpMessagesAsync(AppId, versionId).ConfigureAwait(false);
                isTrained = result.Body.Status.Equals("UpToDate");
                while (!isTrained)
                {
                    await Task.Delay(1000);
                    var status = await Client.Train.GetStatusWithHttpMessagesAsync(AppId, versionId);
                    isTrained = status.Body.All(m => trainedStatus.Contains(m.Details.Status));
                }
            }));

            Console.WriteLine("Your app is trained.");

            WaitForGoBack();
        }
    }
}
