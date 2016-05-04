﻿using System;
using Main.Core.Documents;
using Ncqrs.Commanding;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    public class CloneQuestionnaire : CommandBase
    {
        public CloneQuestionnaire(Guid publicKey, string title, Guid createdBy, bool isPublic, IQuestionnaireDocument doc)
            : base(publicKey)
        {
            this.PublicKey = publicKey;
            this.CreatedBy = createdBy;
            this.Title = CommandUtils.SanitizeHtml(title);
            this.IsPublic = isPublic;
            this.Source = doc;
        }

        public string Title { get; private set; }

        public bool IsPublic { get; private set; }

        public Guid CreatedBy { get; private set; }

        public Guid PublicKey { get; private set; }

        public IQuestionnaireDocument Source { get; private set; }
    }
}