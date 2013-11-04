﻿using System;
using System.Collections.Generic;
using System.Linq;

using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Main.Core.View.Question;

namespace WB.UI.Designer.Views.Questionnaire
{
    public class QuestionnaireView
    {
        private IEnumerable<ICompositeView> children;

        public QuestionnaireView(QuestionnaireDocument doc)
        {
            this.Source = doc;
        }

        public IEnumerable<ICompositeView> Children
        {
            get
            {
                return this.children
                       ?? (this.children =
                           this.Source.Children.Cast<IGroup>().Select(@group => new GroupView(@group, null, 0)).ToList());
            }
        }

        public Guid? CreatedBy
        {
            get
            {
                return this.Source.CreatedBy;
            }
        }

        public DateTime CreationDate
        {
            get
            {
                return this.Source.CreationDate;
            }
        }

        public DateTime LastEntryDate
        {
            get
            {
                return this.Source.LastEntryDate;
            }
        }

        public Guid? Parent { get; set; }

        public Guid PublicKey
        {
            get
            {
                return this.Source.PublicKey;
            }
        }

        public QuestionnaireDocument Source { get; private set; }

        public string Title
        {
            get
            {
                return this.Source.Title;
            }
        }

        public bool IsPublic
        {
            get
            {
                return this.Source.IsPublic;
            }
        }
    }
}

