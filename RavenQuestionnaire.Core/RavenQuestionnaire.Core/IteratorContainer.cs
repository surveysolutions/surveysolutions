using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Parameters;
using RavenQuestionnaire.Core.Entities.Iterators;

namespace RavenQuestionnaire.Core
{
    public class IteratorContainer : IIteratorContainer
    {
        private readonly IKernel container;

        public IteratorContainer(IKernel container)
        {
            this.container = container;
        }

        public Iterator<TOutput> Create<TDocument, TOutput>(TDocument input)
        {
            var iterator = container.Get<Iterator<TOutput>>(new ConstructorArgument("document", input));
            if (iterator == null)
                return null;
            return iterator;
        }
    }
}
