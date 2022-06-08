using System.Text;
using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Resolvers;

namespace Sapient.ServiceHost.Endpoints;

public class NewRelicReportingDiagnosticEventListener : ExecutionDiagnosticEventListener
{
    private readonly ILogger<NewRelicReportingDiagnosticEventListener> _logger;

    public NewRelicReportingDiagnosticEventListener(ILogger<NewRelicReportingDiagnosticEventListener> logger)
    {
        _logger = logger;
    }
    public override void RequestError(IRequestContext context, Exception exception)
    {
        NewRelic.Api.Agent.NewRelic.NoticeError(exception);
        base.RequestError(context, exception);
    }

    public override void ResolverError(IMiddlewareContext context, IError error)
    {
        if (error.Exception != null)
        {
            NewRelic.Api.Agent.NewRelic.NoticeError(error.Exception);
            _logger.LogError(error.Exception, "Resolver error");
        }
        
        base.ResolverError(context, error);
    }

    public override IDisposable ExecuteRequest(IRequestContext context)
    {
        if (context.Request.Query == null) return EmptyScope;
        
        var querySpan = context.Request.Query.AsSpan();
        var slice = querySpan[..Math.Min(32, querySpan.Length - 1)];
        var queryStr = Encoding.UTF8.GetString(slice);
        NewRelic.Api.Agent.NewRelic.SetTransactionName("GQL", queryStr);

        return EmptyScope;
    }
}
