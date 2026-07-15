namespace TextConvert.Converters;

/// <summary>
/// 文件轉換器介面。每個實作負責將某一類檔案格式擷取成純文字內容。
/// </summary>
public interface IDocumentConverter
{
    /// <summary>
    /// 判斷此轉換器是否能處理指定副檔名(小寫、含點,例如 ".docx")。
    /// </summary>
    bool CanHandle(string extension);

    /// <summary>
    /// 將指定檔案擷取為純文字內容。
    /// </summary>
    string Convert(string filePath);
}
