using System;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.CognitiveServices.Samples.TextAnalytics
{
    public static class Program
    {
        static void Main(string[] args)
        {
            RunSampleAsync("ENTER YOUR ENDPOINT HERE", "ENTER YOUR KEY HERE").Wait();
            Console.WriteLine("Press ENTER to exit.");
            Console.ReadLine();
        }

        public static async Task RunSampleAsync(string endpoint, string key)
        {
            // Create a client.
            ITextAnalyticsClient client = new TextAnalyticsClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Extracting language
            Console.WriteLine("===== LANGUAGE EXTRACTION ======");

            LanguageBatchResult result = await client.DetectLanguageAsync(
                    new BatchInput(
                        new List<Input>()
                        {
                          new Input("1", "This is a document written in English."),
                          new Input("2", "Este es un document escrito en Español."),
                          new Input("3", "这是一个用中文写的文件")
                        }));

            // Printing language results.
            foreach (var document in result.Documents)
            {
                Console.WriteLine("Document ID: {0} , Language: {1}", document.Id, document.DetectedLanguages[0].Name);
            }

            // Getting key-phrases
            Console.WriteLine("\n\n===== KEY-PHRASE EXTRACTION ======");

            KeyPhraseBatchResult result2 = await client.KeyPhrasesAsync(
                    new MultiLanguageBatchInput(
                        new List<MultiLanguageInput>()
                        {
                          new MultiLanguageInput("ja", "1", "猫は幸せ"),
                          new MultiLanguageInput("de", "2", "Fahrt nach Stuttgart und dann zum Hotel zu Fu."),
                          new MultiLanguageInput("en", "3", "My cat is stiff as a rock."),
                          new MultiLanguageInput("es", "4", "A mi me encanta el fútbol!")
                        }));


            // Printing keyphrases
            foreach (var document in result2.Documents)
            {
                Console.WriteLine("Document ID: {0} ", document.Id);

                Console.WriteLine("\t Key phrases:");

                foreach (string keyphrase in document.KeyPhrases)
                {
                    Console.WriteLine("\t\t" + keyphrase);
                }
            }

            // Extracting sentiment
            Console.WriteLine("\n\n===== SENTIMENT ANALYSIS ======");

            SentimentBatchResult result3 = await client.SentimentAsync(
                    new MultiLanguageBatchInput(
                        new List<MultiLanguageInput>()
                        {
                          new MultiLanguageInput("en", "0", "I had the best day of my life."),
                          new MultiLanguageInput("en", "1", "This was a waste of my time. The speaker put me to sleep."),
                          new MultiLanguageInput("es", "2", "No tengo dinero ni nada que dar..."),
                          new MultiLanguageInput("it", "3", "L'hotel veneziano era meraviglioso. È un bellissimo pezzo di architettura."),
                        }));


            // Printing sentiment results
            foreach (var document in result3.Documents)
            {
                Console.WriteLine("Document ID: {0} , Sentiment Score: {1:0.00}", document.Id, document.Score);
            }

            // Extracting entities
            Console.WriteLine("\n\n===== Entity Extraction ======");

            EntitiesBatchResultV2dot1 result4 = await client.EntitiesAsync(
                    new MultiLanguageBatchInput(
                        new List<MultiLanguageInput>()
                        {
                          new MultiLanguageInput("en", "0", "Microsoft released win10. Microsoft also released Hololens"),
                          new MultiLanguageInput("en", "1", "Microsoft is an IT company."),
                          new MultiLanguageInput("es", "2", "Microsoft lanzó win10. Microsoft también lanzó Hololens"),
                          new MultiLanguageInput("es", "3", "Microsoft es una empresa de TI."),
                        }));


            // Printing entity extraction results
            foreach (var document in result4.Documents)
            {
                Console.WriteLine("Document ID: {0} ", document.Id);

                Console.WriteLine("\t Entities:");

                foreach (EntityRecordV2dot1 entity in document.Entities)
                {
                    Console.WriteLine("\t\tEntity Name: {0}", entity.Name);
                    Console.WriteLine("\t\tWikipedia Language: {0}", entity.WikipediaLanguage);
                    Console.WriteLine("\t\tWikipedia Url: {0}", entity.WikipediaUrl);
                    Console.WriteLine("\t\tNumber of times appeared on the text: {0}", entity.Matches.Count);
                    Console.WriteLine("\t\tEntity Type: {0}", entity.Type);
                    Console.WriteLine("\t\tEntity SubType: {0}", entity.SubType);
                    Console.WriteLine("\n");
                }
            }
        }
    }
}
