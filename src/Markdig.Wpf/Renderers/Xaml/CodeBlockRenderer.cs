// Copyright (c) 2016-2017 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license.
// See the LICENSE.md file in the project root for more information.

using Markdig.Annotations;
using Markdig.Syntax;

namespace Markdig.Renderers.Xaml
{
    /// <summary>
    /// A XAML renderer for a <see cref="CodeBlock"/>.
    /// </summary>
    /// <seealso cref="Xaml.XamlObjectRenderer{T}" />
    public class CodeBlockRenderer : XamlObjectRenderer<CodeBlock>
    {
        protected override void Write([NotNull] XamlRenderer renderer, [NotNull] CodeBlock obj)
        {
			//renderer.Write("<Paragraph xml:space=\"preserve\"");
			using (renderer.BeginParagraph("markdig:Styles.CodeBlockStyleKey"))
				renderer.WriteRawLines(obj, true);
        }
    }
}
