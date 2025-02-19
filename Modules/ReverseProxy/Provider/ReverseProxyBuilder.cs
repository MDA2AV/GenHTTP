﻿using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.ReverseProxy.Provider;

public sealed class ReverseProxyBuilder : IHandlerBuilder<ReverseProxyBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private TimeSpan _ConnectTimeout = TimeSpan.FromSeconds(10);
    private TimeSpan _ReadTimeout = TimeSpan.FromSeconds(60);

    private string? _Upstream;

    #region Functionality

    public ReverseProxyBuilder Upstream(string upstream)
    {
        _Upstream = upstream;

        if (_Upstream.EndsWith('/'))
        {
            _Upstream = _Upstream[..^1];
        }

        return this;
    }

    public ReverseProxyBuilder ConnectTimeout(TimeSpan connectTimeout)
    {
        _ConnectTimeout = connectTimeout;
        return this;
    }

    public ReverseProxyBuilder ReadTimeout(TimeSpan readTimeout)
    {
        _ReadTimeout = readTimeout;
        return this;
    }

    public ReverseProxyBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        if (_Upstream is null)
        {
            throw new BuilderMissingPropertyException("Upstream");
        }

        return Concerns.Chain(_Concerns,  new ReverseProxyProvider( _Upstream, _ConnectTimeout, _ReadTimeout));
    }

    #endregion

}
