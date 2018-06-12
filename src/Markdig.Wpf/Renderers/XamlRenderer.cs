// Copyright (c) 2016-2017 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license.
// See the LICENSE.md file in the project root for more information.

// Some parts taken from https://github.com/lunet-io/markdig
// Copyright (c) 2016 Alexandre Mutel. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Xaml;
using Markdig.Annotations;
using Markdig.Helpers;
using Markdig.Renderers.Xaml;
using Markdig.Renderers.Xaml.Inlines;
using Markdig.Syntax;

namespace Markdig.Renderers
{
	/// <summary>
	/// XAML renderer for a Markdown <see cref="MarkdownDocument"/> object.
	/// </summary>
	public class XamlRenderer : RendererBase
	{
		private abstract class XamlNode
		{
			public abstract XamlNodeType Type { get; }
		} // class XamlNode

		private sealed class XamlNamespaceDeclaration : XamlNode
		{
			public XamlNamespaceDeclaration(NamespaceDeclaration namespaceDeclaration)
				=> this.NamespaceDeclaration = namespaceDeclaration;

			public NamespaceDeclaration NamespaceDeclaration { get; }
			public override XamlNodeType Type => XamlNodeType.NamespaceDeclaration;
		} // class XamlNamespaceDeclaration
		

		private sealed class XamlStartObject : XamlNode
		{
			public XamlStartObject(XamlType type)
				=> this.XamlType = type;

			public XamlType XamlType { get; }
			public override XamlNodeType Type => XamlNodeType.StartObject;
		} // class XamlStartObject

		private sealed class XamlEndObject : XamlNode
		{
			public override XamlNodeType Type => XamlNodeType.EndObject;
		} // class XamlEndObject

		private sealed class XamlGetObject : XamlNode
		{
			public override XamlNodeType Type => XamlNodeType.GetObject;
		} // class XamlGetObject

		private sealed class XamlStartMember : XamlNode
		{
			public XamlStartMember(XamlMember member)
				=> this.Member = member;

			public XamlMember Member { get; }
			public override XamlNodeType Type => XamlNodeType.StartMember;
		} // class XamlStartMember

		private sealed class XamlEndMember : XamlNode
		{
			public override XamlNodeType Type => XamlNodeType.EndMember;
		} // class XamlEndMember

		private sealed class XamlValue : XamlNode
		{
			public XamlValue(object value)
				=> this.Value = value;

			public object Value { get; }
			public override XamlNodeType Type => XamlNodeType.Value;
		} // class XamlValue

		private sealed class XamlNodeReader : XamlReader
		{
			private readonly XamlSchemaContext schemaContext;
			private readonly IEnumerator<XamlNode> nodes;
			private bool isEof = false;
			private string indent = "";

			public XamlNodeReader(XamlSchemaContext schemaContext, IEnumerable<XamlNode> nodes)
			{
				this.schemaContext = schemaContext ?? throw new ArgumentNullException(nameof(schemaContext));
				this.nodes = nodes.GetEnumerator();
			}

			protected override void Dispose(bool disposing)
			{
				nodes.Dispose();
				base.Dispose(disposing);
			}

			public override bool Read()
			{
				if (isEof)
					return false;
				else if (nodes.MoveNext())
				{
					switch(Current)
					{
						case XamlNamespaceDeclaration ns:
							Debug.Print("{0}{1} {2}", indent, Current.Type, ns.NamespaceDeclaration);
							break;
						case XamlStartObject o:
							Debug.Print("{0}{1} {2}", indent, Current.Type, o.XamlType.Name);
							indent = indent + ". ";
							break;
						case XamlStartMember m:
							Debug.Print("{0}{1} {2}", indent, Current.Type, m.Member.Name);
							indent = indent + ". ";
							break;
						case XamlEndObject eo:
							indent = indent.Substring(0, indent.Length - 2);
							Debug.Print("{0}{1}", indent, Current.Type);
							break;
						case XamlEndMember em:
							indent = indent.Substring(0, indent.Length - 2);
							Debug.Print("{0}{1}", indent, Current.Type);
							break;
						case XamlGetObject go:
							Debug.Print("{0}{1}", indent, Current.Type);
							indent = indent + ". ";
							break;
						case XamlValue v:
							Debug.Print("{0}{1} {2}", indent, Current.Type, v.Value);
							break;
						default:
							Debug.Print("{0}{1}", indent, Current.Type);
							break;
					}
					return true;
				}
				else
				{
					isEof = true;
					return false;
				}
			}

