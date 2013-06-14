namespace Main.Core.View
{
    public interface IViewRepository
    {
        TOutput Load<TInput, TOutput>(TInput input);
    }
}