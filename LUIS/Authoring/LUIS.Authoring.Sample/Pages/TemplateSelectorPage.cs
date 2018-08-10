namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages
{
    using EasyConsole;

    class TemplateSelectorPage : BaseMenuPage
    {
        public TemplateSelectorPage(BaseProgram program) : base("Template", program,
            new Option("Greeting App", () => program.NavigateTo<CreateAppPage<GreetingApp.StartPage>>()),
            new Option("Retail App", () => program.NavigateTo<CreateAppPage<RetailApp.StartPage>>()),
            new Option("Booking App", () => program.NavigateTo<CreateAppPage<BookingApp.StartPage>>()))
        { }
    }
}
