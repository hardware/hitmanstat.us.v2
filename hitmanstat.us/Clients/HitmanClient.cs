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
        private readonly HttpClient HttpClient;

        public HitmanClient(HttpClient httpClient) => HttpClient = httpClient;

        public async Task<EndpointStatus> GetStatusAsync()
        {
            HttpResponseMessage response = null;
            var endpoint = new EndpointStatus();

            try
            {
                response = await HttpClient.GetAsync("/status");
                response.EnsureSuccessStatusCode();

                if (!Utilities.IsJsonResponse(response.Content.Headers))
                {
                    endpoint.Status = "Bad data returned by authentication server";
                }
                else
                {
                    endpoint.State = EndpointState.Up;
                }
            }
            catch (TimeoutRejectedException)
            {
                endpoint.Status = "Authentication server connection timeout";
            }
            catch (BrokenCircuitException)
            {
                endpoint.Status = "Authentication server is down";
            }
            catch (HttpRequestException e)
            {
                if (response != null)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.InternalServerError:
                            endpoint.Status = "Internal authentication server error";
                            break;
                        case HttpStatusCode.BadGateway:
                        case HttpStatusCode.ServiceUnavailable:
                        case HttpStatusCode.GatewayTimeout:
                            endpoint.State = EndpointState.Maintenance;
                            endpoint.Status = "Temporary Azure backend maintenance";
                            break;
                        default:
                            endpoint.Status = string.Format(
                                "Error code returned by authentication server - error HTTP {0}",
                                response.StatusCode);
                            break;
                    }
                }
                else
                {
                    endpoint.Status = e.Message;
                }
            }
            catch (OperationCanceledException)
            {
                endpoint.Status = "Authentication server connection timeout";
            }
            finally
            {
                if(endpoint.State == EndpointState.Up)
                {
                    endpoint.Status = await response.Content.ReadAsStringAsync();
                }
            }

            return endpoint;
        }
    }
}
