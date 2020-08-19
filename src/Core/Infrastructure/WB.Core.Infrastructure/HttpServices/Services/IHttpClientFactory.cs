namespace WB.Core.Infrastructure.HttpServices.Services
{
    public interface IHttpClientFactory
    {
        System.Net.Http.HttpClient CreateClient(IHttpStatistician httpStatistician);
    }
}
