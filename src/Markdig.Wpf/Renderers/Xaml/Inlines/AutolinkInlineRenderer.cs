// Copyright (c) 2016-2017 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license.
// See the LICENSE.md file in the project root for more information.

using Markdig.Annotations;
using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Xaml.Inlines
{
    /// <summary>
    /// A XAML renderer for a <see cref="AutolinkInline"/>.
    /// </summary>
    /// <seealso cref="Xaml.XamlObjectRenderer{T}" />
    public class AutolinkInlineRenderer : XamlObjectRenderer<AutolinkInline>
    {
        protected override void Write([NotNull] XamlRenderer renderer, [NotNull] AutolinkInline obj)
        {
			using (renderer.BeginHyperlink(obj.Url))
				renderer.WriteText(obj.Url);
        }
    }
}
