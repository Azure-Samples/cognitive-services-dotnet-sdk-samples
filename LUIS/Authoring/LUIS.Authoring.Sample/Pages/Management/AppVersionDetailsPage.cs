namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.Management
{
    using Language.LUIS.Authoring;
    using System;

    class AppVersionDetailsPage : BasePage, IAppVersionPage
    {
        public Guid AppId { get; set; }
        public string VersionId { get; set; }

        public AppVersionDetailsPage(BaseProgram program) : base("Details", program)
        { }

        public override void Display()
        {
            base.Display();

            var info = AwaitTask(Client.Versions.GetWithHttpMessagesAsync(AppId, VersionId), true).Body;

            Print(info);

            WaitForGoBack();
        }
    }
}
