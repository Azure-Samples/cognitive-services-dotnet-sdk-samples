namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample.Pages
{
    using EasyConsole;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
    using System;

    class ShareAppPage : BasePage, IAppVersionPage
    {
        public Guid AppId { get; set; }
        public string VersionId { get; set; }

        public ShareAppPage(BaseProgram program) : base("Share", program)
        { }

        public override void Display()
        {
            base.Display();

            var response = Input.ReadString("Do you want to add collaborators to your App? (y/n) ");

            while (response.Trim().ToLowerInvariant().StartsWith("y"))
            {
                var userEmail = Input.ReadString("Enter collaborator email: ");

                var newCollaborator = new UserCollaborator
                {
                    Email = userEmail
                };

                try
                {
                    AwaitTask(Client.Permissions.AddAsync(AppId, newCollaborator));
                    Console.WriteLine("New collaborator added!");
                    response = Input.ReadString("Do you want to add another collaborator? (y/n) ");
                }
                catch (Exception)
                {
                    Console.WriteLine("Something went wrong. Please make sure you provide a valid email address.");
                    response = Input.ReadString("Do you still want to add collaborators to your App? (y/n) ");
                }

            }

            WaitForNavigateHome();
        }
    }
}
