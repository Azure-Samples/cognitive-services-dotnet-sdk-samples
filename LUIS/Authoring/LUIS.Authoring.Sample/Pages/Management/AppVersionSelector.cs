namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.Management
{
    using System;
    using EasyConsole;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;

    class AppVersionSelector : BaseMenuPage, IAppPage
    {
        public Guid AppId { get; set; }

        public AppVersionSelector(BaseProgram program) : base("Version", program)
        { }

        public override void Display()
        {
            Menu = new Menu();
            var versions = AwaitTask(Client.Versions.ListWithHttpMessagesAsync(AppId)).Body;
            foreach (var version in versions)
            {
                SafeAddToMenu($"v{version.Version} [{version.TrainingStatus}]", () => NavigateToVersion(version.Version));
            }

            base.Display();
        }

        private void NavigateToVersion(string versionId)
        {
            NavigateWithInitializer<AppVersionInfoPage>(page => {
                page.AppId = AppId;
                page.VersionId = versionId;
            });
        }
    }
}
