namespace ApiUtil
{
    public enum RestExceptionType
    {
        Unexpected = 0,
        RequestByTimeout,
        RequestCanceledByUser,
        NoNetwork,
        HostUnreachable,
        InvalidUrl
    }
}