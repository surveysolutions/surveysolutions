using System;

namespace WB.Core.Infrastructure.WriteSide
{
    public interface IWriteSideCleaner
    {
        void Clean(Guid aggregateId);
    }
}