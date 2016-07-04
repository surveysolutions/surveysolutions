﻿using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;

namespace Main.Core.Entities.SubEntities.Question
{
    /// <summary>
    /// Single option question
    /// </summary>
    public class SingleQuestion : AbstractQuestion
    {
        public SingleQuestion()
        {
            this.Children = new List<IComposite>();
        }

        public SingleQuestion(Guid qid, string text) : base(text)
        {
            this.PublicKey = qid;
            this.Children = new List<IComposite>();
        }

        public override void AddAnswer(Answer answer)
        {
            if (answer == null)
            {
                return;
            }

            this.Answers.Add(answer);
        }

        public override T Find<T>(Guid publicKey)
        {
            return null;
        }

        public override IEnumerable<T> Find<T>(Func<T, bool> condition)
        {
            return Enumerable.Empty<T>();
        }

        public override T FirstOrDefault<T>(Func<T, bool> condition)
        {
            return null;
        }
    }
}