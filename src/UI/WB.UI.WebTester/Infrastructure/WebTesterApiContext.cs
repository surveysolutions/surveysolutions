using System;
using System.Threading;

namespace WB.UI.WebTester.Infrastructure
{
    /// <summary>
    /// Ambient async-local context that carries the current questionnaire ID
    /// through an async call chain so that <see cref="DesignerJwtAuthHandler"/>
    /// can attach the correct JWT without inspecting the request URL.
    /// </summary>
    public static class WebTesterApiContext
    {
        private static readonly AsyncLocal<Guid?> CurrentQuestionnaireId = new();

        public static Guid? Current => CurrentQuestionnaireId.Value;

        public static IDisposable Use(Guid questionnaireId)
        {
            var previous = CurrentQuestionnaireId.Value;
            CurrentQuestionnaireId.Value = questionnaireId;
            return new ContextScope(() => CurrentQuestionnaireId.Value = previous);
        }

        private sealed class ContextScope : IDisposable
        {
            private readonly Action restore;
            private bool disposed;

            public ContextScope(Action restore) => this.restore = restore;

            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;
                    restore();
                }
            }
        }
    }
}
