using System;

namespace WB.Core.Infrastructure.CommandBus
{
    public class RequiresPreprocessorAttribute : Attribute
    {
        public Type PreProcessor { get; }

        public RequiresPreprocessorAttribute(Type preProcessor)
        {
            PreProcessor = preProcessor;
        }
    }
}
