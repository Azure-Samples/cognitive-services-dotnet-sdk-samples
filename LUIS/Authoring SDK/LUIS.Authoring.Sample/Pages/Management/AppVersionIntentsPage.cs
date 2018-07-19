namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.Management
{
    using Language.LUIS.Authoring;
    using System;

    class AppVersionIntentsPage : BasePage, IAppVersionPage
    {
        public Guid AppId { get; set; }
        public string VersionId { get; set; }

        public AppVersionIntentsPage(BaseProgram program) : base("Intents", program)
        { }

        public override void Display()
        {
            base.Display();

            var info = AwaitTask(Client.Model.ListIntentsWithHttpMessagesAsync(AppId, VersionId), true).Body;

            Print(info);

            WaitForGoBack();
        }
    }
}
