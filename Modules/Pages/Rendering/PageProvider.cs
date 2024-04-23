﻿using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Pages.Rendering
{

    /// <summary>
    /// Abstract handler which can be used by template engine implementations
    /// to easily provide pages using a configured renderer instance.
    /// </summary>
    public abstract class PageProvider<T> : IHandler where T : class, IModel
    {
        private static readonly FlexibleContentType _TextHtmlType = new(ContentType.TextHtml, "UTF-8");

        #region Get-/Setters

        public IHandler Parent { get; }

        public ModelProvider<T> ModelProvider { get; }

        /// <summary>
        /// When implemented in an inheriting class, this property
        /// returns the actual renderer to be used to generate
        /// the page.
        /// </summary>
        public abstract IRenderer<T> Renderer { get; }

        public ContentInfo PageInfo { get; }

        public PageAdditions? Additions { get; }

        public ResponseModifications? Modifications { get; }

        #endregion

        #region Initialization

        protected PageProvider(IHandler parent, ModelProvider<T> modelProvider, ContentInfo pageInfo, PageAdditions? additions, ResponseModifications? modifications)
        {
            Parent = parent;

            ModelProvider = modelProvider;

            PageInfo = pageInfo;
            Additions = additions;  
            Modifications = modifications;
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var model = await ModelProvider(request, this);

            var content = new RenderedContent<T>(Renderer, model, PageInfo, Additions);

            await content.FillBufferAsync();

            return request.Respond()
                          .Content(content)
                          .Type(_TextHtmlType)
                          .Modify(Modifications)
                          .Build();
        }

        public ValueTask PrepareAsync() => Renderer.PrepareAsync();

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => this.GetContent(request, PageInfo, ContentType.TextHtml);

        #endregion

    }

}
