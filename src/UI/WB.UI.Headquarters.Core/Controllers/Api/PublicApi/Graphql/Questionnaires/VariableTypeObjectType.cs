using System;
using HotChocolate.Types;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires
{
    public class VariableTypeObjectType : EnumType<VariableType>
    {
        protected override void Configure(IEnumTypeDescriptor<VariableType> descriptor)
        {
            descriptor.BindValuesExplicitly();
            var values = Enum.GetNames(typeof(VariableType));
            foreach (var variableType in values)
            { 
                descriptor.Value(Enum.Parse<VariableType>(variableType));
            }
        }
    }
}
