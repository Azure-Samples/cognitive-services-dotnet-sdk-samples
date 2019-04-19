namespace bing_search_dotnet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    class Program
    {
        static IDictionary<string, ExampleAbstraction> SampleMap = null;
        const string Separator = "--------------------------------------------------";

        static void Main(string[] args)
        {
            do
            {
                /*
                 * Decide which sample to showcase
                 */

                Console.WriteLine("Hi! Welcome to Entity Search Samples");

                if (SampleMap == null)
                {
                    LoadSampleOptions();
                }

                ExampleAbstraction examples;
                var input = "EntitySearch";

                if (!SampleMap.TryGetValue(input, out examples) || examples?.Examples?.Count == 0)
                {
                    Console.WriteLine("Sorry, \"{0}\" doesn't seem to be a valid sample.", input);
                    continue;
                }

                /*
                 *  Decide which example from the sample to showcase
                 */

                Console.WriteLine("Ok. Now, pick the number corresponding to an example you'd like to run: ");

                Console.WriteLine(Separator);
                for (var i = 0; i < examples.Examples.Count; i++)
                {
                    Console.WriteLine(i + ": " + examples.Examples[i].Description);
                }
                Console.WriteLine(Separator);

                input = Console.ReadLine();
                int exampleIndex;

                if (!int.TryParse(input, out exampleIndex) || exampleIndex < 0 || exampleIndex >= examples.Examples.Count)
                {
                    Console.WriteLine("Sorry, \"{0}\" is not a valid example number.", input);
                    continue;
                }

                Console.WriteLine("Ok, now please enter your subscription key:");
                input = Console.ReadLine();

                Console.WriteLine("Ok, now please enter your endpoint:");
                var inputEndpoint = Console.ReadLine();

                Console.WriteLine("Ok, running example {0} with subscription key \"{1}\"", exampleIndex, input);
                Console.WriteLine(Separator);

                examples.Examples[exampleIndex].Invoke(input, inputEndpoint);

            } while (DecideRetry());
        }

        /// <summary>
        /// Reflects through the assembly to find which samples can be explored
        /// </summary>
        private static void LoadSampleOptions()
        {
            SampleMap = new Dictionary<string, ExampleAbstraction>();

            var samples = Assembly.GetEntryAssembly().DefinedTypes.Where(type => type.GetCustomAttribute<SampleCollectionAttribute>() != null);

            foreach (var sample in samples)
            {
                var sampleImpl = new ExampleAbstraction(sample);
                sampleImpl.Load();

                SampleMap.Add(sample.GetCustomAttribute<SampleCollectionAttribute>().SampleName, sampleImpl);
            }
        }

        /// <summary>
        /// Asks the user whether they should continue and look for another sample, or quit
        /// </summary>
        /// <returns></returns>
        private static bool DecideRetry()
        {
            Console.WriteLine();
            Console.WriteLine("Would you like to look at another example (ENTER) or would you like to quit (q or CTRL+C)?");
            return Console.ReadKey().KeyChar != 'q';
        }
    }
}
