#region License
// Copyright (c) 2023 Ayan Choudhury
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion

using Coracle.Raft.Engine.Configuration.Cluster;
using Coracle.Raft.Engine.Discovery;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Coracle.Web.Impl.Discovery
{
    public class HttpDiscoveryHandler : IDiscoveryHandler
    {
        public HttpDiscoveryHandler(IHttpClientFactory httpClientFactory, IOptions<EngineConfigurationOptions> engineConfigurationOptions)
        {
            HttpClientFactory = httpClientFactory;
            EngineConfigurationOptions = engineConfigurationOptions;
        }

        public IHttpClientFactory HttpClientFactory { get; set; }
        public IOptions<EngineConfigurationOptions> EngineConfigurationOptions { get; }

        public async Task<DiscoveryResult> Enroll(INodeConfiguration configuration, CancellationToken cancellationToken)
        {
            var client = HttpClientFactory.CreateClient();

            var enrollUri = new Uri(EngineConfigurationOptions.Value.DiscoveryServerUri, Constants.Discovery.Enroll);

            var response = await client.PostAsJsonAsync(enrollUri, configuration, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var contentString = await response.Content.ReadAsStringAsync(cancellationToken);

                return JsonConvert.DeserializeObject<DiscoveryResult>(contentString);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public async Task<DiscoveryResult> GetAllNodes(CancellationToken cancellationToken)
        {
            var client = HttpClientFactory.CreateClient();

            var getUri = new Uri(EngineConfigurationOptions.Value.DiscoveryServerUri, Constants.Discovery.GetCluster);

            var response = await client.GetAsync(getUri, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var contentString = await response.Content.ReadAsStringAsync(cancellationToken);

                return JsonConvert.DeserializeObject<DiscoveryResult>(contentString);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
