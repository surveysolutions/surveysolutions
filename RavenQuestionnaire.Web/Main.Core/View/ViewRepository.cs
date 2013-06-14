using Ninject;

namespace Main.Core.View
{
    public class ViewRepository : IViewRepository
    {
        private readonly IKernel container;

        public ViewRepository(IKernel container)
        {
            this.container = container;
        }

        public TOutput Load<TInput, TOutput>(TInput input)
        {
            var factory = this.container.TryGet<IViewFactory<TInput, TOutput>>();
            if (factory == null)
            {
                return default(TOutput);
            }

            return factory.Load(input);
        }
    }
}