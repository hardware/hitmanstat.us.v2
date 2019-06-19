using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly.Timeout;
using Polly.CircuitBreaker;
using hitmanstat.us.Models;

namespace hitmanstat.us.Clients
{
    public class HitmanForumClient : IHitmanForumClient
    {
        private readonly HttpClient HttpClient;

        public HitmanForumClient(HttpClient httpClient) => HttpClient = httpClient;

        public async Task<EndpointStatus> GetStatusAsync()
        {
            var endpoint = new EndpointStatus();
            HttpResponseMessage response = null;

            try
            {
                response = await HttpClient.GetAsync("/");
                response.EnsureSuccessStatusCode();
                endpoint.State = EndpointState.Up;
            }
            catch (TimeoutRejectedException)
            {
                endpoint.Status = "timeout";
            }
            catch (BrokenCircuitException)
            {
                endpoint.Status = "broken";
            }
            catch (HttpRequestException e)
            {
                if(response != null)
                {
                    endpoint.Status = response.StatusCode.ToString();
                }
                else
                {
                    endpoint.Status = e.Message;
                }
            }
            catch (OperationCanceledException)
            {
                endpoint.Status = "timeout";
            }

            return endpoint;
        }
    }
}
