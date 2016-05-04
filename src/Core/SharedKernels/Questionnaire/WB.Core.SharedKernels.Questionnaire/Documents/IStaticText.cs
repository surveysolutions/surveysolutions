using System;
using Main.Core.Entities.Composite;

namespace Main.Core.Entities.SubEntities
{
    public interface IStaticText : IComposite
    {
        string Text { get; set; }
        string AttachmentName { get; set; }
    }
}