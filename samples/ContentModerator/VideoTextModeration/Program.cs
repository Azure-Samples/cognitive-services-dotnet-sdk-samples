/*
 * Copyright (c) 2019
 * Released under the MIT license
 * http://opensource.org/licenses/mit-license.php
 */

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


namespace Microsoft.ContentModerator.VideoContentModerator
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigWrapper config = new ConfigWrapper(new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build());
            TextModerator textModerator = new TextModerator(config);

            string videoPath = "https://shigeyfampdemo.azurewebsites.net/videos/ignite.mp4";

            try
            {
                Task<TextModerationResult> task1 = textModerator.ModerateVideo(videoPath);
                TextModerationResult result = task1.Result;
                Task<string> task2 = WebVttParser.LoadWebVtt(result.StreamingUrlDetails.VttUrl);
                result.WebVtt = task2.Result;
                List<CaptionTextModerationResult> captionTextResults = WebVttParser.ParseWebVtt(result.WebVtt);
                Task<List<CaptionTextModerationResult>> task3 = textModerator.TextScreen(captionTextResults);
                result.CaptionTextResults = task3.Result;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: ", e.ToString());
            }
            Console.WriteLine("Visual Modeation has finished.");
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();
        }
    }
}
