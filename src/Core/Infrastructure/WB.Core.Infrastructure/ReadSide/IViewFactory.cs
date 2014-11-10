namespace WB.Core.Infrastructure.ReadSide
{
    public interface IViewFactory<TInput, TOutput>
    {
        TOutput Load(TInput input);
    }
}