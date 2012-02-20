using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Observers;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Tests.Entities.Observers
{
    [TestFixture]
    public class CompositeHandlerTest
    {
        [Test]
        public void SubscribeOnGroupAdd_CorrectData_GroupIsAdded()
        {
            /*CompleteQuestionnaireDocument document = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire questionnaire = new CompleteQuestionnaire(new CompleteQuestionnaireDocument(), null);*/

            Mock<IComposite> document = new Mock<IComposite>();
            CompleteGroup baseGroup = new CompleteGroup("target");
            PropagatableCompleteGroup target = new PropagatableCompleteGroup(baseGroup, Guid.NewGuid());

            CompleteGroup targetGroup = new CompleteGroup("some group") { Propagated = Propagate.Propagated };
            document.Setup(x => x.Find<CompleteGroup>(targetGroup.PublicKey)).Returns(targetGroup);
            GroupObservable observeble = new GroupObservable(targetGroup.PublicKey, target.PublicKey);
            var observers = new List<IObserver<CompositeInfo>> {observeble};

            CompositeHandler handler = new CompositeHandler(observers,document.Object);
            handler.Add(target);
            document.Verify(
                x => x.Add(It.Is<PropagatableCompleteGroup>(g => g.PublicKey.Equals(targetGroup.PublicKey)), null),
                Times.Once());
        }
    }
}
