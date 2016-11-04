using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Merger
{
    internal class when_merging_questionnaire_and_interview_data_with_static_text_having_attachment : InterviewDataAndQuestionnaireMergerTestContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new Group(nestedGroupTitle)
                {
                    PublicKey = nestedGroupId,
                    Children =
                        new List<IComposite>()
                        {
                            Create.Entity.StaticText(publicKey: staticTextId, text: staticText, attachmentName: attachmentName)
                        }.ToReadOnlyCollection()
                });
            questionnaire.Attachments = new List<Attachment>() {new Attachment() {ContentId = attachmentContentId, Name = attachmentName } };

            interview = CreateInterviewData(interviewId);
            
            user = Mock.Of<UserDocument>();

            merger = CreateMerger(questionnaire);

            attachmentInfos = new List<AttachmentInfoView>() { new AttachmentInfoView(attachmentContentId, attachmentType) };

        };

        Because of = () =>
            mergeResult = merger.Merge(interview, questionnaire, user.GetUseLight(), null, attachmentInfos);

        It should_static_text_exist= () =>
            GetStaticText().ShouldNotBeNull();

        It should_static_text_have_text = () =>
            GetStaticText().Text.ShouldEqual(staticText);

        It should_static_text_attachment_content_type = () =>
            GetStaticText().Attachment.ContentType.ShouldEqual(attachmentType);

        It should_static_text_attachment_content_id = () =>
            GetStaticText().Attachment.ContentId.ShouldEqual(attachmentContentId);

        private static InterviewGroupView GetNestedGroup()
        {
            return mergeResult.Groups.Find(g => g.Id == nestedGroupId);
        }

        private static InterviewStaticTextView GetStaticText()
        {
            return GetNestedGroup().Entities.OfType<InterviewStaticTextView>().FirstOrDefault(q => q.Id == staticTextId);
        }

        private static InterviewDataAndQuestionnaireMerger merger;
        private static InterviewDetailsView mergeResult;
        private static InterviewData interview;
        private static QuestionnaireDocument questionnaire;
        private static UserDocument user;
        private static Guid nestedGroupId = Guid.Parse("11111111111111111111111111111111");
        private static Guid staticTextId = Guid.Parse("55555555555555555555555555555555");
        private static Guid interviewId = Guid.Parse("33333333333333333333333333333333");
        private static string nestedGroupTitle = "nested Group";
        private static string staticText = "static text";
        private static string attachmentName = "test1";

        private static string attachmentType = "img";
        private static string attachmentContentId = "DTGHRHFJFJFJDD";

        private static List<AttachmentInfoView> attachmentInfos;
    }
}
