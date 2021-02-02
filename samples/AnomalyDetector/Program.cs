namespace Azure.AI.AnomalyDetector.Samples
{
    using Azure;
    using Azure.AI.AnomalyDetector;
    using Azure.AI.AnomalyDetector.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class Program
    {
        static void Main(string[] args)
        {
            // Add your Computer Vision subscription key and endpoint to your environment variables
            string endpoint = Environment.GetEnvironmentVariable("ANOMALY_DETECTOR_ENDPOINT");
            string key = Environment.GetEnvironmentVariable("ANOMALY_DETECTOR_SUBSCRIPTION_KEY");

            // Anomaly detection samples.
            try
            {
                EntireDetectSample.RunAsync(endpoint, key).Wait();
                LastDetectSample.RunAsync(endpoint, key).Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (e.InnerException != null && e.InnerException is RequestFailedException exception)
                {
                    Console.WriteLine("Error code: " + exception.ErrorCode);
                    Console.WriteLine("Error message: " + exception.Message);
                }
                else if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException.Message);
                }
            }

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }


        public static List<TimeSeriesPoint> GetSeriesFromFile(string path)
        {
            return File.ReadAllLines(path, Encoding.UTF8)
                .Where(e => e.Trim().Length != 0)
                .Select(e => e.Split(','))
                .Where(e => e.Length == 2)
                .Select(e => new TimeSeriesPoint(DateTime.Parse(e[0]), float.Parse(e[1]))).ToList();
        }

        public static DetectRequest GetRequest()
        {
            List<TimeSeriesPoint> series = GetSeriesFromFile("anomaly_detector_daily_series.csv");
            return new DetectRequest(series, TimeGranularity.Daily);
        }
    }

    public static class EntireDetectSample
    {
        public static async Task RunAsync(string endpoint, string key)
        {
            Console.WriteLine("Sample of detecting anomalies in the entire series.");

            AnomalyDetectorClient client = new AnomalyDetectorClient(new Uri(endpoint), new AzureKeyCredential(key));

            // Detection
            var request = Program.GetRequest();
            request.MaxAnomalyRatio = 0.25F;
            request.Sensitivity = 95;
            var result = await client.DetectEntireSeriesAsync(request).ConfigureAwait(false);

            if (result.Value.IsAnomaly.Contains(true))
            {
                Console.WriteLine("Anomaly was detected from the series at index:");
                for (int i = 0; i < request.Series.Count; ++i)
                {
                    if (result.Value.IsAnomaly[i])
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

                AnomalyDetectorClient client = new AnomalyDetectorClient(new Uri(endpoint), new AzureKeyCredential(key));

                // Detection
                var request = Program.GetRequest();
                request.MaxAnomalyRatio = 0.25F;
                request.Sensitivity = 95;
                var result = await client.DetectLastPointAsync(request).ConfigureAwait(false);

                if (result.Value.IsAnomaly)
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
 
