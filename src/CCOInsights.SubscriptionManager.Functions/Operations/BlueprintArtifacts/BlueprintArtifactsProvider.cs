﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CCOInsights.SubscriptionManager.Functions.Operations.Blueprint;
using CCOInsights.SubscriptionManager.Helpers;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Newtonsoft.Json;

namespace CCOInsights.SubscriptionManager.Functions.Operations.BlueprintArtifacts
{
    public interface IBlueprintArtifactProvider : IProvider<BlueprintArtifactsResponse> { }
    public class BlueprintArtifactsProvider : IBlueprintArtifactProvider
    {
        private readonly IBlueprintProvider _blueprintProvider;
        private readonly RestClient _restClient;
        private readonly IHttpClientFactory _httpClientFactory;

        public BlueprintArtifactsProvider(IBlueprintProvider blueprintProvider, IHttpClientFactory httpClientFactory, RestClient restClient)
        {
            _blueprintProvider = blueprintProvider;
            _httpClientFactory = httpClientFactory;
            _restClient = restClient;
        }

        public async Task<IEnumerable<BlueprintArtifactsResponse>> GetAsync(string subscriptionId, CancellationToken cancellationToken = default)
        {
            var bluePrints = await _blueprintProvider.GetAsync(subscriptionId, cancellationToken);
            var result = new List<BlueprintArtifactsResponse>();

            await bluePrints.AsyncParallelForEach(async bluePrint =>
                {
                    var httpClient = _httpClientFactory.CreateClient("client");

                    var response = await GetModelAsync(httpClient, $"https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.Blueprint/blueprints/{bluePrint.Name}/artifacts?api-version=2018-11-01-preview", cancellationToken);

                    result.AddRange(response.Value);

                    while (!string.IsNullOrEmpty(response.NextLink))
                    {
                        response = await GetModelAsync(httpClient, response.NextLink, cancellationToken);
                        result.AddRange(response.Value);
                    }
                });

            return result;
        }

        private async Task<ProviderResponse<BlueprintArtifactsResponse>> GetModelAsync(HttpClient client, string url, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            await _restClient.Credentials.ProcessHttpRequestAsync(request, cancellationToken);
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            return JsonConvert.DeserializeObject<ProviderResponse<BlueprintArtifactsResponse>>(content);
        }
    }
}
