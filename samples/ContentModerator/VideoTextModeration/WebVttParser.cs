/*
 * Copyright (c) 2019
 * Released under the MIT license
 * http://opensource.org/licenses/mit-license.php
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Microsoft.ContentModerator.VideoContentModerator
{
    public class WebVttParser
    {
        public static List<CaptionTextModerationResult> ParseWebVtt(string webVtt)
        {
            List<CaptionTextModerationResult> captions = new List<CaptionTextModerationResult>();
            string[] webVttLines = Regex.Split(webVtt, "\r\n|\r|\n");

            if (!(webVttLines[0] == "WEBVTT" && webVttLines[1] == ""))
            {
                throw new Exception("Invalid WebVTT");
            }

            bool isNoteOpened = false;
            bool isCaptionOpened = false;
            CaptionTextModerationResult caption = null;
            for (int i = 2; i < webVttLines.Length; i++)
            {
                string line = webVttLines[i];
                if (line == "NOTE")
                {
                    isNoteOpened = true;
                }
                else if (line.StartsWith("NOTE: "))
                {
                    // ignore this line
                }
                else if (line == "")
                {
                    if (isCaptionOpened)
                    {
                        captions.Add(caption);
                        isCaptionOpened = false;
                    }
                    if (isNoteOpened)
                    {
                        isNoteOpened = false;
                    }
                }
                else if (Regex.IsMatch(line, @"^[0-9][0-9][:][0-9][0-9][:][0-9][0-9][.][0-9][0-9][0-9][ ][-][-][>][ ][0-9][0-9][:][0-9][0-9][:][0-9][0-9][.][0-9][0-9][0-9]$"))
                {
                    isCaptionOpened = true;
                    string[] captionTime = Regex.Split(line, " --> ");
                    caption = new CaptionTextModerationResult();
                    caption.Captions = new List<string>();
                    caption.StartTime = (int)TimeSpan.ParseExact(captionTime[0], @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture).TotalMilliseconds;
                    caption.EndTime = (int)TimeSpan.ParseExact(captionTime[1], @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture).TotalMilliseconds;
                }
                else
                {
                    if (isCaptionOpened)
                    {
                        caption.Captions.Add(line);
                    }
                }
            }

            return captions;
        }

        public static async Task<string> LoadWebVtt(string webVttUrl)
        {
            string webVttString = string.Empty;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Call asynchronous network methods in a try/catch block to handle exceptions
                    HttpResponseMessage response = await client.GetAsync(webVttUrl);
                    response.EnsureSuccessStatusCode();
                    webVttString = await response.Content.ReadAsStringAsync();
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }

            return webVttString;
        }
    }
}
