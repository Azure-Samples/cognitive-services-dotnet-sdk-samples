namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.Management
{
    using Language.LUIS.Authoring;
    using System;

    class AppVersionPrebuiltEntitiesPage : BasePage, IAppVersionPage
    {
        public Guid AppId { get; set; }
        public string VersionId { get; set; }

        public AppVersionPrebuiltEntitiesPage(BaseProgram program) : base("Prebuilt", program)
        { }

        public override void Display()
        {
            base.Display();

            var info = AwaitTask(Client.Model.ListPrebuiltsWithHttpMessagesAsync(AppId, VersionId), true);

            Print(info);

            WaitForGoBack();
        }
    }
}
