// Copyright (c) 2016-2017 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license. 
// See the LICENSE.md file in the project root for more information.

using System.Windows;
using Markdig.Annotations;
using Markdig.Extensions.Tables;
using Markdig.Wpf;

using MdTable = Markdig.Extensions.Tables.Table;
using MdTableRow = Markdig.Extensions.Tables.TableRow;
using MdTableCell = Markdig.Extensions.Tables.TableCell;

using WpfTable = System.Windows.Documents.Table;
using WpfTableCell = System.Windows.Documents.TableCell;
using WpfTableColumn = System.Windows.Documents.TableColumn;
using WpfTableRow = System.Windows.Documents.TableRow;
using WpfTableRowGroup = System.Windows.Documents.TableRowGroup;

namespace Markdig.Renderers.Xaml.Extensions
{
    /// <summary></summary>
    public class TableRenderer : XamlObjectRenderer<MdTable>
    {
        /// <summary></summary>
        /// <param name="renderer"></param>
        /// <param name="table"></param>
        protected override void Write([NotNull] XamlRenderer renderer, [NotNull] MdTable table)
        {
            renderer.WriteStartObject(typeof(WpfTable));
            renderer.WriteStaticResourceMember(null, "markdig:Styles.TableStyleKey");
            var t = new WpfTable();
            
            renderer.WriteStartItems(nameof(WpfTable.Columns));
            foreach(var col in table.ColumnDefinitions)
            {
                renderer.WriteStartObject(typeof(WpfTableColumn));
                renderer.WriteMember(nameof(WpfTableColumn.Width),
                    (col?.Width ?? 0) != 0
                        ? new GridLength(col.Width, GridUnitType.Star)
                        : GridLength.Auto
                );
                renderer.WriteEndObject();
            }
            renderer.WriteEndItems();

            renderer.WriteStartItems(nameof(WpfTable.RowGroups));
            renderer.WriteStartObject(typeof(WpfTableRowGroup));
            renderer.WriteStartItems(nameof(WpfTableRowGroup.Rows));

            foreach (var c in table)
            {
                var row = (MdTableRow)c;
                renderer.WriteStartObject(typeof(WpfTableRow));
                if (row.IsHeader)
                    renderer.WriteStaticResourceMember(null, "markdig:Styles.TableHeaderStyleKey");
                renderer.WriteStartItems(nameof(WpfTableRow.Cells));

                for (var i = 0; i < row.Count; i++)
                {
                    var cell = (MdTableCell)row[i];
                    renderer.WriteStartObject(typeof(WpfTableCell));
                    renderer.WriteStaticResourceMember(null, "markdig:Styles.TableCellStyleKey");

                    if (cell.ColumnSpan > 1)
                        renderer.WriteMember(nameof(WpfTableCell.ColumnSpan), cell.ColumnSpan);
                    if (cell.RowSpan > 1)
                        renderer.WriteMember(nameof(WpfTableCell.RowSpan), cell.RowSpan);

                    var columnIndex = cell.ColumnIndex < 0 || cell.ColumnIndex >= table.ColumnDefinitions.Count ? i : cell.ColumnIndex;
                    columnIndex = columnIndex >= table.ColumnDefinitions.Count ? table.ColumnDefinitions.Count - 1 : columnIndex;
                    var alignment = table.ColumnDefinitions[columnIndex].Alignment;
                    if (alignment.HasValue)
                    {
                        switch (alignment)
                        {
                            case TableColumnAlign.Center:
                                renderer.WriteMember(nameof(WpfTableCell.TextAlignment), TextAlignment.Center);
                                break;
                            case TableColumnAlign.Right:
                                renderer.WriteMember(nameof(WpfTableCell.TextAlignment), TextAlignment.Right);
                                break;
                            case TableColumnAlign.Left:
                                renderer.WriteMember(nameof(WpfTableCell.TextAlignment), TextAlignment.Left);
                                break;
                        }
                    }

                    renderer.WriteItems(cell);

                    renderer.WriteEndObject();
                }

                renderer.WriteEndItems();
                renderer.WriteEndObject();
            }

            renderer.WriteEndItems();
            renderer.WriteEndObject();
            renderer.WriteEndItems();

            renderer.WriteEndObject();
        }
    }
}
