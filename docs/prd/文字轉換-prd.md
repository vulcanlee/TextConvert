# 文字轉換 PRD(文件轉純文字批次轉換工具)

- 文件用途:定義 TextConvert 的產品需求、範圍與驗收標準。
- 主要讀者:開發者、產品規劃、驗收人員。
- 文件版本:1.1
- 文件狀態:已實作
- 現行系統版本:1.0.1
- 首次實作版本:1.0.0
- 最後核對日期:2026-07-15
- 建立日期:2026-07-15
- 編碼:UTF-8 繁體中文

> 相關文件:系統設計見 [文件轉純文字設計](../architecture/2026-07-15-文件轉純文字-design.md);
> 變更紀錄見 [初版建立](../changelog/2026-07-15-初版建立.md)。

## 一、目標與範圍

### 目標

提供一個 Console CLI 工具,將指定來源目錄下的多種文件格式批次轉換為純文字內容,
輸出到指定目錄,作為後續知識庫(RAG)處理的原始素材。

### 支援格式

| 類別 | 副檔名 |
|------|--------|
| Word | `.docx` |
| Excel / 表格 | `.xlsx`、`.csv` |
| PowerPoint | `.pptx` |
| PDF | `.pdf` |
| 標記 / 網頁 | `.md`、`.markdown`、`.html`、`.htm` |
| 純文字 | `.txt`、`.text`、`.json`、`.xml` |

### 非範圍(本版本不做)

- 舊版二進位 Office 格式(`.doc`、`.xls`、`.ppt`)。
- 影像型 PDF 的 OCR 文字辨識。
- 平行處理、chunk 切分、多種輸出格式(僅輸出 `.txt`)。

## 二、使用者與入口

| 項目 | 說明 |
|------|------|
| 入口 | 命令列執行 `TextConvert` |
| 主要使用者 | 需建置知識庫素材的開發者 / 資料工程師 |
| 設定來源 | `appsettings.json`,可由命令列參數覆寫 |

## 三、操作與參數

- 預設讀取 `appsettings.json` 的來源與輸出目錄。
- 命令列參數:
  - `--source <目錄>` 或 `-s <目錄>`:覆寫來源目錄。
  - `--output <目錄>` 或 `-o <目錄>`:覆寫輸出目錄。
  - `-h` / `--help` / `-?`:顯示說明後結束。
  - `-v` / `--version`:顯示版本後結束。
- 範例:
  ```
  TextConvert --source "D:\來源" --output "D:\輸出"
  TextConvert -h
  TextConvert --version
  ```

## 四、內部系統運作

1. 載入 `appsettings.json` 設定並初始化 Serilog 日誌(`Program.cs`)。
2. 套用命令列覆寫值後,驗證來源目錄存在、建立輸出目錄。
3. `FileScanner` 依 `Recursive` 設定遞迴列舉來源目錄檔案。
4. 逐檔透過 `ConverterRegistry` 依副檔名解析對應的 `IDocumentConverter`。
5. 呼叫轉換器擷取純文字,依鏡像子目錄結構寫出 `原檔名.原副檔名.txt`(UTF-8)。
6. 單一檔案失敗僅記錄 Error 並繼續下一檔;結束輸出成功 / 跳過 / 失敗統計。

## 五、擷取規則

- Word:逐段落擷取,表格以每列 Tab 分隔輸出。
- Excel:依工作表輸出(標頭 `# 工作表名`),每列以 Tab 分隔,解析共用字串並補齊稀疏欄位。
- CSV:依 RFC 4180 解析(支援引號、跳脫、欄位內逗號與換行),每列以 Tab 分隔。
- PowerPoint:依投影片順序擷取各文字方塊(標頭 `# 投影片 N`)。
- PDF:逐頁擷取內嵌文字(不做 OCR);無文字時記警告並輸出空內容。
- HTML:去除 `script`/`style` 等節點與標籤,保留段落換行。
- Markdown:轉為純文字,去除標記語法。
- Text / JSON / XML:原樣讀取內容輸出。

## 六、設定與日誌

- 設定檔 `appsettings.json`:
  - `Version`:版本編號(semver,每次建置 patch +1)。
  - `Conversion`:`SourceDirectory`、`OutputDirectory`、`Recursive`、`Overwrite`。
  - `Serilog`:日誌等級與輸出(Console 及每日滾動檔案 `logs/`)。
- 日誌等級涵蓋 Information(流程)、Warning(空內容 / 略過已存在)、
  Error(單檔失敗)、Fatal(未預期例外)。

## 七、錯誤與邊界

| 情境 | 行為 |
|------|------|
| 不支援的副檔名 | 跳過並記 Debug |
| 來源目錄不存在 | 拋出例外、記 Fatal、以非零離開碼結束 |
| 未指定來源 / 輸出目錄 | 記 Error 並以離開碼 1 結束 |
| 輸出已存在且 `Overwrite=false` | 跳過並記 Information |
| 單一檔案轉換失敗 | 記 Error,繼續處理其餘檔案 |
| 檔案無可擷取文字 | 記 Warning,輸出空內容 |

## 八、驗收與測試

- `dotnet build` 成功、`dotnet test` 全數通過(單元測試涵蓋各轉換器、掃描器、
  登錄與協調服務,含失敗不中斷、覆寫、鏡像結構等情境)。
- 端到端:對含各格式與不支援檔的目錄執行,輸出鏡像產生 `*.<ext>.txt`,
  不支援檔被跳過,`logs/` 產生含各等級訊息的日誌檔,中文無亂碼。

## 九、相關程式與文件

- 進入點:`src/TextConvert/Program.cs`
- 設定:`src/TextConvert/Configuration/AppSettings.cs`、`src/TextConvert/appsettings.json`
- 轉換器:`src/TextConvert/Converters/`
- 服務:`src/TextConvert/Services/`(`FileScanner`、`ConverterRegistry`、`ConversionService`)
- 測試:`src/TextConvert.Tests/`
- 設計文件:[文件轉純文字設計](../architecture/2026-07-15-文件轉純文字-design.md)

## 十、現況校正

- 1.0.0:初版,以上內容與實作一致。
- 1.0.1:修正自封裝單一檔案發布下 Serilog 無法載入 sink 組件的崩潰
  (appsettings.json 新增 `Serilog:Using`);新增 `-h`/`--help` 與 `--version`/`-v` 參數;
  新增使用者操作手冊並隨發布輸出附帶。

> 返回 [PRD 索引](README.md)
