// Copyright (c) 2016-2017 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license.
// See the LICENSE.md file in the project root for more information.

using System.Windows.Documents;
using Markdig.Annotations;
using Markdig.Syntax;

namespace Markdig.Renderers.Xaml
{
    /// <summary>
    /// A XAML renderer for a <see cref="QuoteBlock"/>.
    /// </summary>
    /// <seealso cref="Xaml.XamlObjectRenderer{T}" />
    public class QuoteBlockRenderer : XamlObjectRenderer<QuoteBlock>
    {
        /// <summary></summary>
        /// <param name="renderer"></param>
        /// <param name="block"></param>
        protected override void Write([NotNull] XamlRenderer renderer, QuoteBlock block)
        {
            renderer.WriteStartObject(typeof(Section));
            renderer.WriteStaticResourceMember(null, "markdig:Styles.QuoteBlockStyleKey");

            renderer.WriteItems(block);

            renderer.WriteEndObject();
        }
    }
}