			public override bool IsEof => isEof;
			private XamlNode Current => isEof ? null : nodes.Current;

			public override XamlNodeType NodeType => Current?.Type ?? XamlNodeType.None;
			public override NamespaceDeclaration Namespace => Current is XamlNamespaceDeclaration nd ? nd.NamespaceDeclaration : null;
			public override XamlType Type => Current is XamlStartObject t ? t.XamlType : null;
			public override XamlMember Member => Current is XamlStartMember v ? v.Member : null;
			public override object Value => Current is XamlValue v ? v.Value : null;

			public override XamlSchemaContext SchemaContext => schemaContext;
		} // class XamlNodeReader

		private sealed class DisposeAdd : IDisposable
		{
			private readonly XamlRenderer renderer;
			private readonly XamlNode node;

			public DisposeAdd(XamlRenderer renderer, XamlNode node)
			{
				this.renderer = renderer;
				this.node = node;
			}

			public void Dispose()
				=> renderer.nodes.Add(node);
		} // class DisposeAdd

		private sealed class DisposeInvoke : IDisposable
		{
			private readonly Action onDispose;

			public DisposeInvoke(Action onDispose)
				=> this.onDispose = onDispose;

			public void Dispose()
				=> onDispose.Invoke();
		} // class DisposeInvoke

		private sealed class DisposeCombine : IDisposable
		{
			private readonly IDisposable[] disposes;

			public DisposeCombine(params IDisposable[] disposes)
				=> this.disposes = disposes;

			public void Dispose()
				=> Array.ForEach(disposes, c => c.Dispose());
		} // class DisposeCombine

		private static readonly XamlSchemaContext schemaContext = System.Windows.Markup.XamlReader.GetWpfSchemaContext();

		private static readonly XamlType staticResourceType;

		private static readonly XamlType flowDocumentType;

		private static readonly XamlType paragraphType;

		private static readonly XamlType hyperLinkType;
		private static readonly XamlType spanType;
		private static readonly XamlType boldType;
		private static readonly XamlType italicType;
		private static readonly XamlType runType;
		private static readonly XamlType lineBreakType;

		private static readonly XamlMember flowDocumentBlocksMember;
		private static readonly XamlMember paragraphInlinesMember;
		private static readonly XamlMember contentStyleMember;

		private static readonly XamlMember hyperLinkCommandMember;
		private static readonly XamlMember hyperLinkCommandParameterMember;
		private static readonly XamlMember spanInlinesMember;
		private static readonly XamlMember runTextMember;

		static XamlRenderer()
		{
			staticResourceType= schemaContext.GetXamlType(typeof(StaticResourceExtension));
			flowDocumentType = schemaContext.GetXamlType(typeof(FlowDocument));
			flowDocumentBlocksMember = flowDocumentType.GetMember(nameof(FlowDocument.Blocks));

			paragraphType = schemaContext.GetXamlType(typeof(Paragraph));
			paragraphInlinesMember = paragraphType.GetMember(nameof(Paragraph.Inlines));

			contentStyleMember = flowDocumentType.GetMember(nameof(FrameworkContentElement.Style));

			hyperLinkType = schemaContext.GetXamlType(typeof(Hyperlink));
			hyperLinkCommandMember = hyperLinkType.GetMember(nameof(Hyperlink.Command));
			hyperLinkCommandParameterMember = hyperLinkType.GetMember(nameof(Hyperlink.CommandParameter));

			spanType = schemaContext.GetXamlType(typeof(Span));
			boldType = schemaContext.GetXamlType(typeof(Bold));
			italicType = schemaContext.GetXamlType(typeof(Italic));
			spanInlinesMember = spanType.GetMember(nameof(Span.Inlines));

			runType = schemaContext.GetXamlType(typeof(Run));
			runTextMember = runType.GetMember(nameof(Run.Text));

			lineBreakType = schemaContext.GetXamlType(typeof(LineBreak));
		} // sctor

		private readonly List<XamlNode> nodes = new List<XamlNode>();


