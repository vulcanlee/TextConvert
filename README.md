# 文字轉換 TextConvert

以 .NET 10 / C# 開發的 Console CLI 工具,將指定目錄下的多種文件格式批次轉換為
純文字內容,供知識庫(RAG)處理之用。不符合條件的檔案會自動跳過。

## 支援格式

| 類別 | 副檔名 |
|------|--------|
| Word | `.docx` |
| Excel / 表格 | `.xlsx`、`.csv` |
| PowerPoint | `.pptx` |
| PDF | `.pdf`(僅內嵌文字,不做 OCR) |
| 標記 / 網頁 | `.md`、`.markdown`、`.html`、`.htm` |
| 純文字 | `.txt`、`.text`、`.json`、`.xml` |

> 不支援舊版二進位 Office(`.doc`/`.xls`/`.ppt`)。

## 建置與執行

需求:.NET 10 SDK。

```
cd src
dotnet build
dotnet run --project TextConvert -- --source "來源目錄" --output "輸出目錄"
```

執行測試:

```
cd src
dotnet test
```

## 使用方式

- 預設讀取 `src/TextConvert/appsettings.json` 的來源與輸出目錄。
- 命令列參數:
  - `--source <目錄>` / `-s <目錄>`:覆寫來源目錄。
  - `--output <目錄>` / `-o <目錄>`:覆寫輸出目錄。
  - `--help` / `-h` / `-?`:顯示說明後結束。
  - `--version` / `-v`:顯示版本後結束。
- 輸出檔採鏡像子目錄結構,檔名保留原副檔名(例:`report.docx` → `report.docx.txt`)。

## 設定檔(appsettings.json)

```json
{
  "Version": "1.0.0",
  "Conversion": {
    "SourceDirectory": "",
    "OutputDirectory": "",
    "Recursive": true,
    "Overwrite": true
  },
  "Serilog": { "...": "日誌等級與輸出設定" }
}
```

- `Recursive`:是否遞迴掃描子目錄。
- `Overwrite`:輸出檔已存在時是否覆寫。
- 日誌同時輸出到 Console 與 `logs/` 每日滾動檔案,涵蓋 Information / Warning / Error 等級。

## 發布(Publish)

以自封裝單一檔案發布(win-x64,使用者端無需安裝 .NET):

```
cd src
dotnet publish TextConvert/TextConvert.csproj -p:PublishProfile=FolderProfile
```

輸出於 `src/TextConvert/bin/Release/net10.0/publish/win-x64/`,內含
`TextConvert.exe`、`appsettings.json` 及自動附帶的 `使用者操作說明.md`。

> 發布版執行說明見 [使用者操作說明](docs/guides/使用者操作說明.md)。

## 文件

完整文件見 [`docs/`](docs/README.md):產品需求(PRD)、系統設計(architecture)、
使用指南(guides)、變更紀錄(changelog)。

## 專案結構

```
src/
  TextConvert/          CLI 主專案(Converters / Services / Configuration / Models)
  TextConvert.Tests/    xUnit 單元測試
docs/                   說明文件
```
