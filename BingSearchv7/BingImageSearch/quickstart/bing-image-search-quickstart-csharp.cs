using System;
using System.Linq;
using Microsoft.Azure.CognitiveServices.Search.ImageSearch;
using Microsoft.Azure.CognitiveServices.Search.ImageSearch.Models;

namespace bing_search_dotnet
{
    public class BingImageSearchSample
    {
 
        static void Main(string[] args)
        {
            //IMPORTANT: replace this variable with your Cognitive Services subscription key.
            string subscriptionKey = "ENTER YOUR KEY HERE";
            // the image search term used in the query
            string searchTerm = "canadian rockies";
            //initialize the client
            var client = new ImageSearchAPI(new ApiKeyServiceClientCredentials(subscriptionKey));

            Console.WriteLine("This application will send an HTTP request to the Bing Image Search API for {0} and print the response.", searchTerm);

            //images to be returned by the Bing Image Search API
            Images imageResults = null;

            //try to send the request, and get the results. 
            Console.WriteLine("Search results for the image query: {0}", searchTerm);
            try
            {
                imageResults = client.Images.SearchAsync(query: searchTerm).Result; //search query
            }
            catch (Exception ex)
            {
                Console.WriteLine("Encountered exception. " + ex.Message);
            }

            if (imageResults != null)
            {
                //display the details for the first image result. After running the application,
                //you can copy the resulting URLs from the console into your browser to view the image. 
                var firstImageResult = imageResults.Value.First();
                Console.WriteLine($"\nTotal number of returned images: {imageResults.Value.Count}\n");
                Console.WriteLine($"Copy the following URLs to view these images on your browser.\n");
                Console.WriteLine($"URL to the first image:\n\n {firstImageResult.ContentUrl}\n");
                Console.WriteLine($"Thumbnail URL for the first image:\n\n {firstImageResult.ThumbnailUrl}");
                Console.Write("\nPress Enter to exit ");
                Console.ReadKey();
            }
        }


    }
}
