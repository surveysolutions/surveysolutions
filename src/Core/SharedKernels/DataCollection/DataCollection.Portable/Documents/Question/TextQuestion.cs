﻿using System;
using System.Collections.Generic;

namespace Main.Core.Entities.SubEntities.Question
{
    public class TextQuestion : AbstractQuestion
    {
        public TextQuestion(string text)
            : base(text)
        {
        }

        public TextQuestion()
        {
        }

        public override void AddAnswer(Answer answer)
        {
            throw new NotImplementedException();
        }
    
        public override T Find<T>(Guid publicKey)
        {
            return null;
        }

        public override IEnumerable<T> Find<T>(Func<T, bool> condition)
        {
            return new T[0];
        }
     
        public override T FirstOrDefault<T>(Func<T, bool> condition)
        {
            return null;
        }

        public string Mask { get; set; }
    }
}