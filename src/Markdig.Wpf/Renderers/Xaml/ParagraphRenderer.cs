// Copyright (c) 2016-2017 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license.
// See the LICENSE.md file in the project root for more information.

using System.Windows.Documents;
using Markdig.Annotations;
using Markdig.Syntax;

namespace Markdig.Renderers.Xaml
{
    /// <summary>
    /// A XAML renderer for a <see cref="ParagraphBlock"/>.
    /// </summary>
    /// <seealso cref="Xaml.XamlObjectRenderer{T}" />
    public class ParagraphRenderer : XamlObjectRenderer<ParagraphBlock>
    {
        /// <summary></summary>
        /// <param name="renderer"></param>
        /// <param name="paragraph"></param>
        protected override void Write([NotNull] XamlRenderer renderer, ParagraphBlock paragraph)
        {
            renderer.WriteStartObject(typeof(Paragraph));
            renderer.WriteItems(paragraph);
            renderer.WriteEndObject();
        }
    }
}
