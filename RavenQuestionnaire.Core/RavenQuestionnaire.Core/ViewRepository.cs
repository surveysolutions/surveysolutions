using Ninject;

namespace RavenQuestionnaire.Core
{
    public class ViewRepository : IViewRepository
    {
        private IKernel container;

        public ViewRepository(IKernel container)
        {
            this.container = container;
        }

        public TOutput Load<TInput, TOutput>(TInput input)
        {
            var factory = container.TryGet<IViewFactory<TInput, TOutput>>();
            if (factory == null) 
                return default(TOutput);
            return factory.Load(input);
        }
    }
}
