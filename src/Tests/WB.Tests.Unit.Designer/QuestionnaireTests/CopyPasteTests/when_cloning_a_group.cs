using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.CopyPasteTests
{
    [Subject(typeof(Group))]
    internal class when_cloning_a_fixed_roster
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            original = Create.FixedRoster(fixedTitles: new List<string> {"title1"});
            clone = (Group)original.Clone();
            BecauseOf();
        }

        private void BecauseOf() => clone.FixedRosterTitles[0].Title = "changed";

        [NUnit.Framework.Test] public void should_not_change_original_fixed_roster_title_when_chaning_title_in_clone () => original.FixedRosterTitles[0].Title.ShouldEqual("title1");

        static Group original;
        static Group clone;
    }
}