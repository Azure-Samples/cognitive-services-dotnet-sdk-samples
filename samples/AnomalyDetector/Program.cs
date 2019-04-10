namespace Microsoft.Azure.CognitiveServices.Samples.AnomalyDetector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.AnomalyDetector;
    using Microsoft.Azure.CognitiveServices.AnomalyDetector.Models;

    class Program
    {
        static void Main(string[] args)
        {
            string apiKey = "ENTER YOUR KEY HERE";
            string endpoint = "ENTER YOUR ENDPOINT HERE";

            // Anomaly detection samples.
            try
            {
                EntireDetectSample.RunAsync(endpoint, apiKey).Wait();
                LastDetectSample.RunAsync(endpoint, apiKey).Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if(e.InnerException != null && e.InnerException is APIErrorException)
                {
                    APIError error = ((APIErrorException)e.InnerException).Body;
                    Console.WriteLine("Error code: " + error.Code);
                    Console.WriteLine("Error message: " + error.Message);
                } else if (e.InnerException != null) {
                    Console.WriteLine(e.InnerException.Message);
                }
            }

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }

    public static class EntireDetectSample
    {
        public static async Task RunAsync(string endpoint, string key)
        {
            Console.WriteLine("Sample of detecting anomalies in the entire series.");
            
            IAnomalyDetectorClient client = new AnomalyDetectorClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            // Create time series
            var series = new List<Point>{
                new Point(DateTime.Parse("1962-01-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-02-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-03-01T00:00:00Z"), 0),
                new Point(DateTime.Parse("1962-04-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-05-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-06-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-07-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-08-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-09-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-10-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-11-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-12-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-01-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-02-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-03-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-04-01T00:00:00Z"), 0),
                new Point(DateTime.Parse("1963-05-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-06-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-07-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-08-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-09-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-10-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-11-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-12-01T00:00:00Z"), 1)
            };

            // Detection
            Request request = new Request(series, Granularity.Monthly);
            request.MaxAnomalyRatio = 0.25;
            request.Sensitivity = 95;
            EntireDetectResponse result = await client.EntireDetectAsync(request).ConfigureAwait(false);

            if (result.IsAnomaly.Contains(true))
            {
                Console.WriteLine("Anomaly was detected from the series at index:");
                for (int i = 0; i < series.Count; ++i)
                {
                    if (result.IsAnomaly[i])
                    {
                        Console.Write(i);
                        Console.Write(" ");
                    }
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("There is no anomaly detected from the series.");
            }
        }
    }

    public static class LastDetectSample
    {
        public static async Task RunAsync(string endpoint, string key)
        {
            Console.WriteLine("Sample of detecting whether the latest point in series is anomaly");

            IAnomalyDetectorClient client = new AnomalyDetectorClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            // Create time series
            var series = new List<Point>{
                new Point(DateTime.Parse("1962-01-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-02-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-03-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-04-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-05-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-06-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-07-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-08-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-09-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-10-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-11-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1962-12-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-01-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-02-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-03-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-04-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-05-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-06-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-07-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-08-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-09-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-10-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-11-01T00:00:00Z"), 1),
                new Point(DateTime.Parse("1963-12-01T00:00:00Z"), 0)
            };

            // Detection
            Request request = new Request(series, Granularity.Monthly);
            request.MaxAnomalyRatio = 0.25;
            request.Sensitivity = 95;
            LastDetectResponse result = await client.LastDetectAsync(request).ConfigureAwait(false);

            if (result.IsAnomaly)
            {
                Console.WriteLine("The latest point is detected as anomaly.");
            }
            else
            {
                Console.WriteLine("The latest point is not detected as anomaly.");
            }
        }

    }
}
