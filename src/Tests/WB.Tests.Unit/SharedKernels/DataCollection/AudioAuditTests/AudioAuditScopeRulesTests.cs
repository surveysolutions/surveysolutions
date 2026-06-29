using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer;

namespace WB.Tests.Unit.SharedKernels.DataCollection.AudioAuditTests
{
    [TestOf(typeof(AudioAuditScopeRules))]
    public class AudioAuditScopeRulesTests
    {
        private static readonly Guid SelectedGroup = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid UnselectedGroup = Guid.Parse("22222222-2222-2222-2222-222222222222");

        [Test]
        public void when_audio_audit_enabled_should_record_whole_interview_ignoring_scope()
        {
            Assert.That(AudioAuditScopeRules.ShouldRecord(true, Array.Empty<Guid>(), UnselectedGroup), Is.True);
            Assert.That(AudioAuditScopeRules.ShouldRecord(true, new[] { SelectedGroup }, UnselectedGroup), Is.True);
            Assert.That(AudioAuditScopeRules.ShouldRecord(true, null, null), Is.True);
        }

        [Test]
        public void when_audio_audit_disabled_and_scope_empty_should_not_record()
        {
            Assert.That(AudioAuditScopeRules.ShouldRecord(false, Array.Empty<Guid>(), SelectedGroup), Is.False);
            Assert.That(AudioAuditScopeRules.ShouldRecord(false, null, SelectedGroup), Is.False);
            Assert.That(AudioAuditScopeRules.ShouldRecord(null, Array.Empty<Guid>(), SelectedGroup), Is.False);
        }

        [Test]
        public void when_audio_audit_disabled_and_entity_in_scope_should_record()
        {
            Assert.That(AudioAuditScopeRules.ShouldRecord(false, new[] { SelectedGroup }, SelectedGroup), Is.True);
        }

        [Test]
        public void when_audio_audit_disabled_and_entity_not_in_scope_should_not_record()
        {
            // selected parent does not implicitly select an unselected nested entity
            Assert.That(AudioAuditScopeRules.ShouldRecord(false, new[] { SelectedGroup }, UnselectedGroup), Is.False);
        }

        [Test]
        public void when_returning_from_unselected_child_to_selected_parent_should_record_again()
        {
            var scope = new[] { SelectedGroup };

            // entered selected parent -> records
            Assert.That(AudioAuditScopeRules.ShouldRecord(false, scope, SelectedGroup), Is.True);
            // navigated into unselected nested entity -> stops
            Assert.That(AudioAuditScopeRules.ShouldRecord(false, scope, UnselectedGroup), Is.False);
            // returned to selected parent -> records again
            Assert.That(AudioAuditScopeRules.ShouldRecord(false, scope, SelectedGroup), Is.True);
        }
    }
}