		/// <summary>
		/// Initializes a new instance of the <see cref="XamlRenderer"/> class.
		/// </summary>
		public XamlRenderer()
        {
            // Default block renderers
            ObjectRenderers.Add(new CodeBlockRenderer());
            ObjectRenderers.Add(new ListRenderer());
            ObjectRenderers.Add(new HeadingRenderer());
            ObjectRenderers.Add(new HtmlBlockRenderer());
            ObjectRenderers.Add(new ParagraphRenderer());
            ObjectRenderers.Add(new QuoteBlockRenderer());
            ObjectRenderers.Add(new ThematicBreakRenderer());

            // Default inline renderers
            ObjectRenderers.Add(new AutolinkInlineRenderer());
            ObjectRenderers.Add(new CodeInlineRenderer());
            ObjectRenderers.Add(new DelimiterInlineRenderer());
            ObjectRenderers.Add(new EmphasisInlineRenderer());
            ObjectRenderers.Add(new LineBreakInlineRenderer());
            ObjectRenderers.Add(new HtmlInlineRenderer());
            ObjectRenderers.Add(new HtmlEntityInlineRenderer());
            ObjectRenderers.Add(new LinkInlineRenderer());
            ObjectRenderers.Add(new LiteralInlineRenderer());
        }

		[NotNull]
		public override object Render(MarkdownObject markdownObject)
		{
			nodes.Clear(); // clear nodes

			if (markdownObject is MarkdownDocument)
			{
				nodes.Add(new XamlNamespaceDeclaration(new NamespaceDeclaration("http://schemas.microsoft.com/winfx/2006/xaml", "x")));
				nodes.Add(new XamlNamespaceDeclaration(new NamespaceDeclaration("clr-namespace:Markdig.Wpf;assembly=Markdig.Wpf", "markdig")));
				using (BeginObject(flowDocumentType))
				{
					WriteStaticResourceMember(contentStyleMember, "markdig:Styles.DocumentStyleKey");
					using (BeginAddChilds(flowDocumentBlocksMember))
						Write(markdownObject);
				}
			}
			else
				Write(markdownObject);

			return new XamlNodeReader(schemaContext, nodes);
		}


		internal IDisposable BeginObject(XamlType type)
		{
			nodes.Add(new XamlStartObject(type));
			return new DisposeAdd(this, new XamlEndObject());
		}

		private IDisposable BeginMember(XamlMember member)
		{
			nodes.Add(new XamlStartMember(member));
			return new DisposeAdd(this, new XamlEndMember());
		}

		internal IDisposable BeginAddChilds(XamlMember member)
		{
			nodes.Add(new XamlStartMember(member));
			nodes.Add(new XamlGetObject());
			nodes.Add(new XamlStartMember(XamlLanguage.Items));

			return new DisposeInvoke(EndAddChilds);
		} 

		private void EndAddChilds()
		{
			nodes.Add(new XamlEndMember());
			nodes.Add(new XamlEndObject());
			nodes.Add(new XamlEndMember());
		}


		internal void WriteMember(XamlMember member, string value)
		{
			if (value == null)
				return;

			nodes.Add(new XamlStartMember(member));
			nodes.Add(new XamlValue(value));
			nodes.Add(new XamlEndMember());
		}

		internal void WriteStaticResourceMember(XamlMember member, string value)
		{
			nodes.Add(new XamlStartMember(member));
			nodes.Add(new XamlStartObject(staticResourceType));
			nodes.Add(new XamlStartMember(XamlLanguage.PositionalParameters));

			nodes.Add(new XamlStartObject(XamlLanguage.Static));
			nodes.Add(new XamlStartMember(XamlLanguage.PositionalParameters));

			nodes.Add(new XamlValue(value));

			nodes.Add(new XamlEndMember());
			nodes.Add(new XamlEndObject());

			nodes.Add(new XamlEndMember());
			nodes.Add(new XamlEndObject());
			nodes.Add(new XamlEndMember());
		}

		private void WriteStaticMember(XamlMember member, string value)
		{
			nodes.Add(new XamlStartMember(member));
			nodes.Add(new XamlStartObject(XamlLanguage.Static));
			nodes.Add(new XamlStartMember(XamlLanguage.PositionalParameters));

			nodes.Add(new XamlValue(value));

			nodes.Add(new XamlEndMember());
			nodes.Add(new XamlEndObject());
			nodes.Add(new XamlEndMember());
		}

		internal IDisposable BeginHyperlink(string url)
		{
			var r1 = BeginObject(hyperLinkType);

			WriteStaticMember(hyperLinkCommandMember, "markdig:Commands.Hyperlink");
			WriteMember(hyperLinkCommandParameterMember, url);

			var r2 = BeginAddChilds(spanInlinesMember);

			return new DisposeCombine(r2, r1);
		}

		internal IDisposable BeginBold()
		{
			var r1 = BeginObject(boldType);
			var r2 = BeginAddChilds(spanInlinesMember);
			return new DisposeCombine(r2, r1);
		}

