namespace Microsoft.Azure.CognitiveServices.Samples.SpellCheck
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.Language.SpellCheck;
    using Microsoft.Azure.CognitiveServices.Language.SpellCheck.Models;

    public static class SpellCheckSample
    {
        public static async Task SpellCheckCorrection(string key)
        {
            var client = new SpellCheckClient(new ApiKeyServiceClientCredentials(key));

            var result = await client.SpellCheckerWithHttpMessagesAsync(text: "Bill Gatas", mode: "proof");
            Console.WriteLine("Correction for Query# \"bill gatas\"");

            // SpellCheck Results
            if (result?.Body.FlaggedTokens?.Count == 0)
            {
                throw new Exception("Didn't see any SpellCheck results..");
            }

            // find the first spellcheck result
            var firstspellCheckResult = result.Body.FlaggedTokens.FirstOrDefault();

            if (firstspellCheckResult == null)
            {
                throw new Exception("Couldn't get any Spell check results!");
            }

            Console.WriteLine("SpellCheck Results#{0}", result.Body.FlaggedTokens.Count);
            Console.WriteLine("First SpellCheck Result token: {0} ", firstspellCheckResult.Token);
            Console.WriteLine("First SpellCheck Result Type: {0} ", firstspellCheckResult.Type);
            Console.WriteLine("First SpellCheck Result Suggestion Count: {0} ", firstspellCheckResult.Suggestions.Count);

            var suggestions = firstspellCheckResult.Suggestions;
            if (suggestions?.Count > 0)
            {
                var firstSuggestion = suggestions.FirstOrDefault();
                Console.WriteLine("First SpellCheck Suggestion Score: {0} ", firstSuggestion.Score);
                Console.WriteLine("First SpellCheck Suggestion : {0} ", firstSuggestion.Suggestion);
            }
        }

        public static async Task SpellCheckError(string key)
        {
            var client = new SpellCheckClient(new ApiKeyServiceClientCredentials(key));

            try
            {
                var result = await client.SpellCheckerAsync("", mode: "proof");
                throw new Exception("Client didn't throw correct exception.");
            }
            catch (ErrorResponseException ex)
            {
                Console.WriteLine("ErrorResponse : {0} ", ex.Message);
            }
        }

    }
}
