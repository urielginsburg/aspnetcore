﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Http.Features;
using Microsoft.Dnx.Compilation.CSharp;
using Microsoft.AspNet.Http.Features.Internal;
using Microsoft.AspNet.Http.Features.Authentication;

namespace Microsoft.AspNet.Server.Kestrel.GeneratedCode
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class FrameFeatureCollection : ICompileModule
    {
        static string Each<T>(IEnumerable<T> values, Func<T, string> formatter)
        {
            return values.Select(formatter).Aggregate((a, b) => a + b);
        }

        public virtual void BeforeCompile(BeforeCompileContext context)
        {
            var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(GeneratedFile());
            context.Compilation = context.Compilation.AddSyntaxTrees(syntaxTree);
        }

        public static string GeneratedFile()
        {
            var commonFeatures = new[]
            {
                typeof(IHttpRequestFeature),
                typeof(IHttpResponseFeature),
                typeof(IHttpRequestIdentifierFeature),
                typeof(IHttpSendFileFeature),
                typeof(IServiceProvidersFeature),
                typeof(IHttpAuthenticationFeature),
                typeof(IHttpRequestLifetimeFeature),
                typeof(IQueryFeature),
                typeof(IFormFeature),
                typeof(IResponseCookiesFeature),
                typeof(IItemsFeature),
                typeof(IHttpConnectionFeature),
                typeof(ITlsConnectionFeature),
                typeof(IHttpUpgradeFeature),
                typeof(IHttpWebSocketFeature),
                typeof(ISessionFeature),
            };

            // NOTE: This list MUST always match the set of feature interfaces implemented by Frame.
            // See also: src/Microsoft.AspNet.Server.Kestrel/Http/Frame.FeatureCollection.cs
            var implementedFeatures = new[]
            {
                typeof(IHttpRequestFeature),
                typeof(IHttpResponseFeature),
                typeof(IHttpUpgradeFeature),
            };
            
            return $@"
using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Server.Kestrel.Http 
{{
    public partial class Frame
    {{{Each(allFeatures, feature => $@"
        private static readonly Type {feature.Name}Type = typeof(global::{feature.FullName});")}
{Each(allFeatures, feature => $@"
        private object _current{feature.Name};")}

        private void FastReset()
        {{{Each(implementedFeatures, feature => $@"
            _current{feature.Name} = this;")}
            {Each(allFeatures.Where( f => !implementedFeatures.Contains(f)), feature => $@"
            _current{feature.Name} = null;")}
        }}

        private object FastFeatureGet(Type key)
        {{{Each(allFeatures, feature => $@"
            if (key == typeof(global::{feature.FullName}))
            {{
                return _current{feature.Name};
            }}")}
            return  SlowFeatureGet(key);
        }}

        private object SlowFeatureGet(Type key)
        {{
            object feature = null;
            if (MaybeExtra?.TryGetValue(key, out feature) ?? false) 
            {{
                return feature;
            }}
            return null;
        }}

        private void FastFeatureSetInner(long flag, Type key, object feature)
        {{
            Extra[key] = feature;

            // Altering only an individual bit of the long
            // so need to make sure other concurrent bit changes are not overridden
            // in an atomic yet lock-free manner

            long currentFeatureFlags;
            long updatedFeatureFlags;
            do
            {{
                currentFeatureFlags = _featureOverridenFlags;
                updatedFeatureFlags = currentFeatureFlags | flag;
            }} while (System.Threading.Interlocked.CompareExchange(ref _featureOverridenFlags, updatedFeatureFlags, currentFeatureFlags) != currentFeatureFlags);

        private void FastFeatureSet(Type key, object feature)
        {{
            _featureRevision++;
            {Each(allFeatures, feature => $@"
            if (key == typeof(global::{feature.FullName}))
            {{
                _current{feature.Name} = feature;
                return;
            }}")};
            SetExtra(key, feature);
        }}

        private IEnumerable<KeyValuePair<Type, object>> FastEnumerable()
        {{{Each(allFeatures, feature => $@"
            if (_current{feature.Name} != null)
            {{
                yield return new KeyValuePair<Type, object>({feature.Name}Type, _current{feature.Name} as global::{feature.FullName});
            }}")}
            if (MaybeExtra != null)
            {{
                foreach(var item in MaybeExtra)
                {{
                    yield return item;
                }}
            }}
        }}
    }}
}}
";
        }

        public virtual void AfterCompile(AfterCompileContext context)
        {
        }
    }
}
