using System;

namespace WB.Core.Infrastructure.Modularity
{
    public struct ConstructorArgument
    {
        public ConstructorArgument(string name, Func<IModuleContext, object> value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public Func<IModuleContext, object> Value { get; set; }
    }
}