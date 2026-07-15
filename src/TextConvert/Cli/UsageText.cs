namespace TextConvert.Cli;

/// <summary>
/// 命令列說明文字(繁體中文),供 -h/--help 顯示。
/// </summary>
public static class UsageText
{
    public const string Value = """
TextConvert - 文件轉純文字批次轉換工具

用途:
  將指定來源目錄下的 Office、PDF、Markdown、Text、HTML、CSV、JSON、XML 檔案,
  批次轉換為純文字內容,輸出到指定目錄,供知識庫使用。不支援的檔案會自動跳過。

用法:
  TextConvert.exe [選項]

選項:
  -s, --source <目錄>    來源目錄(覆寫 appsettings.json 的設定)
  -o, --output <目錄>    輸出目錄(覆寫 appsettings.json 的設定)
  -h, --help, -?         顯示本說明後結束
  -v, --version          顯示版本後結束

設定檔:
  未提供 --source / --output 時,改讀取同目錄下 appsettings.json 的
  Conversion.SourceDirectory 與 Conversion.OutputDirectory,
  並可設定 Recursive(是否遞迴子目錄)與 Overwrite(是否覆寫既有輸出)。

支援格式:
  Word(.docx)、Excel(.xlsx)、PowerPoint(.pptx)、PDF(.pdf,僅內嵌文字)、
  CSV(.csv)、Markdown(.md/.markdown)、HTML(.html/.htm)、
  純文字(.txt/.text/.json/.xml)。

輸出規則:
  依來源子目錄結構鏡像輸出,檔名保留原副檔名,例如 report.docx -> report.docx.txt。

日誌:
  執行過程輸出到主控台,並寫入執行目錄下 logs\textconvert-YYYYMMDD.log。

範例:
  TextConvert.exe --source "D:\來源" --output "D:\輸出"
  TextConvert.exe -s "D:\來源" -o "D:\輸出"
  TextConvert.exe            (使用 appsettings.json 的設定)
""";
}
