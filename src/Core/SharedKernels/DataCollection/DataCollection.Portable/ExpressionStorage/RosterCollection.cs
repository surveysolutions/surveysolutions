using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.ExpressionStorage
{
    public class RostersCollection<T> : IEnumerable<T> where T : class, IInterviewLevel
    {
        private readonly List<T> rosters;

        public RostersCollection(IEnumerable<IInterviewLevel> rosters)
        {
            this.rosters = rosters?.OrderBy(x => x.RowIndex).Select(x => x as T).ToList() ?? new List<T>();
        }
        public IEnumerator<T> GetEnumerator()
        {
            return rosters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T this[int code] => rosters.SingleOrDefault(x => x.RowCode == code);

        public T ByIndex(int index)
        {
            return rosters[index];
        }
    }
}
