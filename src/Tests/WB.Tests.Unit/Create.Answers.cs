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
        }
    }
}