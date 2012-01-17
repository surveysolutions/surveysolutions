using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Web.Models
{
    public class QuestionConditionModel
    {
        public QuestionView Source { get; set; }
        public QuestionView Target { get; set; }
    }
}