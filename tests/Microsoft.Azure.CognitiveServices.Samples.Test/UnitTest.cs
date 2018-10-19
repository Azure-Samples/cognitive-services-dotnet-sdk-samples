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
            var sampleDlls = Directory.GetFiles(samplePath, "Microsoft.Azure.CognitiveServices.Samples.*.dll", SearchOption.AllDirectories);
            Regex regex = new Regex(@".+[\\/]bin[\\/].*netstandard1\.4.*\\Microsoft\.Azure\.CognitiveServices\.Samples\.(.+)\.dll");

            foreach (var sampleDll in sampleDlls)
            {
                var match = regex.Match(sampleDll);
                if (!match.Success)
                {
                    continue;
                }

                var service = match.Groups[1];
                Console.WriteLine(service);
                Console.WriteLine(sampleDll);

                var DLL = Assembly.LoadFile(sampleDll);

                foreach (Type type in DLL.GetExportedTypes())
                {
                    // var c = Activator.CreateInstance(type);

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
        }
    }
}
