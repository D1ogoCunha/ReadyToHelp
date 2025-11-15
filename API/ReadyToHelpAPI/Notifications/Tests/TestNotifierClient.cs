namespace readytohelpapi.Notifications.Tests;

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using readytohelpapi.Notifications;
using Xunit;


[Trait("Category", "Unit")]
public class TestNotifierClient
{
    private class RecordingHandler : DelegatingHandler
    {
        public readonly ConcurrentBag<HttpRequestMessage> Requests = new();

        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> responder;

        public RecordingHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder)
        {
            this.responder = responder ?? throw new ArgumentNullException(nameof(responder));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return await responder(request);
        }
    }

    private static NotificationRequest MakeRequest() => new NotificationRequest
    {
        Type = readytohelpapi.ResponsibleEntity.Models.ResponsibleEntityType.BOMBEIROS,
        EntityName = "X",
        EntityId = 1,
        OccurrenceId = 10,
        Title = "T",
        Latitude = 41.1,
        Longitude = -8.6,
        Message = "m",
        Timestamp = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task NotifyAsync_SendsPostAndSucceeds()
    {
        var handler = new RecordingHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var logger = new Mock<ILogger<NotifierClient>>();
        var client = new NotifierClient(http, logger.Object);
        var req = MakeRequest();

        await client.NotifyAsync(req);

        Assert.Single(handler.Requests);
        var sent = handler.Requests.TryPeek(out var r) ? r : null;
        Assert.NotNull(sent);
        Assert.Equal(HttpMethod.Post, sent.Method);
        Assert.Equal("/notify", sent.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task NotifyAsync_Null_ThrowsArgumentNullException()
    {
        var handler = new RecordingHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var logger = new Mock<ILogger<NotifierClient>>();
        var client = new NotifierClient(http, logger.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => client.NotifyAsync(null!));
    }

    [Fact]
    public async Task NotifyAsync_ServerError_ThrowsException()
    {
        var handler = new RecordingHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var logger = new Mock<ILogger<NotifierClient>>();
        var client = new NotifierClient(http, logger.Object);
        var req = MakeRequest();

        await Assert.ThrowsAsync<HttpRequestException>(() => client.NotifyAsync(req));
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task NotifyRepeatedAsync_SendsMultipleRequests()
    {
        var handler = new RecordingHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var logger = new Mock<ILogger<NotifierClient>>();
        var client = new NotifierClient(http, logger.Object);
        var req = MakeRequest();

        await client.NotifyRepeatedAsync(req, total: 3, interval: TimeSpan.FromMilliseconds(10), ct: CancellationToken.None);

        var swTimeout = TimeSpan.FromSeconds(2);
        var swStart = DateTime.UtcNow;
        while (handler.Requests.Count < 3 && DateTime.UtcNow - swStart < swTimeout)
        {
            await Task.Delay(10);
        }

        Assert.Equal(3, handler.Requests.Count);
    }
}