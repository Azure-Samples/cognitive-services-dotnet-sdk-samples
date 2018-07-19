namespace Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Sample
{
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
    using Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.IO;

    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        private static string ProgrammaticKey;
        private static string EndPoint;

        static void Main(string[] args)
        {
            ReadConfiguration();

            EndPoint = EndPoint.Insert(EndPoint.Length - 5, "/api");
            var client = new LUISAuthoringClient(new Uri(EndPoint), new ApiKeyServiceClientCredentials(ProgrammaticKey));
            var program = new BaseProgram(client, ProgrammaticKey);

            program.Run();
        }

        static void ReadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            ProgrammaticKey = Configuration["LUIS.ProgrammaticKey"];
            EndPoint = Configuration["LUIS.EndPoint"];

            if (string.IsNullOrWhiteSpace(ProgrammaticKey))
            {
                throw new ArgumentException("Missing \"LUIS.ProgrammaticKey\" in appsettings.json");
            }
        }
    }
}