		internal IDisposable BeginItalic()
		{
			var r1 = BeginObject(italicType);
			var r2 = BeginAddChilds(spanInlinesMember);
			return new DisposeCombine(r2, r1);
		}

		internal IDisposable BeginSpan(string staticResourceKey = null)
		{
			var r1 = BeginObject(spanType);
			if (staticResourceKey != null)
				WriteStaticResourceMember(contentStyleMember, staticResourceKey);
			var r2 = BeginAddChilds(spanInlinesMember);
			return new DisposeCombine(r2, r1);
		}

		internal IDisposable BeginParagraph(string staticResourceKey)
		{
			var r1 = BeginObject(paragraphType);

			if (staticResourceKey != null)
				WriteStaticResourceMember(contentStyleMember, staticResourceKey);
			
			var r2 = BeginAddChilds(paragraphInlinesMember);

			return new DisposeCombine(r2, r1);
		}

		internal void WriteText(ref StringSlice text)
		{
			if (!text.IsEmpty)
				WriteText(text.Text, text.Start, text.End);
		}

		internal void WriteText(string text, int start, int end)
		{
			if (start == 0 && text.Length == end)
				WriteText(text);
			else
				WriteText(text.Substring(start, end - start + 1));
		}

		internal void WriteText(string text)
			=> WriteText(null, text);

		internal void WriteText(ResourceKey style, string text)
		{
			using (BeginObject(runType))
			{
				//	WriteResourceMember(contentStyleMember, style);
				WriteMember(runTextMember, text);
			}
		}

		internal void WriteNewLine()
		{
			nodes.Add(new XamlStartObject(lineBreakType));
			nodes.Add(new XamlEndObject());
		}

		/// <summary>
		/// Writes the inlines of a leaf inline.
		/// </summary>
		/// <param name="leafBlock">The leaf block.</param>
		/// <returns>This instance</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void WriteLeafInline([NotNull] LeafBlock leafBlock)
		{
			if (leafBlock == null)
				throw new ArgumentNullException(nameof(leafBlock));

			var inline = (Syntax.Inlines.Inline)leafBlock.Inline;
			while (inline != null)
			{
				Write(inline);
				inline = inline.NextSibling;
			}
		}
		internal void WriteRawLines([NotNull] LeafBlock leafBlock, bool writeEndOfLines)
		{
			if (leafBlock == null)
				throw new ArgumentNullException(nameof(leafBlock));

			if (leafBlock.Lines.Lines != null)
			{
				var lines = leafBlock.Lines;
				var slices = lines.Lines;
				for (var i = 0; i < lines.Count; i++)
				{
					WriteText(ref slices[i].Slice);
					if (writeEndOfLines)
						WriteNewLine();
				}
			}
		}

		public static XamlSchemaContext todo => schemaContext;

		///// <summary>
		///// Writes the content escaped for XAML.
		///// </summary>
		///// <param name="content">The content.</param>
		///// <returns>This instance</returns>
		//[NotNull]
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public XamlRenderer WriteEscape([CanBeNull] string content)
		//{
		//    if (string.IsNullOrEmpty(content))
		//        return this;

		//    WriteEscape(content, 0, content.Length);
		//    return this;
		//}

		///// <summary>
		///// Writes the content escaped for XAML.
		///// </summary>
		///// <param name="slice">The slice.</param>
		///// <param name="softEscape">Only escape &lt; and &amp;</param>
		///// <returns>This instance</returns>
		//[NotNull]
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public XamlRenderer WriteEscape(ref StringSlice slice, bool softEscape = false)
		//{
		//    if (slice.Start > slice.End)
		//    {
		//        return this;
		//    }
		//    return WriteEscape(slice.Text, slice.Start, slice.Length, softEscape);
		//}

		///// <summary>
		///// Writes the content escaped for XAML.
		///// </summary>
		///// <param name="slice">The slice.</param>
		///// <param name="softEscape">Only escape &lt; and &amp;</param>
		///// <returns>This instance</returns>
		//[NotNull]
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public XamlRenderer WriteEscape(StringSlice slice, bool softEscape = false)
		//{
		//    return WriteEscape(ref slice, softEscape);
		//}

		///// <summary>
		///// Writes the content escaped for XAML.
		///// </summary>
		///// <param name="content">The content.</param>
		///// <param name="offset">The offset.</param>
		///// <param name="length">The length.</param>
		///// <param name="softEscape">Only escape &lt; and &amp;</param>
		///// <returns>This instance</returns>
		//[NotNull]
		//public XamlRenderer WriteEscape([CanBeNull] string content, int offset, int length, bool softEscape = false)
		//{
		//    if (string.IsNullOrEmpty(content) || length == 0)
		//        return this;

