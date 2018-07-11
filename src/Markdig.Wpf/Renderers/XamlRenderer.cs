// Copyright (c) 2016-2017 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license.
// See the LICENSE.md file in the project root for more information.

// Some parts taken from https://github.com/lunet-io/markdig
// Copyright (c) 2016 Alexandre Mutel. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Xaml;
using Markdig.Annotations;
using Markdig.Helpers;
using Markdig.Renderers.Xaml;
using Markdig.Renderers.Xaml.Extensions;
using Markdig.Renderers.Xaml.Inlines;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Renderers
{
    /// <summary>
    /// XAML renderer for a Markdown <see cref="MarkdownDocument"/> object.
    /// </summary>
    public class XamlRenderer : RendererBase
	{
        private readonly XamlWriter writer;
        private readonly Stack<XamlType> xamlTypes = new Stack<XamlType>();

        private readonly XamlType runType;
        private readonly XamlMember runTextMember;

        private XamlMember currentContentMember = null; // start a _Items access before the next object

        private bool preserveWhitespace = false; // preserve current whitespaces
        private bool appendWhiteSpace = false;
        private bool firstCharOfBlock = true;
        private readonly StringBuilder textBuffer = new StringBuilder(); // current text buffer to collect all words

        /// <summary>
        /// Initializes a new instance of the <see cref="XamlRenderer"/> class.
        /// </summary>
        /// <param name="writer">Target for the xaml content.</param>
        public XamlRenderer(XamlWriter writer)
        {
            this.writer = writer ?? throw new ArgumentNullException(nameof(writer));

            this.runType = SchemaContext.GetXamlType(typeof(Run)) ?? throw new ArgumentNullException(nameof(Run));
            this.runTextMember = runType.GetMember(nameof(Run.Text))??throw new ArgumentNullException(nameof(Run.Text));

            // Default block renderers
            ObjectRenderers.Add(new CodeBlockRenderer());
            ObjectRenderers.Add(new ListRenderer());
            ObjectRenderers.Add(new HeadingRenderer());
            ObjectRenderers.Add(new ParagraphRenderer());
            ObjectRenderers.Add(new QuoteBlockRenderer());
            ObjectRenderers.Add(new ThematicBreakRenderer());

            // Default inline renderers
            ObjectRenderers.Add(new AutolinkInlineRenderer());
            ObjectRenderers.Add(new CodeInlineRenderer());
            ObjectRenderers.Add(new DelimiterInlineRenderer());
            ObjectRenderers.Add(new EmphasisInlineRenderer());
            ObjectRenderers.Add(new LineBreakInlineRenderer());
            ObjectRenderers.Add(new LinkInlineRenderer());
            ObjectRenderers.Add(new LiteralInlineRenderer());

            // Extension renderers
            ObjectRenderers.Add(new TableRenderer());
            ObjectRenderers.Add(new TaskListRenderer());
        }

        #region -- Primitives ---------------------------------------------------------

        /// <summary>Start to write a xaml-object.</summary>
        /// <param name="type"></param>
        public XamlType WriteStartObject([NotNull] Type type)
        {
            var xamlType = SchemaContext.GetXamlType(type ?? throw new ArgumentNullException(nameof(type)))
                ?? throw new ArgumentOutOfRangeException(nameof(type), type, "Could not resolve xaml type.");

            return WriteStartObject(xamlType);
        }

        private XamlType WriteStartObject(XamlType xamlType)
        {
            xamlTypes.Push(xamlType);

            // write pending elements
            WritePendingStartItems();
            WritePendingText(true);

            writer.WriteStartObject(xamlType);

            return xamlType;
        }

        /// <summary>Closes the current object.</summary>
        /// <returns>Current evaluated object or null.</returns>
        public object WriteEndObject()
        {
            // write pending text
            WritePendingText(false);

            xamlTypes.Pop();
            writer.WriteEndObject();
            return GetResult();
        }

        private object GetResult()
            => writer is XamlObjectWriter ow
                ? ow.Result
                : null;

        /// <summary>Get a xaml member to a member string.</summary>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public XamlMember GetMember([NotNull] string memberName)
        {
            var xamlType = xamlTypes.Peek();
            return xamlType.GetMember(memberName ?? throw new ArgumentNullException(nameof(memberName)));
        }

        /// <summary>Get a xaml member to a dependency property.</summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public XamlMember GetMember([NotNull] DependencyProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var xamlType = xamlTypes.Peek();
            if (property.OwnerType.IsAssignableFrom(xamlType.UnderlyingType))
                return xamlType.GetMember(property.Name);
            else
            {
                var type = SchemaContext.GetXamlType(property.OwnerType);
                return type.GetAttachableMember(property.Name);
            }
        }

        /// <summary>Start a member.</summary>
        /// <param name="memberName"></param>
        public void WriteStartMember([NotNull] string memberName)
            => WriteStartMember(GetMember(memberName));

        /// <summary>Start a member.</summary>
        /// <param name="property"></param>
        public void WriteStartMember([NotNull] DependencyProperty property)
            => WriteStartMember(GetMember(property));

        /// <summary>Start a member.</summary>
        /// <param name="member"></param>
        public void WriteStartMember([NotNull] XamlMember member)
            => writer.WriteStartMember(member ?? throw new ArgumentNullException(nameof(member)));

        /// <summary>End the current member.</summary>
        public void WriteEndMember()
        {
            WritePendingText(false);
            writer.WriteEndMember();
        }

        /// <summary>Start a items collection.</summary>
        /// <param name="memberName"></param>
        /// <param name="preserveSpaces"></param>
        public void WriteStartItems(string memberName, bool preserveSpaces = false)
            => WriteStartItems(GetMember(memberName), preserveSpaces);

        /// <summary>Start a items collection.</summary>
        /// <param name="property"></param>
        /// <param name="preserveSpaces"></param>
        public void WriteStartItems(DependencyProperty property, bool preserveSpaces = false)
            => WriteStartItems(GetMember(property), preserveSpaces);

        /// <summary>Start a items collection.</summary>
        /// <param name="member"></param>
        /// <param name="preserveSpaces"></param>
        public void WriteStartItems(XamlMember member, bool preserveSpaces = false)
        {
            if (currentContentMember != null)
                throw new InvalidOperationException();

            preserveWhitespace = preserveSpaces;
            appendWhiteSpace = false;
            firstCharOfBlock = true;
            currentContentMember = member;
        }

        private void WritePendingStartItems()
        {
            if (currentContentMember != null)
            {
                WriteStartMember(currentContentMember);
                writer.WriteGetObject();
                writer.WriteStartMember(XamlLanguage.Items);

                currentContentMember = null;
            }
        }

        /// <summary>End a items collection.</summary>
        public void WriteEndItems()
        {
            WritePendingText(false);

            if (currentContentMember == null)
            {
                writer.WriteEndMember();
                writer.WriteEndObject();
                writer.WriteEndMember();
            }
            else
                currentContentMember = null;
        }

        /// <summary>Write a complete member.</summary>
        /// <param name="memberName"></param>
        /// <param name="value"></param>
        public void WriteMember([NotNull] string memberName, object value)
        {
            if (value != null)
                WriteMember(GetMember(memberName), value);
        }

        /// <summary>Write a complete member.</summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public void WriteMember([NotNull] DependencyProperty property, object value)
        {
            if (value != null)
                WriteMember(GetMember(property), value);
        }

        /// <summary>Write a complete member.</summary>
        /// <param name="member"></param>
        /// <param name="value"></param>
        public void WriteMember([NotNull] XamlMember member, object value)
        {
            if (value == null)
                return;
            if (IsPendingText)
                throw new InvalidOperationException("Start member during text collection.");

            writer.WriteStartMember(member);
            if (writer is XamlObjectWriter)
                writer.WriteValue(value);
            else
            {
                if (!(value is string str))
                    str = member.TypeConverter.ConverterInstance.ConvertToString(value);

                if (str != null)
                    writer.WriteValue(str);
            }
            writer.WriteEndMember();
        }

        /// <summary>Write Inline LineBreak</summary>
        public void WriteLineBreak()
        {
            WriteStartObject(typeof(LineBreak));
            WriteEndObject();
        }

        /// <summary>Write value</summary>
        /// <param name="value"></param>
        public void WriteValue(string value)
            => WriteText(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AppendChar(char c)
        {
            if (Char.IsWhiteSpace(c))
                appendWhiteSpace = true;
            else
            {
                if (appendWhiteSpace)
                {
                    if (!firstCharOfBlock)
                        textBuffer.Append(' ');
                    appendWhiteSpace = false;
                }

                firstCharOfBlock = false;
                textBuffer.Append(c);
            }
        }

        /// <summary>Write normal text.</summary>
        /// <param name="slice"></param>
        public void WriteText(ref StringSlice slice)
        {
            if (slice.Start > slice.End)
                return;

            if (preserveWhitespace)
                textBuffer.Append(slice.Text, slice.Start, slice.Length);
            else
            {
                for (var i = slice.Start; i <= slice.End; i++)
                {
                    var c = slice[i];
                    AppendChar(c);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void WriteText([CanBeNull] string text)
        {
            if (preserveWhitespace)
                textBuffer.Append(text);
            else
            {
                var l = text.Length;
                for (var i = 0; i < l; i++)
                    AppendChar(text[i]);
            }
        }

        private void WritePendingText(bool onStartObject)
        {
            if (IsPendingText)
            {
                WritePendingStartItems();

                if (preserveWhitespace)
                {
                    var t = textBuffer.ToString();
                    writer.WriteStartObject(runType);
                    writer.WriteStartMember(runTextMember);
                    writer.WriteValue(textBuffer.ToString());
                    writer.WriteEndMember();
                    writer.WriteEndObject();
                    textBuffer.Length = 0;
                }
                else
                {
                    if (appendWhiteSpace && onStartObject)
                    {
                        textBuffer.Append(' ');
                        appendWhiteSpace = false;
                    }

                    writer.WriteValue(textBuffer.ToString());
                    textBuffer.Length = 0;
                }
            }
        }

        /// <summary></summary>
        /// <param name="leafBlock"></param>
        /// <param name="preserveSpaces"></param>
        public void WriteItems([NotNull] LeafBlock leafBlock, bool preserveSpaces = false)
        {
            if (leafBlock == null)
                throw new ArgumentNullException(nameof(leafBlock));

            var member = xamlTypes.Peek().ContentProperty ?? throw new ArgumentNullException(nameof(XamlType.ContentProperty));
            WriteStartItems(member, preserveSpaces);

            if (leafBlock.Inline != null)
            {
                WriteChildren(leafBlock.Inline);
            }
            else
            {
                var lineCount = leafBlock.Lines.Count;
                var first = true;
                for (var i = 0; i < lineCount; i++)
                {
                    if (first)
                        first = false;
                    else if (preserveSpaces)
                        WriteLineBreak();
                    else
                        AppendChar(' ');
                    
                    WriteText(ref leafBlock.Lines.Lines[i].Slice);
                }
            }

            WriteEndItems();
        }

        /// <summary></summary>
        /// <param name="inlines"></param>
        /// <param name="preserveSpaces"></param>
        public void WriteItems([NotNull] ContainerInline inlines, bool preserveSpaces = false)
        {
            if (inlines == null)
                throw new ArgumentNullException(nameof(inlines));

            var member = xamlTypes.Peek().ContentProperty ?? throw new ArgumentNullException(nameof(XamlType.ContentProperty));
            WriteStartItems(member, preserveSpaces);
            WriteChildren(inlines);
            WriteEndItems();
        }

        /// <summary></summary>
        /// <param name="block"></param>
        /// <param name="preserveSpaces"></param>
        public void WriteItems([NotNull] ContainerBlock block, bool preserveSpaces = false)
        {
            if (block == null)
                throw new ArgumentNullException(nameof(block));

            var member = xamlTypes.Peek().ContentProperty ?? throw new ArgumentNullException(nameof(XamlType.ContentProperty));
            WriteStartItems(member, preserveSpaces);
            WriteChildren(block);
            WriteEndItems();
        }

        private bool IsPendingText => textBuffer.Length > 0;

        #endregion

        /// <summary>Render the markdown object in a XamlWriter.</summary>
        /// <param name="markdownObject"></param>
        /// <returns></returns>
        [NotNull]
		public override object Render(MarkdownObject markdownObject)
		{
            if (markdownObject is MarkdownDocument)
            {
                // emit namespaces
                writer.WriteNamespace(new NamespaceDeclaration("http://schemas.microsoft.com/winfx/2006/xaml", "x"));
                writer.WriteNamespace(new NamespaceDeclaration("clr-namespace:Markdig.Wpf;assembly=Markdig.Wpf", "markdig"));

                // start flow document
                WriteStartObject(typeof(FlowDocument));

                //WriteStartItems(nameof(FlowDocument.Resources));
                //WriteEndItems();
                
                 WriteStaticResourceMember(null, "markdig:Styles.DocumentStyleKey");
                
                WriteStartItems(nameof(FlowDocument.Blocks));

                Write(markdownObject);

                WriteEndItems();
                return WriteEndObject();
            }
            else
            {
                Write(markdownObject);
                return GetResult();
            }
	    }

        /// <summary>Acces to current schema context.</summary>
        public XamlSchemaContext SchemaContext => writer.SchemaContext;

        internal void WriteStaticResourceMember(XamlMember member, string value)
        {
            WriteStartMember(member ?? GetMember("Style"));
            WriteStartObject(typeof(StaticResourceExtension));
            WriteStartMember(XamlLanguage.PositionalParameters);

            WriteStartObject(XamlLanguage.Static);
            WriteStartMember(XamlLanguage.PositionalParameters);

            writer.WriteValue(value);

            WriteEndMember();
            WriteEndObject();


            WriteEndMember();
            WriteEndObject();
            WriteEndMember();
        }

        //private void WriteStaticMember(XamlMember member, string value)
        //{
        //	nodes.Add(new XamlStartMember(member));
        //	nodes.Add(new XamlStartObject(XamlLanguage.Static));
        //	nodes.Add(new XamlStartMember(XamlLanguage.PositionalParameters));

        //	nodes.Add(new XamlValue(value));

        //	nodes.Add(new XamlEndMember());
        //	nodes.Add(new XamlEndObject());
        //	nodes.Add(new XamlEndMember());
        //}
    }
}
