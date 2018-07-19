namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.Management
{
    using EasyConsole;
    using Language.LUIS.Authoring;

    class ListAppsPage : BaseMenuPage
    {
        public ListAppsPage(BaseProgram program) : base("Apps", program)
        { }

        public override void Display()
        {
            Menu = new Menu();
            var apps = AwaitTask(Client.Apps.ListWithHttpMessagesAsync()).Body;
            foreach (var app in apps)
            {
                SafeAddToMenu(new Option($"App: {app.Name}", () => NavigateWithInitializer<AppInfoPage>(p => p.AppId = app.Id.Value)));
            }
            SafeAddToMenu(new Option("Import app", () => Program.NavigateTo<AppImportPage>()));
            SafeAddToMenu(new Option("Delete app", () => Program.NavigateTo<AppDeletePage>() ));
            base.Display();
        }
    }
}
