namespace WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading
{
    public class PreloadedDataDto
    {
        public PreloadedDataDto(PreloadedLevelDto[] data)
        {
            this.Data = data;
        }
        public PreloadedLevelDto[] Data { get; private set; }
    }
}
