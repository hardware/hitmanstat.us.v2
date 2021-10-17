using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using hitmanstat.us.Options;

namespace hitmanstat.us.Framework
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPolicies(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PolicyOptions>(configuration);

            var policyOptions = configuration
                .GetSection(nameof(ApplicationOptions.Policies))
                .Get<PolicyOptions>();

            var policyRegistry = services.AddPolicyRegistry();

            // Retry Policy
            policyRegistry.Add(
                PolicyName.HttpRetry,
                HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .Or<TimeoutRejectedException>()
                    .WaitAndRetryAsync(
                        policyOptions.HttpRetry.Count,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(policyOptions.HttpRetry.BackoffPower, retryAttempt))));
            // Timeout policy
            policyRegistry.Add(
                PolicyName.HttpTimeout,
                Policy.TimeoutAsync<HttpResponseMessage>(policyOptions.HttpTimeout.Timeout));
            // CircuitBreaker Policy
            policyRegistry.Add(
                PolicyName.HttpCircuitBreaker,
                HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .CircuitBreakerAsync(
                        handledEventsAllowedBeforeBreaking: policyOptions.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking,
                        durationOfBreak: policyOptions.HttpCircuitBreaker.DurationOfBreak));

            return services;
        }

        public static IServiceCollection AddHttpClient<TClient, TImplementation, TClientOptions>(
            this IServiceCollection services,
            IConfiguration configuration,
            string configurationSectionName)
            where TClient : class
            where TImplementation : class, TClient
            where TClientOptions : HttpClientOptions, new() =>
            services
                .Configure<TClientOptions>(configuration.GetSection(configurationSectionName))
                .AddHttpClient<TClient, TImplementation>()
                .ConfigureHttpClient(
                    (serviceProvider, httpClient) =>
                    {
                        var httpClientOptions = serviceProvider
                            .GetRequiredService<IOptions<TClientOptions>>()
                            .Value;
                        httpClient.BaseAddress = httpClientOptions.BaseAddress;
                        httpClient.Timeout = httpClientOptions.Timeout;
                    })
                .ConfigurePrimaryHttpMessageHandler(x => new DefaultHttpClientHandler())
                .AddPolicyHandlerFromRegistry(PolicyName.HttpRetry)
                .AddPolicyHandlerFromRegistry(PolicyName.HttpTimeout)
                .AddPolicyHandlerFromRegistry(PolicyName.HttpCircuitBreaker)
                .Services;
    }
}
