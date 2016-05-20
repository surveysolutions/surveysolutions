using System;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;

namespace WB.Tests.Unit
{
    internal static partial class Create
    {
        internal static class Answers
        {
            public static LinkedSingleOptionAnswer LinkedSingleOptionAnswer(RosterVector answer, Guid? id = null)
            {
                var model = new LinkedSingleOptionAnswer(id ?? Guid.NewGuid(), new decimal[0]);
                model.SetAnswer(answer);
                return model;
            }

            public static IntegerNumericAnswer IntegerNumericAnswer(Identity answerIdentity = null, int answer = 42)
            {
                answerIdentity = answerIdentity ?? Create.Other.Identity(Guid.NewGuid(), Core.SharedKernels.DataCollection.RosterVector.Empty);
                var model = new IntegerNumericAnswer(answerIdentity.Id, answerIdentity.RosterVector);

                model.SetAnswer(answer);
                return model;
            }

            public static RealNumericAnswer RealNumericAnswer(Identity answerIdentity = null, decimal answer = 42.42m)
            {
                answerIdentity = answerIdentity ?? Create.Other.Identity(Guid.NewGuid(), Core.SharedKernels.DataCollection.RosterVector.Empty);
                var model = new RealNumericAnswer(answerIdentity.Id, answerIdentity.RosterVector);

                model.SetAnswer(answer);
                return model;
            }
        }
    }
}