		//    var end = offset + length;
		//    var previousOffset = offset;
		//    for (; offset < end; offset++)
		//    {
		//        switch (content[offset])
		//        {
		//            case '<':
		//                Write(content, previousOffset, offset - previousOffset);
		//                if (EnableHtmlEscape)
		//                {
		//                    Write("&lt;");
		//                }
		//                previousOffset = offset + 1;
		//                break;

		//            case '>':
		//                if (!softEscape)
		//                {
		//                    Write(content, previousOffset, offset - previousOffset);
		//                    if (EnableHtmlEscape)
		//                    {
		//                        Write("&gt;");
		//                    }
		//                    previousOffset = offset + 1;
		//                }
		//                break;

		//            case '&':
		//                Write(content, previousOffset, offset - previousOffset);
		//                if (EnableHtmlEscape)
		//                {
		//                    Write("&amp;");
		//                }
		//                previousOffset = offset + 1;
		//                break;

		//            case '"':
		//                if (!softEscape)
		//                {
		//                    Write(content, previousOffset, offset - previousOffset);
		//                    if (EnableHtmlEscape)
		//                    {
		//                        Write("&quot;");
		//                    }
		//                    previousOffset = offset + 1;
		//                }
		//                break;
		//        }
		//    }

		//    Write(content, previousOffset, end - previousOffset);
		//    return this;
		//}

		///// <summary>
		///// Writes the URL escaped for XAML.
		///// </summary>
		///// <param name="content">The content.</param>
		///// <returns>This instance</returns>
		//[NotNull]
		//public XamlRenderer WriteEscapeUrl([CanBeNull] string content)
		//{
		//    if (content == null)
		//        return this;

		//    var previousPosition = 0;
		//    var length = content.Length;

		//    for (var i = 0; i < length; i++)
		//    {
		//        var c = content[i];

		//        if (c < 128)
		//        {
		//            var escape = HtmlHelper.EscapeUrlCharacter(c);
		//            if (escape != null)
		//            {
		//                Write(content, previousPosition, i - previousPosition);
		//                previousPosition = i + 1;
		//                Write(escape);
		//            }
		//        }
		//        else
		//        {
		//            Write(content, previousPosition, i - previousPosition);
		//            previousPosition = i + 1;

		//            byte[] bytes;
		//            if (c >= '\ud800' && c <= '\udfff' && previousPosition < length)
		//            {
		//                bytes = Encoding.UTF8.GetBytes(new[] { c, content[previousPosition] });
		//                // Skip next char as it is decoded above
		//                i++;
		//                previousPosition = i + 1;
		//            }
		//            else
		//            {
		//                bytes = Encoding.UTF8.GetBytes(new[] { c });
		//            }

		//            foreach (var t in bytes)
		//            {
		//                Write($"%{t:X2}");
		//            }
		//        }
		//    }

		//    Write(content, previousPosition, length - previousPosition);
		//    return this;
		//}

		///// <summary>
		///// Writes the lines of a <see cref="LeafBlock"/>
		///// </summary>
		///// <param name="leafBlock">The leaf block.</param>
		///// <param name="writeEndOfLines">if set to <c>true</c> write end of lines.</param>
		///// <param name="escape">if set to <c>true</c> escape the content for XAML</param>
		///// <param name="softEscape">Only escape &lt; and &amp;</param>
		///// <returns>This instance</returns>
		//[NotNull]
		//public XamlRenderer WriteLeafRawLines([NotNull] LeafBlock leafBlock, bool writeEndOfLines, bool escape, bool softEscape = false)
		//{
		//    if (leafBlock == null) throw new ArgumentNullException(nameof(leafBlock));
		//    if (leafBlock.Lines.Lines != null)
		//    {
		//        var lines = leafBlock.Lines;
		//        var slices = lines.Lines;
		//        for (var i = 0; i < lines.Count; i++)
		//        {
		//            if (!writeEndOfLines && i > 0)
		//            {
		//                WriteLine();
		//            }
		//            if (escape)
		//            {
		//                WriteEscape(ref slices[i].Slice, softEscape);
		//            }
		//            else
		//            {
		//                Write(ref slices[i].Slice);
		//            }
		//            if (writeEndOfLines)
		//            {
		//                WriteLine();
		//            }
		//        }
		//    }
		//    return this;
		//}
	}
}
