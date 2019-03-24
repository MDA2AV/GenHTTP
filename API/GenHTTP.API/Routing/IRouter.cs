﻿using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Pages;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Routing
{

    public interface IRouter
    {

        IRouter Parent { get; set; }

        void HandleContext(IEditableRoutingContext current);

        IContentProvider GetErrorHandler(IHttpRequest request, IHttpResponse response);

        IContentPage GetPage(IHttpRequest request, IHttpResponse response);

    }

}