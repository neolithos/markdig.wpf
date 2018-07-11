// Copyright (c) 2016-2017 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license.
// See the LICENSE.md file in the project root for more information.

using System.Reflection;
using System.Windows;

namespace Markdig.Wpf
{
    /// <summary>
    /// List of supported styles.
    /// </summary>
    public static class Styles
    {
        private sealed class StyleResourceKey : ResourceKey
        {
            public StyleResourceKey(string key)
                => Key = key;

            public override bool Equals(object obj)
                => obj is StyleResourceKey srk ? Key.Equals(srk.Key) : base.Equals(obj);

            public override int GetHashCode()
                => Assembly.GetHashCode() ^ Key.GetHashCode();

            public string Key { get; }
            public override Assembly Assembly => typeof(StyleResourceKey).Assembly;
        }

        /// <summary>
        /// Resource Key for the CodeStyle.
        /// </summary>
        public static ResourceKey CodeStyleKey { get; } = new StyleResourceKey(nameof(CodeStyleKey));

        /// <summary>
        /// Resource Key for the CodeBlockStyle.
        /// </summary>
        public static ResourceKey CodeBlockStyleKey { get; } = new StyleResourceKey(nameof(CodeBlockStyleKey));

        /// <summary>
        /// Resource Key for the DocumentStyle.
        /// </summary>
        public static ResourceKey DocumentStyleKey { get; } = new StyleResourceKey(nameof(DocumentStyleKey));

        /// <summary>
        /// Resource Key for the Heading1Style.
        /// </summary>
        public static ResourceKey Heading1StyleKey { get; } = new StyleResourceKey(nameof(Heading1StyleKey));

        /// <summary>
        /// Resource Key for the Heading2Style.
        /// </summary>
        public static ResourceKey Heading2StyleKey { get; } = new StyleResourceKey(nameof(Heading2StyleKey));

        /// <summary>
        /// Resource Key for the Heading3Style.
        /// </summary>
        public static ResourceKey Heading3StyleKey { get; } = new StyleResourceKey(nameof(Heading3StyleKey));

        /// <summary>
        /// Resource Key for the Heading4Style.
        /// </summary>
        public static ResourceKey Heading4StyleKey { get; } = new StyleResourceKey(nameof(Heading4StyleKey));

        /// <summary>
        /// Resource Key for the Heading5Style.
        /// </summary>
        public static ResourceKey Heading5StyleKey { get; } = new StyleResourceKey(nameof(Heading5StyleKey));

        /// <summary>
        /// Resource Key for the Heading6Style.
        /// </summary>
        public static ResourceKey Heading6StyleKey { get; } = new StyleResourceKey(nameof(Heading6StyleKey));

        /// <summary>
        /// Resource Key for the ImageStyle.
        /// </summary>
        public static ResourceKey ImageStyleKey { get; } = new StyleResourceKey(nameof(ImageStyleKey));

        /// <summary>
        /// Resource Key for the InsertedStyle.
        /// </summary>
        public static ResourceKey InsertedStyleKey { get; } = new StyleResourceKey(nameof(InsertedStyleKey));

        /// <summary>
        /// Resource Key for the MarkedStyle.
        /// </summary>
        public static ResourceKey MarkedStyleKey { get; } = new StyleResourceKey(nameof(MarkedStyleKey));

        /// <summary>
        /// Resource Key for the QuoteBlockStyle.
        /// </summary>
        public static ResourceKey QuoteBlockStyleKey { get; } = new StyleResourceKey(nameof(QuoteBlockStyleKey));

        /// <summary>
        /// Resource Key for the StrikeThroughStyle.
        /// </summary>
        public static ResourceKey StrikeThroughStyleKey { get; } = new StyleResourceKey(nameof(StrikeThroughStyleKey));
        /// <summary>
        /// Resource Key for the SubscriptStyle.
        /// </summary>
        public static ResourceKey SubscriptStyleKey { get; } = new StyleResourceKey(nameof(SubscriptStyleKey));

        /// <summary>
        /// Resource Key for the SuperscriptStyle.
        /// </summary>
        public static ResourceKey SuperscriptStyleKey { get; } = new StyleResourceKey(nameof(SuperscriptStyleKey));

        /// <summary>
        /// Resource Key for the TableStyle.
        /// </summary>
        public static ResourceKey TableStyleKey { get; } = new StyleResourceKey(nameof(TableStyleKey));

        /// <summary>
        /// Resource Key for the TableCellStyle.
        /// </summary>
        public static ResourceKey TableCellStyleKey { get; } = new StyleResourceKey(nameof(TableCellStyleKey));

        /// <summary>
        /// Resource Key for the TableHeaderStyle.
        /// </summary>
        public static ResourceKey TableHeaderStyleKey { get; } = new StyleResourceKey(nameof(TableHeaderStyleKey));

        /// <summary>
        /// Resource Key for the TaskListStyle.
        /// </summary>
        public static ResourceKey TaskListStyleKey { get; } = new StyleResourceKey(nameof(TaskListStyleKey));

        /// <summary>
        /// Resource Key for the ThematicBreakStyle.
        /// </summary>
        public static ResourceKey ThematicBreakStyleKey { get; } = new StyleResourceKey(nameof(ThematicBreakStyleKey));

        /// <summary>
        /// 
        /// </summary>
        public static ResourceKey HyperlinkStyleKey { get; } = new StyleResourceKey(nameof(HyperlinkStyleKey));
    }
}
