using Main.Core.Entities;

namespace Main.Core.WCF
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IAuthorizationPacket
    {
        RegisterData Data { get; set; }
        bool IsAuthorized { get; set; }
    }
}