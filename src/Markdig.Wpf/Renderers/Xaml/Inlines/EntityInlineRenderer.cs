// Copyright (c) 2016-2017 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license.
// See the LICENSE.md file in the project root for more information.

using System;
using System.IO;
using System.Xaml;
using Markdig.Annotations;
using Markdig.Renderers;
using Markdig.Renderers.Xaml;
using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Xaml.Inlines
{
    /// <summary></summary>
    public class EntityInlineRenderer : XamlObjectRenderer<HtmlEntityInline>
    {
        /// <summary></summary>
        /// <param name="renderer"></param>
        /// <param name="obj"></param>
        protected override void Write([NotNull] XamlRenderer renderer, [NotNull] HtmlEntityInline obj)
        {
            var txt = obj.Transcoded.Text.Substring(obj.Transcoded.Start, obj.Transcoded.Length);
            using (var xaml = new XamlXmlReader(new StringReader(txt), new XamlXmlReaderSettings() { }))
            {
                while (xaml.Read())
                {
                    switch (xaml.NodeType)
                    {
                        case XamlNodeType.NamespaceDeclaration:
                            renderer.WriteNamespace(xaml.Namespace);
                            break;
                        case XamlNodeType.StartObject:
                            renderer.WriteStartObject(xaml.Type);
                            break;
                        case XamlNodeType.GetObject:
                            renderer.WriteGetObject();
                            break;
                        case XamlNodeType.EndObject:
                            renderer.WriteEndObject();
                            break;

                        case XamlNodeType.StartMember:
                            renderer.WriteStartMember(xaml.Member);
                            break;
                        case XamlNodeType.EndMember:
                            renderer.WriteEndMember();
                            break;
                        case XamlNodeType.Value:
                            if (xaml.Value is string text)
                                renderer.WriteValue(text);
                            else
                                renderer.WriteValue(xaml.Value.ToString()); // todo: use xaml to text converter
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }
        } // proc Write
    } // class EntityInlineRenderer
}
