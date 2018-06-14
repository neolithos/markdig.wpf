// Copyright (c) 2016-2017 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Windows;
using System.Windows.Documents;
using Markdig.Annotations;
using Markdig.Syntax.Inlines;
using Markdig.Wpf;

namespace Markdig.Renderers.Xaml.Inlines
{
    /// <summary>
    /// A XAML renderer for a <see cref="AutolinkInline"/>.
    /// </summary>
    /// <seealso cref="Xaml.XamlObjectRenderer{T}" />
    public class AutolinkInlineRenderer : XamlObjectRenderer<AutolinkInline>
    {
        /// <summary></summary>
        /// <param name="renderer"></param>
        /// <param name="link"></param>
        protected override void Write([NotNull] XamlRenderer renderer, [NotNull] AutolinkInline link)
        {
            var url = link.Url;

            LinkInlineRenderer.WriteStartHyperlink(renderer, url, url);
            renderer.WriteText(url);
            LinkInlineRenderer.WriteEndHyperlink(renderer);
        }
    }
}
