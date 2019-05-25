using System.Net.Http;
using System.Threading.Tasks;
using Polly.Timeout;
using Polly.CircuitBreaker;
using hitmanstat.us.Models;

namespace hitmanstat.us.Clients
{
    public class HitmanForumClient : IHitmanForumClient
    {
        private readonly HttpClient httpClient;

        public HitmanForumClient(HttpClient client) => httpClient = client;

        public async Task<ServiceStatus> GetStatus()
        {
            var service = new ServiceStatus();
            HttpResponseMessage response = null;

            try
            {
                response = await httpClient.GetAsync("/");
                response.EnsureSuccessStatusCode();
                service.State = ServiceState.Up;
            }
            catch (TimeoutRejectedException)
            {
                service.Status = "timeout";
            }
            catch (BrokenCircuitException)
            {
                service.Status = "broken";
            }
            catch (HttpRequestException)
            {
                service.Status = response.StatusCode.ToString();
            }
            
            return service;
        }
    }
}
