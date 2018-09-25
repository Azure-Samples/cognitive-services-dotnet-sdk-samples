namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample
{
    using EasyConsole;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
    using Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages;
    using BookingApp = Pages.BookingApp;
    using GreetingApp = Pages.GreetingApp;
    using Management = Pages.Management;
    using RetailApp = Pages.RetailApp;

    public class BaseProgram : Program
    {
        public LUISAuthoringClient Client { get; private set; }
        public string AuthoringKey { get; private set; }

        public static string sampleUtterance { get; set; }

        public BaseProgram(LUISAuthoringClient client, string authoringKey) : base("LUIS Authoring API Demo", true)
        {
            Client = client;
            AuthoringKey = authoringKey;
            AddPage(new MainPage(this));

            AddPage(new Management.ListAppsPage(this));
            AddPage(new Management.AppInfoPage(this));
            AddPage(new Management.AppDetailsPage(this));
            AddPage(new Management.AppVersionIntentsPage(this));
            AddPage(new Management.AppVersionEntitiesPage(this));
            AddPage(new Management.AppVersionPrebuiltEntitiesPage(this));
            AddPage(new Management.AppTrainPage(this));
            AddPage(new Management.AppDeletePage(this));
            AddPage(new Management.AppClonePage(this));
            AddPage(new Management.AppImportPage(this));
            AddPage(new Management.AppExportPage(this));
            AddPage(new Management.AppPublishPage(this));
            AddPage(new Management.AppVersionSelector(this));
            AddPage(new Management.AppVersionInfoPage(this));
            AddPage(new Management.AppVersionDetailsPage(this));

            AddPage(new CreateAppPage<GreetingApp.StartPage>(this));
            AddPage(new GreetingApp.StartPage(this));
            AddPage(new GreetingApp.AddUtterancePage(this));

            AddPage(new CreateAppPage<RetailApp.StartPage>(this));
            AddPage(new RetailApp.StartPage(this));
            AddPage(new RetailApp.FlowerpotPage(this));
            AddPage(new RetailApp.AddFlowersPage(this));
            AddPage(new RetailApp.SendFlowersIntentPage(this));

            AddPage(new CreateAppPage<BookingApp.StartPage>(this));
            AddPage(new BookingApp.StartPage(this));
            AddPage(new BookingApp.FlightsEntityPage(this));
            AddPage(new BookingApp.FindFlightsIntentPage(this));

            AddPage(new TemplateSelectorPage(this));
            AddPage(new TrainAppPage(this));
            AddPage(new PublishAppPage(this));
            AddPage(new ShareAppPage(this));

            SetPage<MainPage>();
        }
    }
}