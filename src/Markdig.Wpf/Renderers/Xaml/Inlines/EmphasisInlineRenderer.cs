// Copyright (c) 2016-2017 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license.
// See the LICENSE.md file in the project root for more information.

using System;
using Markdig.Annotations;
using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Xaml.Inlines
{
    /// <summary>
    /// A XAML renderer for an <see cref="EmphasisInline"/>.
    /// </summary>
    /// <seealso cref="Xaml.XamlObjectRenderer{T}" />
    public class EmphasisInlineRenderer : XamlObjectRenderer<EmphasisInline>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmphasisInlineRenderer"/> class.
        /// </summary>
        public EmphasisInlineRenderer()
        {
        }

		[NotNull]
		private IDisposable BeginSpan([NotNull] XamlRenderer renderer, EmphasisInline obj)
		{
			switch (obj.DelimiterChar)
			{
				case '*':
				case '_':
					return obj.IsDouble
						? renderer.BeginBold()
						: renderer.BeginItalic();
				case '~':
					return renderer.BeginSpan(obj.IsDouble
						? "markdig:Styles.StrikeThroughStyleKey"
						: "markdig:Styles.SubscriptStyleKey");
				case '^':
					return renderer.BeginSpan("markdig:Styles.SubscriptStyleKey");
				case '+':
					return renderer.BeginSpan("markdig:Styles.InsertedStyleKey");
				case '=':
					return renderer.BeginSpan("markdig:Styles.MarkedStyleKey");
				default:
					return renderer.BeginSpan();
			}
		}

		protected override void Write([NotNull] XamlRenderer renderer, EmphasisInline obj)
		{
			using (BeginSpan(renderer, obj))
			{
				renderer.WriteChildren(obj);
			}
		}
    }
}