﻿using System;

namespace Main.Core.Entities.SubEntities
{
    /// <summary>
    /// The answer.
    /// </summary>
    public class Answer : IAnswer
    {
        public Answer(/*Question owner*/)
        {
            this.PublicKey = Guid.NewGuid();
        }

        public string AnswerImage { get; set; }

        public string AnswerText { get; set; }

        public AnswerType AnswerType { get; set; }

        public string AnswerValue { get; set; }
       
        public bool Mandatory { get; set; }

        public string NameCollection { get; set; }
        
        public Guid PublicKey { get; set; }

        #region Implementation of ICloneable

        public Answer Clone()
        {
            return this.MemberwiseClone() as Answer;
        }

        #endregion

        public static Answer CreateFromOther(IAnswer answer)
        {
            return new Answer
            {
                AnswerImage = answer.AnswerImage,
                AnswerText = answer.AnswerText,
                AnswerType = answer.AnswerType,
                AnswerValue = answer.AnswerValue,
                Mandatory = answer.Mandatory,
                NameCollection = answer.NameCollection,
                PublicKey = answer.PublicKey,
            };
        }
    }
}