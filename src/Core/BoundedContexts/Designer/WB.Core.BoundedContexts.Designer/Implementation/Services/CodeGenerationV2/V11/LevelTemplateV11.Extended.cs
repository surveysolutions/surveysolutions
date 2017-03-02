using System;
using System.Collections.Generic;
using System.Linq;
namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGenerationV2.V11
{
    public partial class LevelTemplateV11
    {
        public LevelModel Model { get; }
        public CodeGenerationModel Processor { get; }

        public LevelTemplateV11(LevelModel model, CodeGenerationModel processor)
        {
            this.Model = model;
            this.Processor = processor;
        }
    }
}
