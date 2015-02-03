using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;

namespace WB.Tests.CleanIntegration.EsentTests
{
    [Ignore("Don't know what to do here yet.")]
    internal class when_multiple_tasks_are_updating_entity : with_esent_store<TestStoredEntity>
    {
        Establish context = () =>
        {
            taskCount = 1000;
            completedTaskCount = 0;
            tasks = new Task[taskCount];
            var entity = new TestStoredEntity();
            storage.Store(entity, entityId);

            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    var storedEntity = storage.GetById(entityId);
                    storedEntity.IntegerProperty++;
                    storage.Store(storedEntity, entityId);
                    Interlocked.Increment(ref completedTaskCount);
                });
            }
        };

        Because of = () => Task.WaitAll(tasks);

        It should_handle_concurrent_updates = () => storage.GetById(entityId).IntegerProperty.ShouldEqual(taskCount);

        It should_wait_for_all_tasks_to_complete = () => completedTaskCount.ShouldEqual(1000);

        static Task[] tasks;
        static string entityId = "Id";
        static int taskCount;
        private static int completedTaskCount;
    } 
}