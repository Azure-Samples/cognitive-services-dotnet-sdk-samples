/*
 * Copyright (c) 2019
 * Released under the MIT license
 * http://opensource.org/licenses/mit-license.php
 */

using Microsoft.Extensions.Configuration;
using System;
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
            VisualModerator visualModerator = new VisualModerator(config);

            string videoPath = "https://shigeyfampdemo.azurewebsites.net/videos/ignite.mp4";

            try
            {
                Task<VisualModerationResult> task = visualModerator.ModerateVideo(videoPath);
                VisualModerationResult result = task.Result;
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
