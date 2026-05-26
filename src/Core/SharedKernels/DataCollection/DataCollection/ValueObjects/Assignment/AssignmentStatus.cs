namespace WB.Core.SharedKernels.DataCollection.ValueObjects.Assignment
{
    public enum AssignmentStatus
    {
        /// <summary>Active assignment available for interviewer work</summary>
        Open = 0,
        /// <summary>Assignment marked as complete by the interviewer</summary>
        Completed = 1,
        /// <summary>Assignment closed (downsized) by supervisor</summary>
        Closed = 2,
    }
}
