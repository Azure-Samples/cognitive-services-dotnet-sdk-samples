namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.GreetingApp
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;

    class StartPage : BasePage, IAppVersionPage
    {
        public Guid AppId { get; set; }
        public string VersionId { get; set; }

        public StartPage(BaseProgram program) : base("Greeting App", program)
        { }

        public override void Display()
        {
            base.Display();

            Console.WriteLine();
            Console.WriteLine("We'll create a new \"intent\" including the following utterances:");
            Console.WriteLine(" - Hi");
            Console.WriteLine(" - Hello");

            var greetingIntent = new ModelCreateObject
            {
                Name = "Greeting"
            };

            var intentId = AwaitTask(Client.Model.AddIntentAsync(AppId, VersionId, greetingIntent));

            Console.WriteLine($"{greetingIntent.Name} intent created with the id {intentId}");

            var utterances = new List<ExampleLabelObject>
            {
                new ExampleLabelObject("Hi", null, greetingIntent.Name),
                new ExampleLabelObject("Hello", null, greetingIntent.Name)
            };

            var utterancesResult = AwaitTask(Client.Examples.BatchAsync(AppId, VersionId, utterances));

            Console.WriteLine("Utterances added to the intent");

            BaseProgram.sampleUtterance = "Hello";

            NavigateWithInitializer<AddUtterancePage>((page) => {
                page.AppId = AppId;
                page.VersionId = VersionId;
                page.IntentName = greetingIntent.Name;
            });
        }
    }
}
