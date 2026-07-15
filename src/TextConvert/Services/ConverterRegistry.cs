using TextConvert.Converters;

namespace TextConvert.Services;

/// <summary>
/// 轉換器登錄。依副檔名解析出可處理的轉換器。
/// </summary>
public sealed class ConverterRegistry
{
    private readonly IReadOnlyList<IDocumentConverter> _converters;

    public ConverterRegistry(IEnumerable<IDocumentConverter> converters)
    {
        _converters = converters.ToList();
    }

    /// <summary>
    /// 取得能處理指定檔案的轉換器;若無則回傳 null。
    /// </summary>
    public IDocumentConverter? Resolve(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(extension))
        {
            return null;
        }

        return _converters.FirstOrDefault(c => c.CanHandle(extension));
    }

    /// <summary>
    /// 是否有轉換器能處理此檔案。
    /// </summary>
    public bool IsSupported(string filePath) => Resolve(filePath) is not null;
}
