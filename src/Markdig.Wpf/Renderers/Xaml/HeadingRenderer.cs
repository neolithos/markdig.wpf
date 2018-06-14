// Copyright (c) 2016-2017 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license.
// See the LICENSE.md file in the project root for more information.

using System.Windows;
using System.Windows.Documents;
using Markdig.Annotations;
using Markdig.Syntax;

namespace Markdig.Renderers.Xaml
{
    /// <summary>
    /// An XAML renderer for a <see cref="HeadingBlock"/>.
    /// </summary>
    /// <seealso cref="Xaml.XamlObjectRenderer{T}" />
    public class HeadingRenderer : XamlObjectRenderer<HeadingBlock>
    {
        private static string GetStyleKey(int level)
        {
            switch (level)
            {
                case 1:
                    return "markdig:Styles.Heading1StyleKey";
                case 2:
                    return "markdig:Styles.Heading2StyleKey";
                case 3:
                    return "markdig:Styles.Heading3StyleKey";
                case 4:
                    return "markdig:Styles.Heading4StyleKey";
                case 5:
                    return "markdig:Styles.Heading5StyleKey";
                default:
                    return "markdig:Styles.Heading6StyleKey";
            }
        }

        /// <summary></summary>
        /// <param name="renderer"></param>
        /// <param name="headingBlock"></param>
        protected override void Write([NotNull] XamlRenderer renderer, [NotNull] HeadingBlock headingBlock)
        {
            renderer.WriteStartObject(typeof(Paragraph));
            renderer.WriteStaticResourceMember(null, GetStyleKey(headingBlock.Level));
            renderer.WriteItems(headingBlock);
            renderer.WriteEndObject();
        }
    }
}
