using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;

namespace FleetUpdates.Utils
{
    internal class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        private readonly string _textAnalyticsApi = System.Environment.GetEnvironmentVariable("TextAnalyticsApiKey");

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Ocp-Apim-Subscription-Key", _textAnalyticsApi);
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }
}
