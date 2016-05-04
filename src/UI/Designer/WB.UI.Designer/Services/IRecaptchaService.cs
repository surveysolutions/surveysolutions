namespace WB.UI.Designer.Services
{
    public interface IRecaptchaService
    {
        bool IsValid(string clientResponse);
    }
}