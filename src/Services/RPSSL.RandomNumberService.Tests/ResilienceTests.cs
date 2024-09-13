using Microsoft.Extensions.DependencyInjection;
using Polly.CircuitBreaker;
using RPSSL.RandomNumberService.Clients;
using Polly;
using FluentAssertions;
using System.Net;
using Polly.Timeout;

namespace RPSSL.RandomNumberService.Tests
{
    public class ResilienceTests
    {
        // basic idea and code taken from 
        // https://josef.codes/testing-your-polly-policies/

        [Fact]
        public async Task ShouldRetry_FailingRequests_ThreeTimes()
        {
            var services = new ServiceCollection();
            var fakeHttpDelegatingHandler = new FakeHttpDelegatingHandler(
                _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.GatewayTimeout)));
            var policy =
                ResiliencePolicies.CreateRetryPolicy(sleepDurationProvider: _ => TimeSpan.FromMilliseconds(1));
            services.AddHttpClient("my-httpclient", client =>
            {
                client.BaseAddress = new Uri("http://any.localhost");
            })
            .AddPolicyHandler(policy)
            .AddHttpMessageHandler(() => fakeHttpDelegatingHandler);
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var sut = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("my-httpclient");
            var request = new HttpRequestMessage(HttpMethod.Get, "/any");

            var result = await sut.SendAsync(request);

            result.StatusCode.Should().Be(HttpStatusCode.GatewayTimeout);
            fakeHttpDelegatingHandler.Attempts.Should().Be(4); // initial plus 3 retries
        }

        [Fact]
        public async Task ShouldStopRetrying_IfSuccess_StatusCode_IsEncountered()
        {
            var services = new ServiceCollection();
            var fakeHttpDelegatingHandler = new FakeHttpDelegatingHandler(
                attempt =>
                {
                    return attempt switch
                    {
                        2 => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)),
                        _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.GatewayTimeout))
                    };
                });
            var policy =
                ResiliencePolicies.CreateRetryPolicy(sleepDurationProvider: _ => TimeSpan.FromMilliseconds(1));
            services.AddHttpClient("my-httpclient", client =>
            {
                client.BaseAddress = new Uri("http://any.localhost");
            })
            .AddPolicyHandler(policy)
            .AddHttpMessageHandler(() => fakeHttpDelegatingHandler);
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var sut = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("my-httpclient");
            var request = new HttpRequestMessage(HttpMethod.Get, "/any");

            var result = await sut.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            fakeHttpDelegatingHandler.Attempts.Should().Be(2);
        }

        [Fact]
        public async Task TimeoutPolicy_ShouldThrowTimeoutRejectedException_WhenRequestExceedsTimeout()
        {
            var services = new ServiceCollection();
            var fakeHttpDelegatingHandler = new FakeHttpDelegatingHandler(
                _ => Task.Delay(TimeSpan.FromMilliseconds(100)).ContinueWith(_ => new HttpResponseMessage(HttpStatusCode.OK))
            );

            var timeoutPolicy = ResiliencePolicies.CreateTimoutPolicy(TimeSpan.FromMilliseconds(10));

            services.AddHttpClient("my-httpclient", client =>
            {
                client.BaseAddress = new Uri("http://any.localhost");
            })
            .AddPolicyHandler(timeoutPolicy)
            .AddHttpMessageHandler(() => fakeHttpDelegatingHandler);

            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var sut = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("my-httpclient");
            var request = new HttpRequestMessage(HttpMethod.Get, "/any");

            await Assert.ThrowsAsync<TimeoutRejectedException>(async () => await sut.SendAsync(request, HttpCompletionOption.ResponseHeadersRead));
        }        

        [Fact]
        public async Task CircuitBreakerPolicy_ShouldOpenCircuit_AfterSpecifiedFailures()
        {
            var services = new ServiceCollection();
            var fakeHttpDelegatingHandler = new FakeHttpDelegatingHandler(
                _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

            var circuitBreakerPolicy = ResiliencePolicies.CreateCircuitBreakerPolicy(eventsBeforeBreaking: 3, durationOfBreak: TimeSpan.FromSeconds(30));

            services.AddHttpClient("my-httpclient", client =>
            {
                client.BaseAddress = new Uri("http://any.localhost");
            })
            .AddPolicyHandler(circuitBreakerPolicy)
            .AddHttpMessageHandler(() => fakeHttpDelegatingHandler);

            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("my-httpclient");

            // First 3 calls should fail but not break the circuit
            for (int i = 0; i < 3; i++)
            {
                var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/any"));
                response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            }

            // The 4th call should throw BrokenCircuitException
            await Assert.ThrowsAsync<BrokenCircuitException<HttpResponseMessage>>(() =>
                httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/any")));

            fakeHttpDelegatingHandler.Attempts.Should().Be(3);
        }

        [Fact]
        public async Task CombinedPolicies_ShouldWorkTogether_AsExpected()
        {
            // Arrange
            var services = new ServiceCollection();

            // fakeHttpDelegatingHandler is not called when Cirquit is Open
            var fakeHttpDelegatingHandler = new FakeHttpDelegatingHandler(attempt => attempt switch
            {
                1 => Task.FromResult(new HttpResponseMessage(HttpStatusCode.GatewayTimeout)),
                2 => Task.FromResult(new HttpResponseMessage(HttpStatusCode.GatewayTimeout)),
                3 => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)),
                4 => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)),
                5 => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)),
                _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK))
            });

            var retryPolicy = ResiliencePolicies.CreateRetryPolicy(retryCount: 2, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(1));
            var circuitBreakerPolicy = ResiliencePolicies.CreateCircuitBreakerPolicy(eventsBeforeBreaking: 3, durationOfBreak: TimeSpan.FromMilliseconds(200));
            var timeoutPolicy = ResiliencePolicies.CreateTimoutPolicy(TimeSpan.FromMilliseconds(50));

            var combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);

            services.AddHttpClient("combined-policy-client", client =>
            {
                client.BaseAddress = new Uri("http://dummy.localhost");
            })
            .AddPolicyHandler(combinedPolicy)
            .AddHttpMessageHandler(() => fakeHttpDelegatingHandler);

            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("combined-policy-client");

            // Act & Assert
            var request = new HttpRequestMessage(HttpMethod.Get, "/any");
            var result = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            // First atempt is time outed, and Retry Policy tries two additonal times
            fakeHttpDelegatingHandler.Attempts.Should().Be(3);
            
            // fourth try is failed, but now kicks in circuit breaker policy, API is not called
            await Assert.ThrowsAsync<BrokenCircuitException<HttpResponseMessage>>(() => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/any")));
            fakeHttpDelegatingHandler.Attempts.Should().Be(3);

            await Task.Delay(TimeSpan.FromMilliseconds(200));
            // fifth time, Circuit is half open, but beacuse it is bad call again Broken Cirquit exception is thrown          
            await Assert.ThrowsAsync<BrokenCircuitException<HttpResponseMessage>>(() => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/any")));                        
            fakeHttpDelegatingHandler.Attempts.Should().Be(4);

            // during the Open Cirquit, API is not called, and also whole timeout is not extended
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            await Assert.ThrowsAsync<BrokenCircuitException<HttpResponseMessage>>(() => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/any")));

            // good call, Circuit is half open, but Ok response is ok    
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            var response2 = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/any"));
            response2.StatusCode.Should().Be(HttpStatusCode.OK);
            fakeHttpDelegatingHandler.Attempts.Should().Be(5);            
        }       
    }
}
