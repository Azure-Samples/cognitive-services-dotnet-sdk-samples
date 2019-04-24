using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AnalyzeImage
{
    static class AnalyzeImage
    {
        // Replace <Subscription Key> with your valid subscription key.
        const string subscriptionKey = "0123456789abcdef0123456789ABCDEF";

        // You must use the same Azure region in your REST API method as you used to get your subscription keys. 
        const string uriBase = "https://westus.api.cognitive.microsoft.com/vision/v2.0/analyze";

        static void Main()
        {
            Console.WriteLine("Detect objects in images:");

            string imageFilePath = @"Images\sample6.png";
            string remoteImageUrl = "https://github.com/harishkrishnav/cognitive-services-dotnet-sdk-samples/raw/master/ComputerVision/Images/sample4.png";

            Console.WriteLine("Images being analyzed ...");
            var t1 = AnalyzeFromStreamAsync(imageFilePath);
            var t2 = AnalyzeFromUrlAsync(remoteImageUrl);

            Task.WhenAll(t1, t2).Wait(5000);
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }
                
        static async Task AnalyzeFromStreamAsync(string imageFilePath)
        {
            if (!File.Exists(imageFilePath))
            {
                Console.WriteLine("Invalid file path");
                return;
            }

            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                // Request parameters. A third optional parameter is "details".
                // Comment parameters that aren't required
                string requestParameters = "visualFeatures=" +
                    "Categories," +
                    "Description," +
                    "Color, " +
                    "Tags, " +
                    "Faces, " +
                    "ImageType, " +
                    "Adult , " +
                    "Brands , " +
                    "Objects"
                    ;

                // Assemble the URI for the REST API method.
                string uri = uriBase + "?" + requestParameters;

                // Read the contents of the specified local image
                // into a byte array.
                byte[] byteData = GetImageAsByteArray(imageFilePath);

                // Add the byte array as an octet stream to the request body.
                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses the "application/octet-stream" content type.
                    // The other content types you can use are "application/json" and "multipart/form-data".
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    // Asynchronously call the REST API method.
                    HttpResponseMessage response = await client.PostAsync(uri, content);
                    // Asynchronously get the JSON response.
                    string contentString = await response.Content.ReadAsStringAsync();

                    // Display the JSON response.
                    Console.WriteLine("\nResponse:\n\n{0}\n", JToken.Parse(contentString).ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
        }
        
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            // Open a read-only file stream for the specified file.
            using (FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                // Read the file's contents into a byte array.
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }

        static async Task AnalyzeFromUrlAsync(string imageUrl)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine("\nInvalid remote image url:\n{0} \n", imageUrl);
                return;
            }

            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                // Request parameters. A third optional parameter is "details".
                // Comment parameters that aren't required
                string requestParameters = "visualFeatures=" +
                    "Categories," +
                    "Description," +
                    "Color, " +
                    "Tags, " +
                    "Faces, " +
                    "ImageType, " +
                    "Adult , " +
                    "Brands , " +
                    "Objects"
                    ;

                //Assemble the URI and content header for the REST API request
                string uri = uriBase + "?" + requestParameters;

                string requestBody = " {\"url\":\"" + imageUrl + "\"}";
                var content = new StringContent(requestBody);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                // Post the request and display the result
                HttpResponseMessage response = await client.PostAsync(uri, content);
                string contentString = await response.Content.ReadAsStringAsync();
                Console.WriteLine("\nResponse:\n\n{0}\n", JToken.Parse(contentString).ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
        }

    }
}