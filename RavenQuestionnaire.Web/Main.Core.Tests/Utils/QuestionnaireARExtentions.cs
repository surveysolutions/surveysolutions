using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;

namespace Main.Core.Tests.Utils
{
    public static class QuestionnaireARExtentions
    {
        public static Group AddChapter(this QuestionnaireAR questionnaire, Guid? chapterId = null)
        {
            var groupId = chapterId.HasValue ? chapterId.Value : Guid.NewGuid();
            var document = questionnaire.CreateSnapshot();
            return document.AddChapter(groupId);
        }
    }
}
