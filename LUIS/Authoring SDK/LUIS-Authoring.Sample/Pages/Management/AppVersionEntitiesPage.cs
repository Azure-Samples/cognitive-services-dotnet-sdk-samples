namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.Management
{
    using Language.LUIS.Authoring;
    using System;

    class AppVersionEntitiesPage : BasePage, IAppVersionPage
    {
        public Guid AppId { get; set; }
        public string VersionId { get; set; }

        public AppVersionEntitiesPage(BaseProgram program) : base("Entities", program)
        { }

        public override void Display()
        {
            base.Display();

            var info = AwaitTask(Client.Model.ListEntitiesWithHttpMessagesAsync(AppId, VersionId), true).Body;

            Print(info);

            WaitForGoBack();
        }
    }
}
