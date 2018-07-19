namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages
{
    using EasyConsole;
    using System;

    class MainPage : BaseMenuPage
    {
        public MainPage(BaseProgram program) : base("Main", program,
            new Option("Start new LUIS app wizard", () => program.NavigateTo<TemplateSelectorPage>()),
            new Option("Manage Apps", () => program.NavigateTo<Management.ListAppsPage>()),
            new Option("Exit", () => Environment.Exit(0)))
        { }
    }
}
