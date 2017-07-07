namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public enum RestExceptionType
    {
        Unexpected = 0,
        RequestByTimeout,
        RequestCanceledByUser,
        NoNetwork,
        HostUnreachable,
        InvalidUrl,
        UnacceptableCertificate
    }
}