﻿using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Security.Providers;

public sealed class SecureUpgradeConcern : IConcern
{

    #region Get-/Setters

    public SecureUpgrade Mode { get; }

    public IHandler Content { get; }

    #endregion

    #region Initialization

    public SecureUpgradeConcern(IHandler content, SecureUpgrade mode)
    {
        Content = content;

        Mode = mode;
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (!request.EndPoint.Secure)
        {
            if (request.Server.EndPoints.Any(e => e.Secure))
            {
                var endpoints = request.Server.EndPoints.Where(e => e.Secure)
                                       .ToList();

                if (endpoints.Count > 0)
                {
                    if (Mode == SecureUpgrade.Force)
                    {
                        return await Redirect.To(GetRedirectLocation(request, endpoints))
                                             .Build()
                                             .HandleAsync(request)
                            ;
                    }
                    if (Mode == SecureUpgrade.Allow)
                    {
                        if (request.Method.KnownMethod == RequestMethod.Get)
                        {
                            if (request.Headers.TryGetValue("Upgrade-Insecure-Requests", out var flag))
                            {
                                if (flag == "1")
                                {
                                    var response = await Redirect.To(GetRedirectLocation(request, endpoints), true)
                                                                 .Build()
                                                                 .HandleAsync(request)
                                        ;

                                    response?.Headers.Add("Vary", "Upgrade-Insecure-Requests");

                                    return response;
                                }
                            }
                        }
                    }
                }
            }
        }

        return await Content.HandleAsync(request);
    }

    private static string GetRedirectLocation(IRequest request, List<IEndPoint> endPoints)
    {
        var targetPort = GetTargetPort(request, endPoints);

        var port = targetPort == 443 ? string.Empty : $":{targetPort}";

        return $"https://{request.HostWithoutPort()}{port}{request.Target.Path}";
    }

    private static ushort GetTargetPort(IRequest request, List<IEndPoint> endPoints)
    {
        // this extension can only be added if there are secure endpoints available
        if (endPoints.Count == 0)
        {
            throw new NotSupportedException("No secure endpoints available");
        }

        // if there is a correlated port, use this one
        var correlated = (ushort)(request.EndPoint.Port + (443 - 80));

        if (endPoints.Any(e => e.Port == correlated))
        {
            return correlated;
        }

        // default to 443, if available
        if (endPoints.Any(e => e.Port == 443))
        {
            return 443;
        }

        // use the first secure endpoint
        return endPoints.First().Port;
    }

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    #endregion

}
