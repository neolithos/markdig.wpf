// Copyright (c) 2016-2017 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license.
// See the LICENSE.md file in the project root for more information.

using System.Windows.Controls;
using System.Xaml;
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
		private static readonly XamlType imageType;
			private static readonly XamlMember styleMember;
			private static readonly XamlMember sourceMember;

		static LinkInlineRenderer()
		{
			imageType = XamlRenderer.todo.GetXamlType(typeof(Image));
			styleMember = imageType.GetMember(nameof(Image.Style)); // FrameworkElement.Style
			sourceMember = imageType.GetMember(nameof(Image.Source));
		}

        protected override void Write([NotNull] XamlRenderer renderer, [NotNull] LinkInline obj)
        {
			var url = obj.GetDynamicUrl?.Invoke() ?? obj.Url;
			if (obj.IsImage)
			{
				using (renderer.BeginObject(imageType))
				{
					renderer.WriteStaticResourceMember(styleMember, "markdig:Styles.ImageStyleKey");
					renderer.WriteMember(sourceMember, url);
				}
			}
			else
			{
				using (renderer.BeginHyperlink(url))
					renderer.WriteChildren(obj);
			}
        }
    }
}
