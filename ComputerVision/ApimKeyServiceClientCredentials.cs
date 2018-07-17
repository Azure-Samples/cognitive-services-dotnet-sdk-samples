using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Rest;

namespace ComputerVisionSample
{
    class ApimKeyServiceClientCredentials : ServiceClientCredentials
    {
        private readonly string subscriptionKey;

        /// <summary>
        /// Creates a new instance of the ApimKeyServiceClientCredentials class
        /// </summary>
        /// <param name="subscriptionKey">The subscription key to authenticate and authorize as</param>
        public ApimKeyServiceClientCredentials(string subscriptionKey)
        {
            if (string.IsNullOrWhiteSpace(subscriptionKey))
                throw new ArgumentNullException("subscriptionKey");

            this.subscriptionKey = subscriptionKey;
        }

        /// <summary>
        /// Add the Basic Authentication Header to each outgoing request
        /// </summary>
        /// <param name="request">The outgoing request</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            request.Headers.Add("Ocp-Apim-Subscription-Key", this.subscriptionKey);

            return Task.FromResult<object>(null);
        }
    }
}
