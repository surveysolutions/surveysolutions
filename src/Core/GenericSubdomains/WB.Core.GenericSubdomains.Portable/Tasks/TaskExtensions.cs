using System.Threading.Tasks;

namespace WB.Core.GenericSubdomains.Portable.Tasks
{
    public static class TaskExtensions
    {
        public static void WaitAndUnwrapException(this Task task)
        {
            task.GetAwaiter().GetResult();
        }

        public static T WaitAndUnwrapException<T>(this Task<T> task)
        {
            return task.GetAwaiter().GetResult();
        }
    }
}