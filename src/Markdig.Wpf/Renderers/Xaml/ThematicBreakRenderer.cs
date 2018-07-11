// Copyright (c) 2016-2017 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license.
// See the LICENSE.md file in the project root for more information.

using System.Windows.Documents;
using Markdig.Annotations;
using Markdig.Syntax;

namespace Markdig.Renderers.Xaml
{
    /// <summary>
    /// A XAML renderer for a <see cref="ThematicBreakBlock"/>.
    /// </summary>
    /// <seealso cref="Xaml.XamlObjectRenderer{T}" />
    public class ThematicBreakRenderer : XamlObjectRenderer<ThematicBreakBlock>
    {
        /// <summary></summary>
        /// <param name="renderer"></param>
        /// <param name="obj"></param>
        protected override void Write([NotNull] XamlRenderer renderer, ThematicBreakBlock obj)
        {
            renderer.WriteStartObject(typeof(Paragraph));
            renderer.WriteStaticResourceMember(null, "markdig:Styles.ThematicBreakStyleKey");
            renderer.WriteEndObject();

            //var line = new System.Windows.Shapes.Line { X2 = 1 };
            //line.SetResourceReference(FrameworkContentElement.StyleProperty, Styles.ThematicBreakStyleKey);

            //var paragraph = new Paragraph
            //{
            //    Inlines = { new InlineUIContainer(line) }
            //};

            //renderer.WriteBlock(paragraph);
        }
    }
}
