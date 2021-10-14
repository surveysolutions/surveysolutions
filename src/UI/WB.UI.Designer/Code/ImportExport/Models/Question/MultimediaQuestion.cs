using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;

namespace WB.UI.Designer.Code.ImportExport.Models.Question
{
    public class MultimediaQuestion : AbstractQuestion
    {
        public override QuestionType QuestionType
        {
            get => QuestionType.Multimedia;
            set { }
        }

        public bool IsSignature { get; set; }
    }
}
