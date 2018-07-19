namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.Management
{
    using EasyConsole;
    using System;

    class AppVersionInfoPage : BaseMenuPage, IAppVersionPage
    {
        public Guid AppId { get; set; }
        public string VersionId { get; set; }

        public AppVersionInfoPage(BaseProgram program) : base("Details", program)
        { }

        public override void Display()
        {
            Menu = new Menu();

            SafeAddToMenu("View Details",
                () => NavigateToVersion<AppVersionDetailsPage>());

            SafeAddToMenu("View Intent", 
                () => NavigateToVersion<AppVersionIntentsPage>());

            SafeAddToMenu("View Entities",
                () => NavigateToVersion<AppVersionEntitiesPage>());

            SafeAddToMenu("View Prebuilt Entities",
                () => NavigateToVersion<AppVersionPrebuiltEntitiesPage>());

            base.Display();
        }
        
        private void NavigateToVersion<T>() where T : Page, IAppVersionPage
        {
            NavigateWithInitializer<T>(page => {
                page.AppId = AppId;
                page.VersionId = VersionId;
            });
        }
    }
}
