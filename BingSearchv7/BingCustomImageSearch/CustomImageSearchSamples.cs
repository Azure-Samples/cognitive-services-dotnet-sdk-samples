namespace bing_search_dotnet.Samples
{
    using System;
    using System.Linq;
    using Microsoft.Azure.CognitiveServices.Search.CustomImageSearch;

    [SampleCollection("CustomImageSearch")]
    public class CustomImageSearchSamples
    {
        [Example("This will look up a single query (Xbox) and print out number of results, insights token, thumbnail url, content url for first image result")]
        public static void CustomImageSearchResultLookup(string subscriptionKey, long customConfig)
        {
            var client = new CustomImageSearchAPI(new ApiKeyServiceClientCredentials(subscriptionKey));
            
            try
            {
                var imageResults = client.CustomInstance.ImageSearchAsync(query: "Xbox", customConfig: customConfig).Result;
                
                Console.WriteLine("Searched for Query# \" Xbox \"");

                //WebPages
                if (imageResults?.Value?.Count > 0)
                {
                    // find the first web page
                    var firstImageResult = imageResults.Value.First();

                    if (firstImageResult != null)
                    {
                        Console.WriteLine($"Image result count: {imageResults.Value.Count}");
                        Console.WriteLine($"First image insights token: {firstImageResult.ImageInsightsToken}");
                        Console.WriteLine($"First image thumbnail url: {firstImageResult.ThumbnailUrl}");
                        Console.WriteLine($"First image content url: {firstImageResult.ContentUrl}");
                    }
                    else
                    {
                        Console.WriteLine("Couldn't find image results!");
                    }
                }
                else
                {
                    Console.WriteLine("Couldn't find image results!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Encountered exception. " + ex.Message);
            }
        }
    }
}