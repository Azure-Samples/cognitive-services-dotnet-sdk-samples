using Microsoft.CognitiveServices.ContentModerator;
using Microsoft.CognitiveServices.ContentModerator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VideoTranscriptReviews
{
    public static class VideoHelper
    {
        /// <summary>
        /// Posts the Review Video object and returns a result.
        /// </summary>
        /// <param name="reviewVideoObj">Reviewvideo requestJson</param>
        /// <returns>Review Id</returns>
        public static async Task<string> ExecuteCreateReviewApi(
            ContentModeratorClient CMClient, string TeamName, List<CreateVideoReviewsBodyItem> reviewBody)
        {
            string resultJson = string.Empty;
            try
            {
                HttpResponseMessage response;

                var oResponse =
                    await CMClient.Reviews.CreateVideoReviewsWithHttpMessagesAsync(
                        "application/json", TeamName, reviewBody);
                response = oResponse.Response;
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(
                        $"ExecuteCreateReviewApi has failed to get a review. Code: {response.StatusCode}");
                }
                resultJson = await response.Content.ReadAsStringAsync();

                return resultJson;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}
