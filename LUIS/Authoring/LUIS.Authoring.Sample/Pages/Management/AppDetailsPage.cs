namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.Management
{
    using Language.LUIS.Authoring;
    using System;

    class AppDetailsPage : BasePage, IAppPage
    {
        public Guid AppId { get; set; }

        public AppDetailsPage(BaseProgram program) : base("Details", program)
        { }

        public override void Display()
        {
            base.Display();

            var info = AwaitTask(Client.Apps.GetWithHttpMessagesAsync(AppId), true).Body;

            Print(info);

            WaitForGoBack();
        }
    }
}
