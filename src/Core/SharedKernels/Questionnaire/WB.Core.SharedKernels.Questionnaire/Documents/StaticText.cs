using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace Main.Core.Entities.SubEntities
{
    [DebuggerDisplay("StaticText {PublicKey}")]
    public class StaticText : IStaticText
    {
        public StaticText(Guid publicKey, string text, string conditionExpression, 
            bool hideIfDisabled, IList<ValidationCondition>? validationConditions, string? attachmentName = null, List<IComposite>? children = null) 
        {

            this.PublicKey = publicKey;
            this.Text = text;
            this.AttachmentName = attachmentName ?? string.Empty;
            this.HideIfDisabled = hideIfDisabled;
            this.ConditionExpression = conditionExpression;
            this.ValidationConditions = validationConditions ?? new List<ValidationCondition>();
        }

        private readonly ReadOnlyCollection<IComposite> children = new ReadOnlyCollection<IComposite>(new List<IComposite>(0));

        public ReadOnlyCollection<IComposite> Children
        {
            get => children;
            set { }
        }

        private IComposite? parent;

        public IComposite? GetParent()
        {
            return this.parent;
        }

        public void SetParent(IComposite? parent)
        {
            this.parent = parent;
        }

        public T? Find<T>(Guid publicKey) where T : class, IComposite
        {
            return null;
        }

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return new T[0];
        }

        public T? FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            return null;
        }

        public void ConnectChildrenWithParent()
        {
        }

        public IComposite Clone()
        {
            var staticText = this.MemberwiseClone() as IStaticText;
            if (staticText == null)
                throw new InvalidOperationException($"Cloned object is not {nameof(IStaticText)}");
            staticText.SetParent(null);
            
            staticText.ValidationConditions = new List<ValidationCondition>(this.ValidationConditions.Select(x => x.Clone()));

            return staticText;
        }

        public void Insert(int index, IComposite itemToInsert, Guid? parent)
        {
            
        }

        public void RemoveChild(Guid child)
        {
        }

        public string VariableName => String.Empty;

        public Guid PublicKey { get; set; }

        public string Text { get; set; }

        public string AttachmentName { get; set; }

        public IList<ValidationCondition> ValidationConditions { get; set; }
        public string ConditionExpression { get; set; }
        public bool HideIfDisabled { get; set; }
    }
}
