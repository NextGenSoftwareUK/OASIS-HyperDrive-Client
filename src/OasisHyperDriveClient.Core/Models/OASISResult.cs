namespace OasisHyperDriveClient.Core.Models;

public class OASISResult<T>
{
    public bool IsError { get; set; }
    public bool IsWarning { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public T? Result { get; set; }
}
