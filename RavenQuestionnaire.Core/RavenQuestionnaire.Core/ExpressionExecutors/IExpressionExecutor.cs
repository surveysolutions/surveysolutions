namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    public interface IExpressionExecutor<in TInput, out TOutput>
    {
        TOutput Execute(TInput entity, string condition);
    }
}
