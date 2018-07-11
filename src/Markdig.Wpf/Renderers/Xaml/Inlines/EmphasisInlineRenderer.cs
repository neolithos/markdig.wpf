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
        private static bool WriteSpan(XamlRenderer renderer, EmphasisInline span)
        {
            switch (span.DelimiterChar)
            {
                case '*': // bold
                    renderer.WriteStartObject(typeof(Bold));
                    return true;
                case '_': // italic
                    renderer.WriteStartObject(typeof(Italic));
                    return true;
                case '~': // strike through
                    renderer.WriteStartObject(typeof(Span));
                    renderer.WriteStaticResourceMember(null, "markdig:Styles.StrikeThroughStyleKey")
                    return true;
                case '^': // superscript, subscript
                    renderer.WriteStartObject(typeof(Span));
                    if (span.IsDouble)
                        renderer.WriteStaticResourceMember(null, "markdig:Styles.SuperscriptStyleKey");
                    else
                        renderer.WriteStaticResourceMember(null, "markdig:Styles.SubscriptStyleKey");
                    return true;
                case '+': // underline
                    renderer.WriteStartObject(typeof(Span));
                    renderer.WriteStaticResourceMember(null, "markdig:Styles.InsertedStyleKey");
                    return true;
                case '=': // Marked
                    renderer.WriteStartObject(typeof(Span));
                    renderer.WriteStaticResourceMember(null, "markdig:Styles.MarkedStyleKey");
                    return true;
                default:
                    return false;
            }
        } // proc WriteSpan

        /// <summary></summary>
        /// <param name="renderer"></param>
        /// <param name="span"></param>
		protected override void Write([NotNull] XamlRenderer renderer, EmphasisInline span)
		{
            if (WriteSpan(renderer, span))
            {
                renderer.WriteItems(span);
                renderer.WriteEndObject();
            }
            else
                renderer.WriteChildren(span);
        }
    }
}