// Copyright (c) 2016 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license. 
// See the LICENSE.md file in the project root for more information.

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Xaml;
using Markdig.Wpf;
using XamlReader = System.Windows.Markup.XamlReader;

namespace Markdig.Xaml.SampleApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private static MarkdownPipeline BuildPipeline()
        {
            return new MarkdownPipelineBuilder()
                .UseSupportedExtensions()
                .Build();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
			var markdown = File.ReadAllText("Documents/Markdig-readme.md");
			var xaml = Wpf.Markdown.ToXaml(markdown, BuildPipeline());
			//File.WriteAllText("d:\\temp\\text.xml", xaml);
			using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(xaml)))
            {
                var reader = new XamlXmlReader(stream, XamlReader.GetWpfSchemaContext());
				if (XamlReader.Load(reader) is FlowDocument document)
					Viewer.Document = document;
				else
					MessageBox.Show("Result is no FlowDocument?");
			}
        }

        private void OpenHyperlink(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            Process.Start(e.Parameter.ToString());
        }

        class MyXamlSchemaContext : XamlSchemaContext
        {
            public override bool TryGetCompatibleXamlNamespace(string xamlNamespace, out string compatibleNamespace)
            {
                if (xamlNamespace.Equals("clr-namespace:Markdig.Wpf"))
                {
                    compatibleNamespace = $"clr-namespace:Markdig.Wpf;assembly={Assembly.GetAssembly(typeof(Markdig.Wpf.Styles)).FullName}";
                    return true;
                }
                return base.TryGetCompatibleXamlNamespace(xamlNamespace, out compatibleNamespace);
            }
        }
    }
}
