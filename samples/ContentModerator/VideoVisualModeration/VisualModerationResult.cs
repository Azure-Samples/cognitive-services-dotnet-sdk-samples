/*
 * Copyright (c) 2019
 * Released under the MIT license
 * http://opensource.org/licenses/mit-license.php
 */


namespace Microsoft.ContentModerator.VideoContentModerator
{
    public class VisualModerationResult
    {
        public string VideoName { get; set; }
        public string VideoFilePath { get; set; }
        public PublishedUrlDetails StreamingUrlDetails { get; set; }
        public string AccessToken { get; set; }
        public string VisualModerationJson { get; set; }
        public string OcrJson { get; set; }
    }

    public class PublishedUrlDetails
    {
        public string SmoothUri { get; set; }
        public string MpegDashUri { get; set; }
        public string HlsUri { get; set; }
        public string UrlWithOriginLocator { get; set; }
        public string DownloadUri { get; set; }
        public string VttUrl { get; set; }
        public string ContentModerationJsonUrl { get; set; }
        public string OcrJsonUrl { get; set; }
    }
}
