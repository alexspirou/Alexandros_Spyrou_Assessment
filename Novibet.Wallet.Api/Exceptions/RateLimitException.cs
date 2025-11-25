namespace Novibet.Wallet.Api.Exceptions;
using System.Net;


public sealed class RateLimitException : Exception
{
    public TimeSpan RetryAfter { get; }

    // Always 429 for this exception
    public HttpStatusCode StatusCode => HttpStatusCode.TooManyRequests;

    public RateLimitException(string message, TimeSpan retryAfter)
        : base(message)
    {
        RetryAfter = retryAfter;
    }
}
