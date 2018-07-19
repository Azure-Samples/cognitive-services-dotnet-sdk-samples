namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.BookingApp
{
    using System;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;

    class FlightsEntityPage : BasePage, IAppVersionPage
    {
        public Guid AppId { get; set; }
        public string VersionId { get; set; }

        public FlightsEntityPage(BaseProgram program) : base("Flights Composite Entity", program)
        { }

        public override void Display()
        {
            base.Display();

            Console.WriteLine("We’ll now create the  \"Flight\" composite entity including \"Class\" and \"Destination\".");

            var compositeEntity = new CompositeEntityModel
            {
                Name = "Flight",
                Children = new[] { "Class", "Destination" }
            };

            var compositeEntityId = AwaitTask(Client.Model.AddCompositeEntityAsync(AppId, VersionId, compositeEntity));

            Console.WriteLine($"{compositeEntity.Name} composite entity created with id {compositeEntityId}");

            NavigateWithInitializer<FindFlightsIntentPage>((page) => {
                page.AppId = AppId;
                page.VersionId = VersionId;
            });
        }
    }
}