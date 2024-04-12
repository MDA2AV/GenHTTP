﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Providers;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.Controllers.Provider
{

    public sealed class ControllerHandler : IHandler, IHandlerResolver
    {
        private static readonly MethodRouting EMPTY = new("/", "^(/|)$", null, true);

        #region Get-/Setters

        public IHandler Parent { get; }

        private MethodCollection Provider { get; }

        private ResponseProvider ResponseProvider { get; }

        private FormatterRegistry Formatting { get; }

        private object Instance { get; }

        #endregion

        #region Initialization

        public ControllerHandler(IHandler parent, object instance, SerializationRegistry serialization, InjectionRegistry injection, FormatterRegistry formatting)
        {
            Parent = parent;
            Formatting = formatting;

            Instance = instance;

            ResponseProvider = new(serialization, formatting);

            Provider = new(this, AnalyzeMethods(instance.GetType(), serialization, injection, formatting));
        }

        private IEnumerable<Func<IHandler, MethodHandler>> AnalyzeMethods(Type type, SerializationRegistry serialization, InjectionRegistry injection, FormatterRegistry formatting)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var annotation = method.GetCustomAttribute<ControllerActionAttribute>(true) ?? new MethodAttribute();

                var arguments = FindPathArguments(method);

                var path = DeterminePath(method, arguments);

                yield return (parent) => new MethodHandler(parent, method, path, () => Instance, annotation, ResponseProvider.GetResponseAsync, serialization, injection, formatting);
            }
        }

        private static MethodRouting DeterminePath(MethodInfo method, List<string> arguments)
        {
            var pathArgs = string.Join('/', arguments.Select(a => a.ToParameter()));
            var rawArgs = string.Join('/', arguments.Select(a => $":{a}"));

            if (method.Name == "Index")
            {
                return pathArgs.Length > 0 ? new MethodRouting($"/{rawArgs}/", $"^/{pathArgs}/", null, false) : EMPTY;
            }
            else
            {
                var name = HypenCase(method.Name);

                var path = $"^/{name}";

                return pathArgs.Length > 0 ? new MethodRouting($"/{name}/{rawArgs}/", $"{path}/{pathArgs}/", name, false) : new MethodRouting($"/{name}", $"{path}/", name, false);
            }
        }

        private List<string> FindPathArguments(MethodInfo method)
        {
            var found = new List<string>();

            var parameters = method.GetParameters();

            foreach (var parameter in parameters)
            {
                if (parameter.GetCustomAttribute(typeof(FromPathAttribute), true) is not null)
                {
                    if (!parameter.CanFormat(Formatting))
                    {
                        throw new InvalidOperationException("Parameters marked as 'FromPath' must be formattable (e.g. string or int)");
                    }

                    if (parameter.CheckNullable())
                    {
                        throw new InvalidOperationException("Parameters marked as 'FromPath' are not allowed to be nullable");
                    }

                    if (parameter.Name is null)
                    {
                        throw new InvalidOperationException("Parameters marked as 'FromPath' must have a name");
                    }

                    found.Add(parameter.Name);
                }
            }

            return found;
        }

        private static string HypenCase(string input)
        {
            return Regex.Replace(input, @"([a-z])([A-Z0-9]+)", "$1-$2").ToLowerInvariant();
        }

        #endregion

        #region Functionality
        
        public ValueTask PrepareAsync() => Provider.PrepareAsync();

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Provider.GetContentAsync(request);

        public ValueTask<IResponse?> HandleAsync(IRequest request) => Provider.HandleAsync(request);

        public IHandler? Find(string segment)
        {
            if (segment == "{controller}")
            {
                return this;
            }

            if (segment == "{index}")
            {
                return Provider.Methods.FirstOrDefault(m => m.Method.Name == "Index" && m.Configuration.SupportedMethods.Contains(FlexibleRequestMethod.Get(RequestMethod.GET)));
            }

            return null;
        }


        #endregion

    }

}
