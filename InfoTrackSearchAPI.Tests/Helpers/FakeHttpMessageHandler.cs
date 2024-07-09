using System.Net;

namespace InfoTrackSearchAPI.Tests.Helpers;

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly string _response;
    private readonly Exception _exception;

    public FakeHttpMessageHandler(string response, Exception exception = null)
    {
        _response = response;
        _exception = exception;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_exception != null)
        {
            throw _exception;
        }

        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(_response)
        };

        return Task.FromResult(httpResponseMessage);
    }
}