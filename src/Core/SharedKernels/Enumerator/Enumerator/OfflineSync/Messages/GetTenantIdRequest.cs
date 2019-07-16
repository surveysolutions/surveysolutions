namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class GetTenantIdRequest : ICommunicationMessage
    {
        
    }

    public class GetTenantIdResponse : ICommunicationMessage
    {
        public string TenantId { get; set; }
    }
}
