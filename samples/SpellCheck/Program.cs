namespace Microsoft.Azure.CognitiveServices.Samples.SpellCheck
{
    class Program
    {
        static void Main(string[] args)
        {
            SpellCheckSample.SpellCheckCorrection("ENTER YOUR KEY HERE").Wait();
            SpellCheckSample.SpellCheckError("ENTER YOUR KEY HERE").Wait();
        }
    }
}
