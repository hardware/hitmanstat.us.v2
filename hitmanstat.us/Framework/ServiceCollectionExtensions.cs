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
        private const string PoliciesConfigurationSectionName = "Policies";

        public static IServiceCollection AddPolicies(this IServiceCollection services, IConfiguration configuration)
        {
            var section = configuration.GetSection(PoliciesConfigurationSectionName); // nameof(ApplicationOptions.Policies)
            services.Configure<PolicyOptions>(configuration);
            var policyOptions = section.Get<PolicyOptions>();
            var policyRegistry = services.AddPolicyRegistry();

            // Timeout policy
            policyRegistry.Add(
                PolicyName.HttpTimeout,
                Policy.TimeoutAsync<HttpResponseMessage>(policyOptions.HttpTimeout.Timeout));
            // Retry Policy
            policyRegistry.Add(
                PolicyName.HttpRetry,
                HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .Or<TimeoutRejectedException>()
                    .RetryAsync(policyOptions.HttpRetry.Count));
            // CircuitBreaker Policy
            policyRegistry.Add(
                PolicyName.HttpCircuitBreaker,
                HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .Or<TimeoutRejectedException>()
                    .AdvancedCircuitBreakerAsync(
                         failureThreshold: policyOptions.HttpCircuitBreaker.FailureThreshold,
                         samplingDuration: policyOptions.HttpCircuitBreaker.SamplingDuration,
                        minimumThroughput: policyOptions.HttpCircuitBreaker.MinimumThroughput,
                          durationOfBreak: policyOptions.HttpCircuitBreaker.DurationOfBreak));

            return services;
        }

        public static IServiceCollection AddHttpClient<TClient, TImplementation, TClientOptions>(
            this IServiceCollection services, IConfiguration configuration, string configurationSectionName)
            where TClient : class
            where TImplementation : class, TClient
            where TClientOptions : HttpClientOptions, new() =>
            services
                .Configure<TClientOptions>(configuration.GetSection(configurationSectionName))
                .AddHttpClient<TClient, TImplementation>()
                .ConfigureHttpClient((serviceProvider, options) =>
                {
                    var httpClientOptions = serviceProvider
                        .GetRequiredService<IOptions<TClientOptions>>()
                        .Value;
                    options.BaseAddress = httpClientOptions.BaseAddress;
                    options.Timeout = httpClientOptions.Timeout;
                })
                .ConfigurePrimaryHttpMessageHandler(x => new DefaultHttpClientHandler())
                .AddPolicyHandlerFromRegistry(PolicyName.HttpRetry)
                .AddPolicyHandlerFromRegistry(PolicyName.HttpCircuitBreaker)
                .AddPolicyHandlerFromRegistry(PolicyName.HttpTimeout)
                .Services;
    }
}
