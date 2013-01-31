using System;
using System.Collections.Generic;
using System.Text;
using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails;
using NUnit.Framework;

namespace AndroidApp.Core.Model.Tests
{
    [TestFixture]
    public class CompleteQuestionnaireViewTests
    {
        [Test]
        public void Testt()
        {
            CompleteQuestionnaireView target = new CompleteQuestionnaireView(Guid.NewGuid().ToString());
        }
    }
}
