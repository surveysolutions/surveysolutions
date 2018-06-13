using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewHistoryDenormalizerTests
{
    internal class when_all_available_descendants_of_QuestionAnswered_recived
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    availableDescendants.AddRange(
                        assembly.GetTypes().Where(eh => typeof (QuestionAnswered).IsAssignableFrom(eh) && !eh.IsAbstract && !eh.IsInterface));
                }
                catch (ReflectionTypeLoadException)
                {
                }
            }
            BecauseOf();
        }

        private void BecauseOf() 
            {
                foreach (var eventToPublish in availableDescendants)
                {
                    var publishedEventClosedType = typeof(PublishedEvent<>).MakeGenericType(eventToPublish);
                    var handleMethod = typeof(InterviewParaDataEventHandler).GetMethod("Update", new[] { typeof(InterviewHistoryView), publishedEventClosedType });

                    if (handleMethod == null)
                        missingHandlers.Add(eventToPublish);
                }
            }

        [NUnit.Framework.Test] public void should_each_QuestionAnswered_descendant_be_handled_by_InterviewHistoryDenormalizer () =>
            missingHandlers.Should().BeEmpty();

        private static HashSet<Type> missingHandlers = new HashSet<Type>();

        private static List<Type> availableDescendants=new List<Type>();
    }
}
