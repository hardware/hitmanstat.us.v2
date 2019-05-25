using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Polly.Timeout;
using Polly.CircuitBreaker;
using hitmanstat.us.Models;
using hitmanstat.us.Framework;

namespace hitmanstat.us.Clients
{
    public class HitmanClient : IHitmanClient
    {
        private readonly HttpClient httpClient;

        public HitmanClient(HttpClient client) => httpClient = client;

        public async Task<ServiceStatus> GetStatus()
        {
            HttpResponseMessage response = null;
            var service = new ServiceStatus();

            try
            {
                response = await httpClient.GetAsync("/status");
                response.EnsureSuccessStatusCode();

                if (!Utilities.IsJsonResponse(response.Content.Headers))
                {
                    service.Status = "Bad data returned by authentication server";
                }
                else
                {
                    service.State = ServiceState.Up;
                }
            }
            catch (TimeoutRejectedException)
            {
                service.Status = "Authentication server connection timeout";
            }
            catch (BrokenCircuitException)
            {
                service.Status = "Authentication server is down";
            }
            catch (HttpRequestException)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.InternalServerError:
                        service.Status = "Internal authentication server error";
                        break;
                    case HttpStatusCode.BadGateway:
                    case HttpStatusCode.ServiceUnavailable:
                    case HttpStatusCode.GatewayTimeout:
                        service.State = ServiceState.Maintenance;
                        service.Status = "Temporary Azure backend maintenance";
                        break;
                    default:
                        service.Status = string.Format("Unhandled error code returned by authentication server - error HTTP {0}", response.StatusCode);
                        break;
                }
            }
            finally
            {
                if(service.State == ServiceState.Up)
                {
                    service.Status = await response.Content.ReadAsStringAsync();
                }
            }

            return service;
        }
    }
}
