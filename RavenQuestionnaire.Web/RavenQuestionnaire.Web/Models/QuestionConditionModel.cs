using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Main.Core.View.Question;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Web.Models
{
    public class QuestionConditionModel
    {
        public QuestionView Source { get; set; }
        public Guid TargetPublicKey { get; set; }
    }
}