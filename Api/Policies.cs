using System.Net;
using Polly;
using Polly.Extensions.Http;

public static class Policies
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
            );

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 2,
                durationOfBreak: TimeSpan.FromSeconds(30)
            );

    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy() =>
        Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));

    public static IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy() =>
        Policy<HttpResponseMessage>
            .Handle<Exception>()
            .FallbackAsync(
                fallbackValue: new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"message\":\"fallback\"}")
                },
                onFallbackAsync: async (exception, ctx) =>
                {
                    Console.WriteLine($"[Fallback] {exception.Exception?.Message}");
                }
            );
}
