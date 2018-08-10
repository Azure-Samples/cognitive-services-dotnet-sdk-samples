namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.Management
{
    using EasyConsole;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
    using System;

    class AppDeletePage : BaseMenuPage
    {
        public AppDeletePage(BaseProgram program) : base("Delete", program)
        { }

        public override void Display()
        {
            Menu = new Menu();
            var apps = AwaitTask(Client.Apps.ListWithHttpMessagesAsync()).Body;
            foreach (var app in apps)
            {
                SafeAddToMenu(new Option(app.Name, () => DeleteApp(app.Id.Value)));
            }

            base.Display();
        }

        private void DeleteApp(Guid appId)
        {
            AwaitTask(Client.Apps.DeleteWithHttpMessagesAsync(appId));
            Console.WriteLine("App deleted!\n");
            WaitForGoBack();
        }
    }
}
