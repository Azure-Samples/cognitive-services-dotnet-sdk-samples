namespace Microsoft.Azure.CognitiveServices.Samples.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    [TestClass]
    public class UnitTest
    {
        static TestContext TestContext;

        private static string SamplePrefix = "Microsoft.Azure.CognitiveServices.Samples.";
        private static string SamplePrefixRegexEscaped = SamplePrefix.Replace(".", @"\.");

        [ClassInitialize]
        public static void Initialize(TestContext testContext)
        {
            TestContext = testContext;
        }

        /// <summary>
        /// Returns an array of sample project name.
        /// </summary>
        /// <param name="samplePath">The path contains samples</param>
        /// <returns>An array of sample project name</returns>
        private static string[] GetSampleProjects(string samplePath)
        {
            var sampleProjs = Directory.GetFiles(samplePath, "*.csproj", SearchOption.AllDirectories);
            Regex regex = new Regex($@".*\\{SamplePrefixRegexEscaped}([^\\]+)\.csproj");

            List<string> results = new List<string>();

            foreach (var sampleProj in sampleProjs)
            {
                var match = regex.Match(sampleProj);
                if (!match.Success)
                {
                    throw new Exception($"Found a project not in standard name: {sampleProj}");
                }

                results.Add(match.Groups[1].Value);
            }

            return results.ToArray();
        }

        /// <summary>
        /// Returns an array of service name with sample(s).
        /// </summary>
        /// <param name="samplePath">The path contains samples</param>
        /// <returns>An array of service name</returns>
        private static string[] GetSampleServices(string samplePath)
        {
            List<string> results = new List<string>();

            foreach (var path in Directory.GetDirectories(samplePath))
            {
                results.Add(Path.GetFileName(path));
            }

            return results.ToArray();
        }



        /// <summary>
        /// Run the samples in a file.
        /// </summary>
        /// <param name="service">The service name</param>
        /// <param name="project">The sample project name</param>
        /// <param name="path">The path contains the samples</param>
        /// <returns>The async task</returns>
        private static async Task RunSampleAsync(string service, string project, string path)
        {
            Console.WriteLine($"Service: {service}, Project: {project}, File: {path}");

            var DLL = Assembly.LoadFrom(path);

            foreach (Type type in DLL.GetExportedTypes())
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(path));
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
                {
                    var paras = new List<string>();
                    foreach (var para in method.GetParameters())
                    {
                        string paraName = $"{service}." + para.Name;

                        if (!TestContext.Properties.ContainsKey(paraName))
                        {
                            throw new Exception($"The configuration '{paraName}' is missing.");
                        }

                        paras.Add(TestContext.Properties[paraName].ToString());
                    }

                    Console.WriteLine($"{project}: {method.Name}");

                    if (method.ReturnType == typeof(Task))
                    {
                        await(Task)method.Invoke(null, paras.ToArray());
                    }
                    else
                    {
                        method.Invoke(null, paras.ToArray());
                    }

                    // Delay one second to control TPS
                    await Task.Delay(1000);
                }
            }

            Console.WriteLine();
        }

        [TestMethod]
        public async Task TestMethod()
        {
            string samplePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\..\\..\\samples\\"));
            HashSet<string> projects = new HashSet<string>(GetSampleProjects(samplePath));
            HashSet<string> services = new HashSet<string>(GetSampleServices(samplePath));
            
            var sampleDlls = Directory.GetFiles(samplePath, $"{SamplePrefix}*.dll", SearchOption.AllDirectories);
            Regex regex = new Regex($@".+[\\/]bin[\\/].*netstandard1\.4.*\\publish\\{SamplePrefixRegexEscaped}(([^\.\\]+)(?:\.[^\.\\]+)?)\.dll");

            HashSet<string> projectsTested = new HashSet<string>();
            HashSet<string> servicesTested = new HashSet<string>();

            foreach (var sampleDll in sampleDlls)
            {
                var match = regex.Match(sampleDll);
                if (!match.Success)
                {
                    continue;
                }

                var project = match.Groups[1].Value;
                var service = match.Groups[2].Value;
                projectsTested.Add(project);
                servicesTested.Add(service);

                await RunSampleAsync(service, project, sampleDll);
            }

            foreach (var service in services)
            {
                if (!servicesTested.Contains(service))
                {
                    throw new Exception($"Testes for {service} is missing.");
                }
            }

            foreach (var project in projects)
            {
                if (!projectsTested.Contains(project))
                {
                    throw new Exception($"Project {project} contains no tests.");
                }
            }
        }
    }
}
