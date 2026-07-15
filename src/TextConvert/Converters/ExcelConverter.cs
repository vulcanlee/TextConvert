using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace TextConvert.Converters;

/// <summary>
/// Excel(.xlsx)轉換器。使用 OpenXML SDK 依工作表逐列擷取,
/// 每列以 Tab 分隔儲存格值;會解析共用字串並補齊稀疏欄位間的空格。
/// </summary>
public sealed class ExcelConverter : IDocumentConverter
{
    public bool CanHandle(string extension) =>
        string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase);

    public string Convert(string filePath)
    {
        using var document = SpreadsheetDocument.Open(filePath, false);
        var workbookPart = document.WorkbookPart;
        if (workbookPart is null)
        {
            return string.Empty;
        }

        var sheets = workbookPart.Workbook?.Sheets;
        if (sheets is null)
        {
            return string.Empty;
        }

        var sharedStrings = GetSharedStrings(workbookPart);
        var builder = new StringBuilder();

        foreach (var sheet in sheets.Elements<Sheet>())
        {
            if (sheet.Id?.Value is not { } relationshipId)
            {
                continue;
            }

            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(relationshipId);
            builder.AppendLine($"# {sheet.Name}");

            var sheetData = worksheetPart.Worksheet?.GetFirstChild<SheetData>();
            if (sheetData is not null)
            {
                foreach (var row in sheetData.Elements<Row>())
                {
                    builder.AppendLine(GetRowText(row, sharedStrings));
                }
            }

            builder.AppendLine();
        }

        return builder.ToString().Trim();
    }

    private static string GetRowText(Row row, IReadOnlyList<string> sharedStrings)
    {
        var values = new List<string>();
        var expectedColumn = 0;

        foreach (var cell in row.Elements<Cell>())
        {
            // 依儲存格參照(如 "C5")補齊被略過的空欄位,維持欄位對齊。
            var columnIndex = GetColumnIndex(cell.CellReference?.Value);
            while (columnIndex > expectedColumn)
            {
                values.Add(string.Empty);
                expectedColumn++;
            }

            values.Add(GetCellValue(cell, sharedStrings));
            expectedColumn++;
        }

        return string.Join('\t', values);
    }

    private static string GetCellValue(Cell cell, IReadOnlyList<string> sharedStrings)
    {
        var value = cell.CellValue?.InnerText ?? string.Empty;

        if (cell.DataType?.Value == CellValues.SharedString
            && int.TryParse(value, out var index)
            && index >= 0 && index < sharedStrings.Count)
        {
            return sharedStrings[index];
        }

        if (cell.DataType?.Value == CellValues.Boolean)
        {
            return value == "1" ? "TRUE" : "FALSE";
        }

        // 內嵌字串(InlineString)直接取其文字。
        if (cell.DataType?.Value == CellValues.InlineString)
        {
            return cell.InlineString?.Text?.Text ?? cell.InnerText;
        }

        return value;
    }

    private static IReadOnlyList<string> GetSharedStrings(WorkbookPart workbookPart)
    {
        var table = workbookPart.SharedStringTablePart?.SharedStringTable;
        if (table is null)
        {
            return [];
        }

        return table.Elements<SharedStringItem>()
            .Select(item => item.InnerText)
            .ToList();
    }

    /// <summary>
    /// 由儲存格參照(如 "AB12")解析出 0-based 欄位索引。無效參照回傳 0。
    /// </summary>
    private static int GetColumnIndex(string? cellReference)
    {
        if (string.IsNullOrEmpty(cellReference))
        {
            return 0;
        }

        var columnIndex = 0;
        foreach (var c in cellReference)
        {
            if (c is >= 'A' and <= 'Z')
            {
                columnIndex = (columnIndex * 26) + (c - 'A' + 1);
            }
            else if (c is >= 'a' and <= 'z')
            {
                columnIndex = (columnIndex * 26) + (c - 'a' + 1);
            }
            else
            {
                break;
            }
        }

        return columnIndex - 1;
    }
}
