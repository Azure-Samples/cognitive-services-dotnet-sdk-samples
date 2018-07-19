namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.Management
{
    using EasyConsole;
    using System;

    class AppInfoPage : BaseMenuPage, IAppPage
    {
        public Guid AppId { get; set; }

        public AppInfoPage(BaseProgram program) : base("Details", program)
        { }

        public override void Display()
        {
            Menu = new Menu();

            SafeAddToMenu("View Details",
                () => NavigateWithInitializer<AppDetailsPage>(page => page.AppId = AppId));

            SafeAddToMenu("Version SubMenu",
                () => NavigateWithInitializer<AppVersionSelector>(p => p.AppId = AppId));

            SafeAddToMenu("Train",
                () => NavigateWithInitializer<AppTrainPage>(p => p.AppId = AppId));

            SafeAddToMenu("Publish",
                () => NavigateWithInitializer<AppPublishPage>(p => p.AppId = AppId));

            SafeAddToMenu("Clone",
                () => NavigateWithInitializer<AppClonePage>(p => p.AppId = AppId));

            SafeAddToMenu("Export",
                () => NavigateWithInitializer<AppExportPage>(p => p.AppId = AppId));

            base.Display();
        }
    }
}
