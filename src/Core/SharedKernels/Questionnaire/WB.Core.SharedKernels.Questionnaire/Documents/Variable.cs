using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;

namespace WB.Core.SharedKernels.QuestionnaireEntities
{
    public class Variable : IVariable
    {
        public Variable(Guid publicKey, VariableData variableData)
        {
            this.PublicKey = publicKey;
            if (variableData != null)
            {
                this.Type = variableData.Type;
                this.Name = variableData.Name;
                this.Expression = variableData.Expression;
                this.Description = variableData.Description;
            }
        }

        public string Description { get; set; }
        public Guid PublicKey { get; set; }
        public VariableType Type { get; set; }
        public string Name { get; set; }
        public string Expression { get; set; }

        public List<IComposite> Children
        {
            get { return new List<IComposite>(0); }
            set { }
        }

        private IComposite parent;

        public IComposite GetParent()
        {
            return this.parent;
        }

        public void SetParent(IComposite parent)
        {
            this.parent = parent;
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return null;
        }

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return new T[0];
        }

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            return null;
        }

        public void ConnectChildrenWithParent()
        {
        }

        public IComposite Clone()
        {
            var variable = (IVariable)this.MemberwiseClone();
            variable.SetParent(null);
            return variable;
        }
    }
}