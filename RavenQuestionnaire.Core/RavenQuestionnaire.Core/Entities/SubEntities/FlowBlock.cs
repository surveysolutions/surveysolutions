using System;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public interface IFlowBlock : IComposite
    {
        int Left { get; set; }
        int Top { get; set; }
        int Height { get; set; }
        int Width { get; set; }
        Guid QuestionId { get; set; }
    }

    public class FlowBlock : IFlowBlock
    {
        public FlowBlock()
        {
        }

        public FlowBlock(Guid questionId)
        {
            QuestionId = questionId;
        }
        public int Height { get; set; }
        public int Width { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public Guid QuestionId { get; set; }

        public bool Add(IComposite c, Guid? parent)
        {
            return false;
        }

        public bool Remove(IComposite c)
        {
            return false;
        }

        public bool Remove<T>(Guid publicKey) where T : class, IComposite
        {
            return false;
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return null;
        }
    }
}
