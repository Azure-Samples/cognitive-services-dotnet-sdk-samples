namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample
{
    using System;

    interface IAppPage
    {
        Guid AppId { get; set; }
    }

    interface IAppVersionPage : IAppPage
    {
        string VersionId { get; set; }

    }
}
