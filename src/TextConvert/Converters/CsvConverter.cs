using System.Text;

namespace TextConvert.Converters;

/// <summary>
/// CSV 轉換器(表格類)。以 RFC 4180 規則解析欄位(支援雙引號包裹、跳脫引號、
/// 欄位內逗號與換行),每列以 Tab 分隔輸出,風格與 Excel 擷取一致。
/// </summary>
public sealed class CsvConverter : IDocumentConverter
{
    public bool CanHandle(string extension) =>
        string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase);

    public string Convert(string filePath)
    {
        var content = File.ReadAllText(filePath);
        var rows = ParseCsv(content);
        return string.Join(Environment.NewLine, rows.Select(row => string.Join('\t', row)));
    }

    /// <summary>
    /// 將 CSV 內容解析為列/欄二維結構。
    /// </summary>
    private static List<List<string>> ParseCsv(string content)
    {
        var rows = new List<List<string>>();
        var currentRow = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < content.Length; i++)
        {
            var c = content[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    // 連續兩個引號表示一個跳脫的引號字元。
                    if (i + 1 < content.Length && content[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    field.Append(c);
                }

                continue;
            }

            switch (c)
            {
                case '"':
                    inQuotes = true;
                    break;
                case ',':
                    currentRow.Add(field.ToString());
                    field.Clear();
                    break;
                case '\r':
                    // 交由 \n 處理換行;單獨的 \r 亦視為行尾。
                    if (i + 1 >= content.Length || content[i + 1] != '\n')
                    {
                        currentRow.Add(field.ToString());
                        field.Clear();
                        rows.Add(currentRow);
                        currentRow = [];
                    }
                    break;
                case '\n':
                    currentRow.Add(field.ToString());
                    field.Clear();
                    rows.Add(currentRow);
                    currentRow = [];
                    break;
                default:
                    field.Append(c);
                    break;
            }
        }

        // 收尾:最後一個欄位/列(檔案未以換行結尾時)。
        if (field.Length > 0 || currentRow.Count > 0)
        {
            currentRow.Add(field.ToString());
            rows.Add(currentRow);
        }

        return rows;
    }
}
