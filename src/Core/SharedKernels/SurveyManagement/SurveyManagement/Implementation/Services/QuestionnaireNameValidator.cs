using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    internal class QuestionnaireNameValidator : ICommandValidator<Questionnaire, ImportFromDesigner>
    {
        public void Validate(Questionnaire aggregate, ImportFromDesigner command)
        {
            // TODO: KP-5910
        }
    }
}