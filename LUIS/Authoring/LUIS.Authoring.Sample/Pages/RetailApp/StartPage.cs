namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages.RetailApp
{
    using System;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;

    class StartPage : BasePage, IAppVersionPage
    {
        public Guid AppId { get; set; }
        public string VersionId { get; set; }

        public StartPage(BaseProgram program) : base("Retail App", program)
        { }

        public override void Display()
        {
            base.Display();

            Console.WriteLine("We’ll create a new \"Bouquet\" hierarchical entity.");
            Console.WriteLine("The Bouquet entity will contain \"Roses\" and \"Carnations\" as child entities.");

            var bouquetEntity = new HierarchicalEntityModel
            {
                Name = "Bouquet",
                Children = new[] { "Roses", "Carnations" }
            };

            var entityId = AwaitTask(Client.Model.AddHierarchicalEntityAsync(AppId, VersionId, bouquetEntity));

            Console.WriteLine($"{bouquetEntity.Name} hierarchical Entity created with the id {entityId}");

            BaseProgram.sampleUtterance = "Send a bouquet of roses to Mary";

            NavigateWithInitializer<FlowerpotPage>((page) =>
            {
                page.AppId = AppId;
                page.VersionId = VersionId;
            });
        }
    }
}