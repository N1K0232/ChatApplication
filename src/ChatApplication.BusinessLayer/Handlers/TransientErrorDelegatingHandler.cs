using Polly.Registry;

namespace ChatApplication.BusinessLayer.Handlers;

public class TransientErrorDelegatingHandler : DelegatingHandler
{
    private readonly ResiliencePipelineProvider<string> provider;

    public TransientErrorDelegatingHandler(ResiliencePipelineProvider<string> provider)
    {
        this.provider = provider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var pipeline = provider.GetPipeline<HttpResponseMessage>("http");
        return await pipeline.ExecuteAsync(async token => await base.SendAsync(request, token), cancellationToken);
    }
}