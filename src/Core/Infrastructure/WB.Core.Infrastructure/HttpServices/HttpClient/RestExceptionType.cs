namespace WB.Core.Infrastructure.HttpServices.HttpClient
{
    public enum RestExceptionType
    {
        Unexpected = 0,
        RequestByTimeout,
        RequestCanceledByUser,
        NoNetwork,
        HostUnreachable,
        InvalidUrl,
        UnacceptableCertificate,
        UnknownResponseSource
    }
}
