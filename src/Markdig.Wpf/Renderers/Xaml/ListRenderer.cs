// Copyright (c) 2016-2017 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license.
// See the LICENSE.md file in the project root for more information.

using System.Windows;
using System.Windows.Documents;
using System.Xaml;
using Markdig.Annotations;
using Markdig.Syntax;

namespace Markdig.Renderers.Xaml
{
    /// <summary>
    /// A XAML renderer for a <see cref="ListBlock"/>.
    /// </summary>
    /// <seealso cref="Xaml.XamlObjectRenderer{T}" />
    public class ListRenderer : XamlObjectRenderer<ListBlock>
    {
		private readonly static XamlType listType;
		private readonly static XamlType listItemType;
		private readonly static XamlMember markStyleMember;
		private readonly static XamlMember startIndexMember;
		private readonly static XamlMember listItemsMember;
		private readonly static XamlMember blocksMember;

		static ListRenderer()
		{
			listType = XamlRenderer.todo.GetXamlType(typeof(List));
			markStyleMember = listType.GetMember(nameof(List.MarkerStyle));
			startIndexMember = listType.GetMember(nameof(List.StartIndex));
			listItemsMember = listType.GetMember(nameof(List.ListItems));

			listItemType = XamlRenderer.todo.GetXamlType(typeof(ListItem));
			blocksMember = listItemType.GetMember(nameof(ListItem.Blocks));
		}

        protected override void Write([NotNull] XamlRenderer renderer, [NotNull] ListBlock listBlock)
        {
			using (renderer.BeginObject(listType))
			{
				if (listBlock.IsOrdered)
				{
					renderer.WriteMember(markStyleMember, TextMarkerStyle.Decimal.ToString());

					if (listBlock.OrderedStart != null && (listBlock.DefaultOrderedStart != listBlock.OrderedStart))
						renderer.WriteMember(startIndexMember, listBlock.OrderedStart);
				}
				else
				{
					renderer.WriteMember(markStyleMember, TextMarkerStyle.Disc.ToString());
				}

				using (renderer.BeginAddChilds(listItemsMember))
				{
					foreach (var item in listBlock)
					{
						var listItem = (ListItemBlock)item;

						using (renderer.BeginObject(listItemType))
						using (renderer.BeginAddChilds(blocksMember))
							renderer.WriteChildren(listItem);
					}
				}
			}
		
        }
    }
}
