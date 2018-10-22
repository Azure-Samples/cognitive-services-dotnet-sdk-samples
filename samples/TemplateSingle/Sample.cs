
/// <summary>
/// Namespace should be Microsoft.Azure.CognitiveServices.Samples.{ServiceName}
/// </summary>
namespace Microsoft.Azure.CognitiveServices.Samples.TemplateSingle
{
    using System;
    using System.Threading.Tasks;


    /// <summary>
    /// Sample class.
    /// 
    /// Sample class must be public static, with any name, without parent class.
    /// </summary>
    public static class Sample
    {

        /// <summary>
        /// Sample method.
        /// 
        /// Sample method must be public static void or public static async Task, with any name.
        /// Common parameters: endpoint, key.
        /// </summary>
        /// <param name="endpoint">The endpoint of service.</param>
        /// <param name="key">The key of service.</param>
        public static void RunSample(string endpoint, string key)
        {
            // Some servces have separated key for managing/trainning, for those cases please contact admin about the parameter name.
            Console.WriteLine("RunSample");
            Console.WriteLine("Endpoint:" + endpoint);

            // Resources URL must be hardcoded.
            // Account name etc, must be hardcoded or be random generalized.

            // There should be no user interaction.
            // In case of any error, throw expections. Recomend to use below pattern to give friendly messages before exception.
            try { }
            catch (Exception e)
            {
                Console.Error.WriteLine("Hints");
                throw e;
            }

            // Safe to use Console.WriteLine()
            Console.WriteLine("Hello World!");
        }

        /// <summary>
        /// Sample async method.
        /// </summary>
        /// <param name="endpoint">The endpoint of service.</param>
        /// <param name="key">The key of service.</param>
        public static async Task RunSampleAsync(string endpoint, string key)
        {
            Console.WriteLine("RunSampleAsync");
            Console.WriteLine("Endpoint:" + endpoint);

            await Console.Out.WriteLineAsync("Hello async World!");
        }
    }
}
