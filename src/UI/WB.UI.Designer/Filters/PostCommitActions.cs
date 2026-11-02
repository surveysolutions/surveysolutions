using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WB.UI.Designer.Filters
{
    // Holds side effects (e.g. e-mail notifications) that must happen only if the request transaction owned by
    // TransactionFilter commits; they are discarded on rollback and never run while changes are uncommitted.
    public interface IPostCommitActions
    {
        void Enqueue(Func<Task> action);
        Task ExecuteAsync();
        void Discard();
    }

    public class PostCommitActions : IPostCommitActions
    {
        private readonly ILogger<PostCommitActions> logger;
        private readonly List<Func<Task>> pending = new();

        public PostCommitActions(ILogger<PostCommitActions> logger)
        {
            this.logger = logger;
        }

        public void Enqueue(Func<Task> action) => this.pending.Add(action);

        public async Task ExecuteAsync()
        {
            var actions = this.pending.ToArray();
            this.pending.Clear();

            foreach (var action in actions)
            {
                try
                {
                    await action();
                }
                catch (Exception exception)
                {
                    // The transaction is already committed, so a failed side effect must not fail the request.
                    this.logger.LogError(exception, "Post-commit action failed");
                }
            }
        }

        public void Discard() => this.pending.Clear();
    }
}
