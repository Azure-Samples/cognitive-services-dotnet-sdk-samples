namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.RetailApp
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;

    class SendFlowersIntentPage : BasePage, IAppVersionPage
    {
        public Guid AppId { get; set; }
        public string VersionId { get; set; }

        const string sendBouquetOfRoses = "send a bouquet of roses to Mary";
        const string sendCactusFlowerpot = "send a cactus flowerpot to Jim";

        public SendFlowersIntentPage(BaseProgram program) : base("SendFlowers Intent", program)
        { }

        public override void Display()
        {
            base.Display();

            Console.WriteLine("We’ll create a new \"SendFlowers\" intent including the following utterances:");
            Console.WriteLine($" - {sendBouquetOfRoses}");
            Console.WriteLine($" - {sendCactusFlowerpot}");

            var sendFlowersIntent = new ModelCreateObject
            {
                Name = "SendFlowers"
            };

            var intentId = AwaitTask(Client.Model.AddIntentAsync(this.AppId, this.VersionId, sendFlowersIntent));

            Console.WriteLine($"{sendFlowersIntent.Name} intent created with the id {intentId}");

            var bouquetOfRosesUtterance = new ExampleLabelObject
            {
                Text = sendBouquetOfRoses,
                EntityLabels = new[] { GetExampleLabel(sendBouquetOfRoses, "Bouquet"), GetExampleLabel(sendBouquetOfRoses, "Bouquet::Roses", "roses") },
                IntentName = sendFlowersIntent.Name
            };
            var cactusFlowerpotUtterance = new ExampleLabelObject
            {
                Text = sendCactusFlowerpot,
                EntityLabels = new[] { GetExampleLabel(sendCactusFlowerpot, "Flowerpot"), GetExampleLabel(sendCactusFlowerpot, "Flowerpot::Cactus", "cactus") },
                IntentName = sendFlowersIntent.Name
            };

            var utterances = new List<ExampleLabelObject> { bouquetOfRosesUtterance, cactusFlowerpotUtterance };

            var utterancesResult = AwaitTask(Client.Examples.BatchAsync(this.AppId, this.VersionId, utterances));

            Console.WriteLine($"Utterances added to the {sendFlowersIntent.Name} intent");


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