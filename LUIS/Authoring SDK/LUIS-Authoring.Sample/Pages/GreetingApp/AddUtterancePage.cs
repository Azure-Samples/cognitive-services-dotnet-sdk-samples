namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.GreetingApp
{
    using System;
    using EasyConsole;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring
        .Models;

    class AddUtterancePage : BasePage, IAppVersionPage
    {
        public Guid AppId { get; set; }
        public string VersionId { get; set; }
        public string IntentName { get; set; }

        public AddUtterancePage(BaseProgram program) : base("Add utterances", program)
        { }

        public override void Display()
        {
            base.Display();

            var newUtterance = new ExampleLabelObject
            {
                IntentName = IntentName
            };

            var addNew = Input.ReadString("Do you want to add more utterances? (y/n) ");

            while (addNew.Trim().ToLowerInvariant().StartsWith("y"))
            {
                newUtterance.Text = Input.ReadString("Type new utterance: ");

                var result = AwaitTask(Client.Examples.AddAsync(AppId, VersionId, newUtterance));

                Console.WriteLine($"Utterance \"{newUtterance.Text}\" added to the intent {IntentName}\n");

                addNew = Input.ReadString("Do you want to add additional utterances? (y/n) ");
            }

            NavigateWithInitializer<TrainAppPage>((page) => {
                page.AppId = AppId;
                page.VersionId = VersionId;
            });
        }
    }
}
