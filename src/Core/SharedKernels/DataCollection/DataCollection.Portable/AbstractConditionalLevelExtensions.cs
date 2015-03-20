using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WB.Core.SharedKernels.DataCollection
{
    public partial class AbstractConditionalLevel<T> where T : IExpressionExecutable
    {
        public long CmCode(long? mo, long? year)
        {
            const long baseYear = 1900; // not sure how other calendars will be handled?? e.g. Afghanistan, Thailand, etc.
            if (year == null) return -1;
            if (mo == null) return -1;

            if (mo.Value < 1 || mo.Value > 12) return -1;
            if (year.Value < baseYear) return -1;

            return (year.Value - baseYear) * 12 + mo.Value;
        }

        /// <summary>
        /// Counts the occurrancies of a certain value in a group of single choice questions
        /// </summary>
        /// <param name="x">Specific value to be searched for.</param>
        /// <param name="singleChoiceQuestions">One or more single choice questions.</param>
        /// <returns>Number of occurrancies of the specified value or zero if it is never encountered.</returns>
        public  long CountValue(decimal x, params decimal?[] singleChoiceQuestions)
        {
            var c = 0;
            foreach (var variable in singleChoiceQuestions)
                if (variable.HasValue) if (variable.Value == x) c++;

            return c;
        }
    }
}
