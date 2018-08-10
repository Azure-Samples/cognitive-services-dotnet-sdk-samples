namespace bing_search_dotnet.Samples
{
    using System;
    using System.Linq;
    using System.Text;
    using Microsoft.Azure.CognitiveServices.Search.CustomSearch;
    using Microsoft.Azure.CognitiveServices.Search.CustomSearch.Models;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    [SampleCollection("CustomSearch")]
    public class CustomSearchSamples
    {
        [Example("This will look up a single query (Xbox) and print out name and url for first web result")]
        public static void CustomSearchWebPageResultLookup(string subscriptionKey, Int32 customConfig)
        {
            var client = new CustomSearchAPI(new ApiKeyServiceClientCredentials(subscriptionKey));
            
            try
            {
                var webData = client.CustomInstance.SearchAsync(query: "Xbox", customConfig: customConfig).Result;
                Console.WriteLine("Searched for Query# \" Xbox \"");

                //WebPages
                if (webData?.WebPages?.Value?.Count > 0)
                {
                    // find the first web page
                    var firstWebPagesResult = webData.WebPages.Value.FirstOrDefault();

                    if (firstWebPagesResult != null)
                    {
                        Console.WriteLine("Webpage Results#{0}", webData.WebPages.Value.Count);
                        Console.WriteLine("First web page name: {0} ", firstWebPagesResult.Name);
                        Console.WriteLine("First web page URL: {0} ", firstWebPagesResult.Url);
                    }
                    else
                    {
                        Console.WriteLine("Couldn't find web results!");
                    }
                }
                else
                {
                    Console.WriteLine("Didn't see any Web data..");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Encountered exception. " + ex.Message);
            }
        }
    }
}