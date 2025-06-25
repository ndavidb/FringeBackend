namespace Fringe.Domain.Entities;

public class FileUploadModel
{
    public string Path { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
}