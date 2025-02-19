﻿using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.LoadBalancing.Provider;

public sealed class LoadBalancerBuilder : IHandlerBuilder<LoadBalancerBuilder>
{
    private static readonly PriorityEvaluation DefaultPriority = _ => Priority.Medium;

    private readonly List<IConcernBuilder> _Concerns = [];

    private readonly List<(IHandlerBuilder, PriorityEvaluation)> _Nodes = [];

    #region Functionality

    public LoadBalancerBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public LoadBalancerBuilder Add(IHandlerBuilder handler, PriorityEvaluation? priority = null)
    {
        _Nodes.Add((handler, priority ?? DefaultPriority));
        return this;
    }

    public LoadBalancerBuilder Redirect(string node, PriorityEvaluation? priority = null) => Add(new LoadBalancerRedirectionBuilder().Root(node), priority);

    public LoadBalancerBuilder Proxy(string node, PriorityEvaluation? priority = null) => Add(ReverseProxy.Proxy.Create().Upstream(node), priority);

    public IHandler Build()
    {
        return Concerns.Chain(_Concerns,  new LoadBalancerHandler( _Nodes));
    }

    #endregion

}
