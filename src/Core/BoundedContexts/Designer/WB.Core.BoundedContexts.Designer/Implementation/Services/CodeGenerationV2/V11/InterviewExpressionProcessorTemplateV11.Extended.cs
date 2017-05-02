﻿using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGenerationV2.V11
{
    public partial class InterviewExpressionProcessorTemplateV11
    {
        public InterviewExpressionProcessorTemplateV11(CodeGenerationModel model)
        {
            this.Model = model;
        }

        public CodeGenerationModel Model { get; private set; }

        protected LevelTemplateV11 CreateLevelTemplate(LevelModel level, CodeGenerationModel model)
        {
            return new LevelTemplateV11(level, model);
        }
    }
}