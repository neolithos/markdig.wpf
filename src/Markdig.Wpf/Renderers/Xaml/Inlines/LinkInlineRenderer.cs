// Copyright (c) 2016-2017 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Markdig.Annotations;
using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Xaml.Inlines
{
    /// <summary>
    /// A XAML renderer for a <see cref="LinkInline"/>.
    /// </summary>
    /// <seealso cref="Xaml.XamlObjectRenderer{T}" />
    public class LinkInlineRenderer : XamlObjectRenderer<LinkInline>
    {
        /// <summary></summary>
        /// <param name="renderer"></param>
        /// <param name="link"></param>
        protected override void Write([NotNull] XamlRenderer renderer, [NotNull] LinkInline link)
        {
            var url = link.GetDynamicUrl != null ? link.GetDynamicUrl() ?? link.Url : link.Url;

            if (link.IsImage)
            {
                if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                    url = "#";

                renderer.WriteStartObject(typeof(Image));
                renderer.WriteStaticResourceMember(null, "markdig:Styles.ImageStyleKey");
                if (!String.IsNullOrEmpty(link.Title))
                    renderer.WriteMember(ToolTipService.ToolTipProperty, link.Title);
                renderer.WriteMember(Image.SourceProperty, new Uri(url, UriKind.RelativeOrAbsolute));
                renderer.WriteEndObject();
            }
            else
            {
                WriteStartHyperlink(renderer, url, link.Title);
                renderer.WriteItems(link);
                WriteEndHyperlink(renderer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteStartHyperlink(XamlRenderer renderer, string url, string linkTitle)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                url = "#";

            renderer.WriteStartObject(typeof(Hyperlink));
            renderer.WriteStaticResourceMember(null, "markdig:Styles.HyperlinkStyleKey");
            //renderer.WriteMember(Hyperlink.CommandProperty, Commands.Hyperlink);
            //renderer.WriteMember(Hyperlink.CommandParameterProperty, url);
            renderer.WriteMember(Hyperlink.NavigateUriProperty, new Uri(url, UriKind.RelativeOrAbsolute));
            renderer.WriteMember(FrameworkContentElement.ToolTipProperty, String.IsNullOrEmpty(linkTitle) ? url : linkTitle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteEndHyperlink(XamlRenderer renderer)
            => renderer.WriteEndObject();

    }
}
