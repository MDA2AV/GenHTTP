﻿namespace GenHTTP.Modules.Security.Providers;

public sealed class StrictTransportPolicy
{

    #region Initialization

    public StrictTransportPolicy(TimeSpan maximumAge, bool includeSubdomains, bool preload)
    {
        MaximumAge = maximumAge;
        IncludeSubdomains = includeSubdomains;
        Preload = preload;
    }

    #endregion

    #region Get-/Setters

    public TimeSpan MaximumAge { get; }

    public bool IncludeSubdomains { get; }

    public bool Preload { get; }

    #endregion

}
