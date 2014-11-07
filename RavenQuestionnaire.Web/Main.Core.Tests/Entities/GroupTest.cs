using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using RavenQuestionnaire.Core.Tests.Utils;

namespace Main.Core.Tests.Entities
{
    [TestFixture]
    public class GroupTest
    {
        [Test]
        public void ClonedGroup_TargetIsProperlySet()
        {
            var children = new List<IComposite> { new NumericQuestion("n1"), new TextListQuestion("t1") };
            
            var group = new Group("group")
            {
                Children = children,
                ConditionExpression = "expr",
                Description = "desc",
                Enabled = true,
                IsRoster = true,
                Propagated = Propagate.AutoPropagated,
                PublicKey = Guid.NewGuid(),
                RosterFixedTitles = new[] { "t1", "t2" },
                RosterTitleQuestionId=Guid.NewGuid(),
                RosterSizeSource=RosterSizeSourceType.Question,
                RosterSizeQuestionId=Guid.NewGuid()
            };

            var target = group.Clone() as Group;
            PropertyInfo[] propertiesForCheck = typeof(IGroup).GetPublicPropertiesExcept("Children");
            foreach (PropertyInfo publicProperty in propertiesForCheck)
            {
                Assert.AreEqual(publicProperty.GetValue(group, null), publicProperty.GetValue(target, null));
            }

            Assert.AreEqual(group.Children.Count, target.Children.Count);
            for (int i = 0; i < group.Children.Count; i++)
            {
                var answer = target.Children.FirstOrDefault(q => q.PublicKey == group.Children[i].PublicKey);
                Assert.IsTrue(answer != null);


                Assert.IsTrue(!answer.Equals(group.Children[i])); // they are interfaces and Equals uses Reference equality
            }
        }
    }
}