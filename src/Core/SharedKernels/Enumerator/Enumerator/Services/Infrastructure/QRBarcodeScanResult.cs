namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public class QRBarcodeScanResult
    {
        public string Code { get; set; }
        public byte[] RawBytes { get; set; }
    }
}