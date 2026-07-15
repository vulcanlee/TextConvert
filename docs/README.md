# TextConvert 文件總索引

- 文件用途:作為 TextConvert 專案所有說明文件的入口與導覽。
- 主要讀者:開發者、維運人員、使用者。
- 文件版本:1.0
- 對應系統版本:1.0.0
- 建立日期:2026-07-15
- 編碼:UTF-8 繁體中文

## 一、專案簡介

TextConvert 是一個以 .NET 10 / C# 開發的 Console CLI 工具,能將指定來源目錄下的
Office(Word、PowerPoint、Excel)、PDF、Markdown、Text、HTML、CSV、JSON、XML 檔案,
批次轉換成純文字內容,輸出到指定目錄,供知識庫處理之用。不符合條件的檔案會自動跳過。

## 二、文件分類

沿用英文小寫分類目錄慣例,每個分類目錄下皆有一份 `README.md` 作為該分類索引。

| 目錄 | 收納內容 |
|------|----------|
| [`prd/`](prd/README.md) | 產品需求文件(功能需求、範圍、驗收) |
| [`architecture/`](architecture/README.md) | 系統架構與設計文件 |
| [`changelog/`](changelog/README.md) | 改版與變更紀錄 |
| [`prompts/`](prompts/) | 開發過程使用的需求提示詞原稿 |

> 若未來新增內容無適用分類,請自建語意明確的英文小寫分類目錄,並同步更新本索引。

## 三、快速連結

- 產品需求:[文字轉換 PRD](prd/文字轉換-prd.md)
- 系統設計:[文件轉純文字設計](architecture/2026-07-15-文件轉純文字-design.md)
- 最新變更:[初版建立](changelog/2026-07-15-初版建立.md)
- 使用說明:[專案 README](../README.md)
