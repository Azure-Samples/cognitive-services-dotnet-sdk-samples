namespace Microsoft.Azure.CognitiveServices.Samples.AnomalyDetector
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.AnomalyDetector;
    using Microsoft.Azure.CognitiveServices.AnomalyDetector.Models;

    class Program
    {
        static void Main(string[] args)
        {
            string endpoint = "ENTER YOUR ENDPOINT HERE";
            string key = "ENTER YOUR KEY HERE";

            // Anomaly detection samples.
            try
            {
                EntireDetectSample.RunAsync(endpoint, key).Wait();
                LastDetectSample.RunAsync(endpoint, key).Wait();
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


        public static List<Point> GetSeriesFromFile(string path)
        {
            return File.ReadAllLines(path, Encoding.UTF8)
                .Where(e => e.Trim().Length != 0)
                .Select(e => e.Split(','))
                .Where(e => e.Length == 2)
                .Select(e => new Point(DateTime.Parse(e[0]), Double.Parse(e[1]))).ToList();
        }

        public static Request GetRequest()
        {
            List<Point> series = GetSeriesFromFile("anomaly_detector_daily_series.csv");
            return new Request(series, Granularity.Daily);
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

            // Detection
            Request request = Program.GetRequest();
            request.MaxAnomalyRatio = 0.25;
            request.Sensitivity = 95;
            EntireDetectResponse result = await client.EntireDetectAsync(request).ConfigureAwait(false);

            if (result.IsAnomaly.Contains(true))
            {
                Console.WriteLine("Anomaly was detected from the series at index:");
                for (int i = 0; i < request.Series.Count; ++i)
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
            Console.WriteLine("Sample of detecting whether the latest point in series is anomaly.");

            IAnomalyDetectorClient client = new AnomalyDetectorClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            // Detection
            Request request = Program.GetRequest();
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
