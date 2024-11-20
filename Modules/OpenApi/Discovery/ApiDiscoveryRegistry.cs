﻿using GenHTTP.Api.Content;
using NSwag;

namespace GenHTTP.Modules.OpenApi.Discovery;

public sealed class ApiDiscoveryRegistry
{

    #region Get-/Setters

    private List<IApiExplorer> Explorers { get; }

    #endregion

    #region Initialization

    public ApiDiscoveryRegistry(List<IApiExplorer> explorers)
    {
        Explorers = explorers;
    }

    #endregion

    #region Functionality

    /// <summary>
    /// Iterates through the registered explorers to find a responsible one to analyze
    /// the given handler instance.
    /// </summary>
    /// <param name="handler">The handler to get analyzed</param>
    /// <param name="path">The current stack of path segments that have already been analyzed, relative to the location of the OpenAPI concern</param>
    /// <param name="document">The document to be adjusted and enriched</param>
    /// <param name="schemata">The manager to generate JSON schemas with</param>
    public void Explore(IHandler handler, List<string> path, OpenApiDocument document, SchemaManager schemata)
    {
        foreach (var explorer in Explorers)
        {
            if (explorer.CanExplore(handler))
            {
                explorer.Explore(handler, path, document, schemata, this);
                break;
            }
        }
    }

    #endregion

}