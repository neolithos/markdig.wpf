// Copyright (c) 2016-2017 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license.
// See the LICENSE.md file in the project root for more information.

using System.Windows.Documents;
using Markdig.Annotations;
using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Xaml.Inlines
{
    /// <summary>
    /// A XAML renderer for a <see cref="CodeInline"/>.
    /// </summary>
    /// <seealso cref="Xaml.XamlObjectRenderer{T}" />
    public class CodeInlineRenderer : XamlObjectRenderer<CodeInline>
    {
        /// <summary></summary>
        /// <param name="renderer"></param>
        /// <param name="code"></param>
        protected override void Write([NotNull] XamlRenderer renderer, [NotNull] CodeInline code)
        {
            renderer.WriteStartObject(typeof(Span));
            renderer.WriteStaticResourceMember(null, "markdig:Styles.CodeStyleKey");

            renderer.WriteStartItems(nameof(Span.Inlines), true);
            renderer.WriteText(code.Content);
            renderer.WriteEndItems();
            renderer.WriteEndObject();
        }
    }
}
