namespace WB.Core.SharedKernels.DataCollection.Views.BinaryData
{
    public class Attachment
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public ContentType Type { get; set; }

        public int Size { get; set; }
    }

    public enum ContentType
    {
        Image,
    }
}