﻿using System.Linq;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public interface IAssignmentIdGenerator
    {
        int GetNextDisplayId();
    }
}
