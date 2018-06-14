// Copyright (c) 2016-2017 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Windows.Documents;
using Markdig.Annotations;
using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Xaml.Inlines
{
    /// <summary>
    /// A XAML renderer for an <see cref="EmphasisInline"/>.
    /// </summary>
    /// <seealso cref="Xaml.XamlObjectRenderer{T}" />
    public class EmphasisInlineRenderer : XamlObjectRenderer<EmphasisInline>
    {
        /// <summary></summary>
        /// <param name="renderer"></param>
        /// <param name="span"></param>
		protected override void Write([NotNull] XamlRenderer renderer, EmphasisInline span)
		{
            var spanType = typeof(Span);
            
            switch (span.DelimiterChar)
            {
                case '*':
                case '_':
                    spanType = span.IsDouble ? typeof(Bold) : typeof(Italic);
                    break;
            }

            renderer.WriteStartObject(spanType);

            switch (span.DelimiterChar)
            {
                case '~':
                    if (span.IsDouble)
                        renderer.WriteStaticResourceMember(null, "markdig:Styles.StrikeThroughStyleKey");
                    else
                        goto case '^';
                    break;
                case '^':
                    renderer.WriteStaticResourceMember(null, "markdig:Styles.SuperscriptStyleKey");
                    break;
                case '+':
                    renderer.WriteStaticResourceMember(null, "markdig:Styles.InsertedStyleKey");
                    break;
                case '=':
                    renderer.WriteStaticResourceMember(null, "markdig:Styles.MarkedStyleKey");
                    break;
            }
            
            renderer.WriteItems(span);
            renderer.WriteEndObject();
        }
    }
}