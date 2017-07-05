namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface IInterviewAnswerSerializer
    {
        string Serialize(object item);

        T Deserialize<T>(string payload);
    }
}