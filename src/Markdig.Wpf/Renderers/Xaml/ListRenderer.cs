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
    /// A XAML renderer for a <see cref="ListBlock"/>.
    /// </summary>
    /// <seealso cref="Xaml.XamlObjectRenderer{T}" />
    public class ListRenderer : XamlObjectRenderer<ListBlock>
    {
        /// <summary></summary>
        /// <param name="renderer"></param>
        /// <param name="listBlock"></param>
        protected override void Write([NotNull] XamlRenderer renderer, [NotNull] ListBlock listBlock)
        {
            renderer.WriteStartObject(typeof(List));

            if (listBlock.IsOrdered)
            {
                renderer.WriteMember(List.MarkerStyleProperty, TextMarkerStyle.Decimal);

                if (listBlock.OrderedStart != null && (listBlock.DefaultOrderedStart != listBlock.OrderedStart))
                    renderer.WriteMember(List.StartIndexProperty, listBlock.OrderedStart);
            }
            else
                renderer.WriteMember(List.MarkerStyleProperty, TextMarkerStyle.Disc);

            renderer.WriteStartItems(nameof(List.ListItems));

            foreach (var cur in listBlock)
            {
                renderer.WriteStartObject(typeof(ListItem));
                renderer.WriteItems((ContainerBlock)cur);
                renderer.WriteEndObject();
            }

            renderer.WriteEndItems();
            renderer.WriteEndObject();
        }
    }
}
