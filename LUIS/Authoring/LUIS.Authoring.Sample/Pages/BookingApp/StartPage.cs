namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.BookingApp
{
    using System;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;

    class StartPage : BasePage, IAppVersionPage
    {
        public Guid AppId { get; set; }
        public string VersionId { get; set; }

        public StartPage(BaseProgram program) : base("Booking App", program)
        { }

        public override void Display()
        {
            base.Display();

            Console.WriteLine("We’ll create two new entities.");
            Console.WriteLine("The \"Destination\" simple entity will hold the flight destination.");
            Console.WriteLine("The \"Class\" hierarchical entity will accept \"First\", \"Business\" and \"Economy\" values.");

            var simpleEntity = new ModelCreateObject
            {
                Name = "Destination"
            };

            var simpleEntityId = AwaitTask(Client.Model.AddEntityAsync(AppId, VersionId, simpleEntity));

            Console.WriteLine($"{simpleEntity.Name} simple entity created with id {simpleEntityId}");

            var hierarchicalEntity = new HierarchicalEntityModel
            {
                Name = "Class",
                Children = new[] { "First", "Business", "Economy" }
            };

            var hierarchicalEntityId = AwaitTask(Client.Model.AddHierarchicalEntityAsync(AppId, VersionId, hierarchicalEntity));

            Console.WriteLine($"{hierarchicalEntity.Name} hierarchical entity created with id {hierarchicalEntityId}");

            BaseProgram.sampleUtterance = "Find flights to London in first class";

            NavigateWithInitializer<FlightsEntityPage>((page) =>
            {
                page.AppId = AppId;
                page.VersionId = VersionId;
            });
        }
    }
}
