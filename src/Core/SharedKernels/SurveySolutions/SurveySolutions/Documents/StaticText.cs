﻿using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;

namespace Main.Core.Entities.SubEntities
{
    public class StaticText : IStaticText
    {
        public StaticText(Guid publicKey, string text)
        {
            this.PublicKey = publicKey;
            this.Text = text;
        }

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
            var staticText = this.MemberwiseClone() as IStaticText;

            staticText.SetParent(null);

            return staticText;
        }

        public Guid PublicKey { get; private set; }

        public string Text { get; set; }
    }
}
