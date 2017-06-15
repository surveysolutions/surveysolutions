using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AutoMapper;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.UI.Headquarters.Code;
using Main.Core.Entities.SubEntities;
using NHibernate.Util;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.API.Interviewer.v2
{
    public class AssignmentsApiV2Controller : ApiController
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IAssignmentsService assignmentsService;
        private readonly IInterviewAnswerSerializer answerSerializer;
        private readonly IMapper autoMapper;

        public AssignmentsApiV2Controller(IAuthorizedUser authorizedUser,
            IAssignmentsService assignmentsService,
            IMapper autoMapper, 
            IInterviewAnswerSerializer answerSerializer)
        {
            this.authorizedUser = authorizedUser;
            this.assignmentsService = assignmentsService;
            this.autoMapper = autoMapper;
            this.answerSerializer = answerSerializer;
        }

        [WriteToSyncLog(SynchronizationLogType.GetAssignments)]
        [ApiBasicAuth(UserRoles.Interviewer)]
        [HttpGet]
        public List<AssignmentApiView> List()
        {
            var authorizedUserId = this.authorizedUser.Id;
            var assignments = this.assignmentsService.GetAssignments(authorizedUserId);

            List<AssignmentApiView> assignmentApiViews = new List<AssignmentApiView>();
            foreach (var assignment in assignments)
            {
                var assignmentApiView = new AssignmentApiView
                {
                    Id = assignment.Id,
                    QuestionnaireId = assignment.QuestionnaireId,
                    Quantity = assignment.Quantity,
                    InterviewsCount = assignment.InterviewSummaries.Count
                }; //this.autoMapper.Map<Assignment, AssignmentApiView>(assignment);

                var assignmentIdentifyingData = assignment.IdentifyingData.ToList();

                foreach (var answer in assignment.Answers)
                {
                    var serializedAnswer = new AssignmentApiView.InterviewSerializedAnswer
                    {
                        Identity = answer.Identity,
                        SerializedAnswer = this.answerSerializer.Serialize(answer.Answer)
                    };

                    var identifyingAnswer = assignmentIdentifyingData.FirstOrDefault(x => x.Identity == answer.Identity);
                    if (identifyingAnswer!=null)
                    {
                        if (answer.Answer is GpsAnswer)
                        {
                            var gpsAnswer = answer.Answer as GpsAnswer;
                            assignmentApiView.LocationLatitude = gpsAnswer.Value.Latitude;
                            assignmentApiView.LocationLongitude = gpsAnswer.Value.Latitude;
                        }
                        serializedAnswer.AnswerAsString = identifyingAnswer.AnswerAsString;
                    }

                    assignmentApiView.Answers.Add(serializedAnswer);
                }
              
                assignmentApiViews.Add(assignmentApiView);
            }

            return assignmentApiViews;
        }
    }
}