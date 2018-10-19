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

        [ClassInitialize]
        public static void Initialize(TestContext testContext)
        {
            TestContext = testContext;
        }

        [TestMethod]
        public async Task TestMethod()
        {
            string samplePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\..\\..\\samples\\"));
            HashSet<string> apis = new HashSet<string>();

            foreach (var path in Directory.GetDirectories(samplePath))
            {
                apis.Add(Path.GetFileName(path));
            }
            
            var sampleDlls = Directory.GetFiles(samplePath, "Microsoft.Azure.CognitiveServices.Samples.*.dll", SearchOption.AllDirectories);
            Regex regex = new Regex(@".+[\\/]bin[\\/].*netstandard1\.4.*\\publish\\Microsoft\.Azure\.CognitiveServices\.Samples\.(.+)\.dll");

            HashSet<string> tested = new HashSet<string>();
            foreach (var sampleDll in sampleDlls)
            {
                var match = regex.Match(sampleDll);
                if (!match.Success)
                {
                    continue;
                }

                var service = match.Groups[1].Value;
                tested.Add(service);
                Console.WriteLine(service);
                Console.WriteLine(sampleDll);
                var DLL = Assembly.LoadFrom(sampleDll);

                foreach (Type type in DLL.GetExportedTypes())
                {
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(sampleDll));
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

                        if (method.ReturnType == typeof(Task))
                        {
                            await (Task)method.Invoke(null, paras.ToArray());
                        }
                        else
                        {
                            method.Invoke(null, paras.ToArray());
                        }

                        Console.WriteLine(method.Name);
                        Console.WriteLine();
                    }
                }
            }


            foreach (var api in apis)
            {
                if (!tested.Contains(api))
                {
                    throw new Exception($"Testes for {api} is missing.");
                }
            }
        }
    }
}
