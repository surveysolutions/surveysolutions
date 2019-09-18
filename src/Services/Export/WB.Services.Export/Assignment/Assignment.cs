using System;

namespace WB.Services.Export.Assignment
{
    public class Assignment
    {
        public int Id { get; set; }
        public Guid PublicKey { get; set; }
        public Guid ResponsibleId { get; set; }
    }
}
