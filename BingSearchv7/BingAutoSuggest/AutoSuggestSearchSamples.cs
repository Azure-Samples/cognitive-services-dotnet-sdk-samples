namespace bing_search_dotnet.Samples
{
    using System;
    using Microsoft.Azure.CognitiveServices.Search.AutoSuggest;
    using Microsoft.Azure.CognitiveServices.Search.AutoSuggest.Models;

    [SampleCollection("AutoSuggestSearch")]
    public class AutoSuggestSearchSamples
    {
        [Example("This will request suggestions (Satya Nadella) and print out content about them")]
        public static void AutoSuggestLookup(string subscriptionKey)
        {
            var client = new AutoSuggestSearchAPI(new ApiKeyServiceClientCredentials(subscriptionKey));

            try
            {
                var suggestions = client.AutoSuggestMethod(query: "Satya Nadella");

                if (suggestions != null && suggestions.SuggestionGroups.Count > 0)
                {
                    // dump content
                    Console.WriteLine("Searched for \"Satya Nadella\" and found suggestions:");
                    foreach (var suggestion in suggestions.SuggestionGroups[0].SearchSuggestions)
                    {
                        Console.WriteLine("....................................");
                        Console.WriteLine(suggestion.Query);
                        Console.WriteLine(suggestion.DisplayText);
                        Console.WriteLine(suggestion.Url);
                        Console.WriteLine(suggestion.SearchKind);
                        
                    }
                }
                else
                {
                    Console.WriteLine("Didn't see any suggestion..");
                }
            }
            catch (ErrorResponseException ex)
            {
                Console.WriteLine("Encountered exception. " + ex.Message);
            }
        }

        [Example("This triggers a bad request and shows how to read the error response")]
        public static void Error(string subscriptionKey)
        {
            var client = new AutoSuggestSearchAPI(new ApiKeyServiceClientCredentials(subscriptionKey + "1"));

            try
            {
                var suggestions = client.AutoSuggestMethod(query: "Satya Nadella", market: "no-ty");
            }
            catch (ErrorResponseException ex)
            {
                // The status code of the error should be a good indication of what occurred. However, if you'd like more details, you can dig into the response.
                // Please note that depending on the type of error, the response schema might be different, so you aren't guaranteed a specific error response schema.
                Console.WriteLine("Exception occurred, status code {0} with reason {1}.", ex.Response.StatusCode, ex.Response.ReasonPhrase);
            }
        }
    }
}
