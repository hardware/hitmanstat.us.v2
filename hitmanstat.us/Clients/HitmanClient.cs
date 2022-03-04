using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Polly.Timeout;
using Polly.CircuitBreaker;
using hitmanstat.us.Models;
using hitmanstat.us.Framework;

namespace hitmanstat.us.Clients
{
    public class HitmanClient : IHitmanClient
    {
        private readonly HttpClient HttpClient;
        private readonly TelemetryClient TelemetryClient;

        public HitmanClient(HttpClient httpClient, TelemetryClient telemetryClient)
        {
            HttpClient = httpClient;
            TelemetryClient = telemetryClient;
        }

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
                            endpoint.Status = "Authentication server critical error";
                            break;
                        case HttpStatusCode.BadGateway:
                        case HttpStatusCode.ServiceUnavailable:
                        case HttpStatusCode.GatewayTimeout:
                            endpoint.State = EndpointState.Maintenance;
                            endpoint.Status = "Authentication server unavailable";
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
                endpoint.Status = "Authentication server connection canceled";
            }
            finally
            {
                if(endpoint.State == EndpointState.Up)
                {
                    endpoint.Status = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    TelemetryClient.TrackEvent("HitmanTransientHTTPError");
                }
            }

            return endpoint;
        }
    }
}
