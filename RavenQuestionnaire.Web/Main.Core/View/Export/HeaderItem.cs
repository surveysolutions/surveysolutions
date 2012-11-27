// -----------------------------------------------------------------------
// <copyright file="HeaderItem.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace Main.Core.View.Export
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class HeaderItem
    {
        public HeaderItem(IQuestion question)
        {
            PublicKey = question.PublicKey;
            Caption = string.IsNullOrEmpty(question.StataExportCaption)
                          ? question.QuestionText
                          : question.StataExportCaption;
            Title = question.QuestionText;
            this.Labels=new Dictionary<Guid, LabelItem>();
            foreach (IAnswer answer in question.Answers)
            {
                this.Labels.Add(answer.PublicKey, new LabelItem(answer));
            }
        }

        public Guid PublicKey { get; private set; }
        public string Caption { get; private set; }
        public string Title { get; private set; }
        public Dictionary<Guid, LabelItem> Labels { get; private set; }

    }

    public class LabelItem
    {
        public LabelItem(IAnswer answer)
        {
            PublicKey = answer.PublicKey;
            Caption = answer.AnswerValue == null ? answer.AnswerText : answer.AnswerValue.ToString();
            Title = answer.AnswerText;
        }

        public Guid PublicKey { get; private set; }
        public string Caption { get; private set; }
        public string Title { get; private set; }
    }
}
