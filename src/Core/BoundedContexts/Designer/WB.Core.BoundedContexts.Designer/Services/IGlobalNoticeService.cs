namespace WB.Core.BoundedContexts.Designer.Services;

public interface IGlobalNoticeService
{
    string? GetMessage();
    void SetMessage(string? message);
}
