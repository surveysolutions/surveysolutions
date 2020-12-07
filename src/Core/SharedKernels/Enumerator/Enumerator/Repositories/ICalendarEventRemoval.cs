using System;

namespace WB.Core.SharedKernels.Enumerator.Repositories
{
    public interface ICalendarEventRemoval
    {
        void Remove(Guid id);
    }
}