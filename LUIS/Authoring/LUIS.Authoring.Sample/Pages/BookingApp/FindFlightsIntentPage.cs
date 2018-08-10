namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.BookingApp
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;

    class FindFlightsIntentPage : BasePage, IAppVersionPage
    {
        public Guid AppId { get; set; }
        public string VersionId { get; set; }

        const string findEconomyToMadrid = "find flights in economy to Madrid";
        const string findFirstToLondon = "find flights to London in first class";

        public FindFlightsIntentPage(BaseProgram program) : base("FindFlights Intent", program)
        { }

        public override void Display()
        {
            base.Display();

            Console.WriteLine("We’ll create a new \"FindFlights\" intent including the following utterances:");
            Console.WriteLine($" - {findEconomyToMadrid}");
            Console.WriteLine($" - {findFirstToLondon}");

            var findFlightsIntent = new ModelCreateObject
            {
                Name = "FindFlights"
            };

            var intentId = AwaitTask(Client.Model.AddIntentAsync(this.AppId, this.VersionId, findFlightsIntent));

            Console.WriteLine($"{findFlightsIntent.Name} intent created with the id {intentId}");

            var findEconomyToMadridUtterance = new ExampleLabelObject
            {
                Text = findEconomyToMadrid,
                EntityLabels = new[]
                {
                    GetExampleLabel(findEconomyToMadrid, "Flight", "economy to Madrid"),
                    GetExampleLabel(findEconomyToMadrid, "Destination", "Madrid"),
                    GetExampleLabel(findEconomyToMadrid, "Class", "economy")
                },
                IntentName = findFlightsIntent.Name
            };
            var findFirstToLondonUtterance = new ExampleLabelObject
            {
                Text = findFirstToLondon,
                EntityLabels = new[]
                {
                    GetExampleLabel(findFirstToLondon, "Flight", "London in first class"),
                    GetExampleLabel(findFirstToLondon, "Destination", "London"),
                    GetExampleLabel(findFirstToLondon, "Class", "first")
                },
                IntentName = findFlightsIntent.Name
            };

            var utterances = new List<ExampleLabelObject> { findEconomyToMadridUtterance, findFirstToLondonUtterance };

            var utterancesResult = AwaitTask(Client.Examples.BatchAsync(this.AppId, this.VersionId, utterances));

            Console.WriteLine($"Utterances added to the {findFlightsIntent.Name} intent");

            NavigateWithInitializer<TrainAppPage>((page) =>
            {
                page.AppId = AppId;
                page.VersionId = VersionId;
            });
        }

        private static EntityLabelObject GetExampleLabel(string utterance, string label, string value = null)
        {
            value = value ?? label;
            return new EntityLabelObject
            {
                EntityName = label,
                StartCharIndex = utterance.IndexOf(value, StringComparison.InvariantCultureIgnoreCase),
                EndCharIndex = utterance.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) + value.Length
            };
        }
    }
}