using System;

namespace Main.Core.Entities.SubEntities
{
    public struct Image
    {
        public DateTime CreationDate { get; set; }

        public string Description { get; set; }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }
    }
}