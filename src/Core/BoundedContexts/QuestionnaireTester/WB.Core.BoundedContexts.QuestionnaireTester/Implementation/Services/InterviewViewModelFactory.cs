using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
{
    internal class InterviewViewModelFactory : IInterviewViewModelFactory
    {
        private readonly IPlainRepository<QuestionnaireModel> plainQuestionnaireRepository;
        private readonly IPlainRepository<InterviewModel> plainStorageInterviewAccessor;

        private static readonly Dictionary<Type, Func<BaseInterviewItemViewModel>> QuestionnaireEntityTypeToViewModelMap = 
            new Dictionary<Type, Func<BaseInterviewItemViewModel>>
            {
                { typeof(StaticTextModel),                 Load<StaticTextViewModel>                 },
                { typeof(GroupModel),                      Load<GroupReferenceViewModel>             },
                { typeof(RosterModel),                     Load<RostersReferenceViewModel>           },
                // questions
                { typeof(SingleOptionQuestionModel),       Load<SingleOptionQuestionViewModel>       },
                { typeof(LinkedSingleOptionQuestionModel), Load<LinkedSingleOptionQuestionViewModel> },
                { typeof(MultiOptionQuestionModel),        Load<MultiOptionQuestionViewModel>        },
                { typeof(LinkedMultiOptionQuestionModel),  Load<LinkedMultiOptionQuestionViewModel>  },
                { typeof(IntegerNumericQuestionModel),     Load<IntegerNumericQuestionViewModel>     },
                { typeof(RealNumericQuestionModel),        Load<RealNumericQuestionViewModel>        },
                { typeof(MaskedTextQuestionModel),         Load<MaskedTextQuestionViewModel>         },
                { typeof(TextListQuestionModel),           Load<TextListQuestionViewModel>           },
                { typeof(QrBarcodeQuestionModel),          Load<QrBarcodeQuestionViewModel>          },
                { typeof(MultimediaQuestionModel),         Load<MultimediaQuestionViewModel>         },
                { typeof(DateTimeQuestionModel),           Load<DateTimeQuestionViewModel>           },
                { typeof(GpsCoordinatesQuestionModel),     Load<GpsCoordinatesQuestionViewModel>     },
            };

        private static T Load<T>() where T : class
        {
            return Mvx.Resolve<T>();
        }

        public InterviewViewModelFactory(
            IPlainRepository<QuestionnaireModel> plainQuestionnaireRepository,
            IPlainRepository<InterviewModel> plainStorageInterviewAccessor)
        {
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.plainStorageInterviewAccessor = plainStorageInterviewAccessor;
        }

        public Task<IEnumerable> GetEntitiesAsync(string interviewId, Identity groupIdentity)
        {
            return Task.Run(()=> GenerateViewModels(interviewId, groupIdentity));
        }

        public Task<IEnumerable> GetPrefilledQuestionsAsync(string interviewId)
        {
            return Task.Run(() => GetPrefilledQuestionsImpl(interviewId));
        }

        private IEnumerable GenerateViewModels(string interviewId, Identity groupIdentity)
        {
            var interview = this.plainStorageInterviewAccessor.Get(interviewId);
            var questionnaire = this.plainQuestionnaireRepository.Get(interview.QuestionnaireId);

            if (groupIdentity == null || groupIdentity.Id == Guid.Empty)
            {
                groupIdentity = new Identity(questionnaire.GroupsWithoutNestedChildren.Keys.First(), new decimal[0]);
            }

            if (!questionnaire.GroupsWithoutNestedChildren.ContainsKey(groupIdentity.Id))
                throw new KeyNotFoundException("Group with id : {0} don't found".FormatString(groupIdentity));

            var groupWithoutNestedChildren = questionnaire.GroupsWithoutNestedChildren[groupIdentity.Id];

            var viewModels = groupWithoutNestedChildren
                .Children
                .Select(child => CreateInterviewItemViewModel(child.Id, groupIdentity.RosterVector, child.ModelType, interview, questionnaire))
                .ToList();

            return viewModels;
        }

        private IEnumerable GetPrefilledQuestionsImpl(string interviewId)
        {
            var interview = this.plainStorageInterviewAccessor.Get(interviewId);
            var questionnaire = this.plainQuestionnaireRepository.Get(interview.QuestionnaireId);

            return questionnaire.PrefilledQuestionsIds.Select(x => CreateInterviewItemViewModel(x.Id, new decimal[0], x.ModelType, interview, questionnaire));
        }

        private static BaseInterviewItemViewModel CreateInterviewItemViewModel(
            Guid entityId,
            decimal[] rosterVector,
            Type entityModelType,
            InterviewModel interview,
            QuestionnaireModel questionnaire)
        {
            var identity = new Identity(entityId, rosterVector);

            if (!QuestionnaireEntityTypeToViewModelMap.ContainsKey(entityModelType))
            {
                throw new ArgumentOutOfRangeException("entityModelType");
            }

            var viewModelActivator = QuestionnaireEntityTypeToViewModelMap[entityModelType];

            BaseInterviewItemViewModel viewModel = viewModelActivator.Invoke();

            viewModel.Init(identity, interview, questionnaire);
            return viewModel;
        }
    }
}