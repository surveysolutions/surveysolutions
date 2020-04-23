using System;
using System.Reflection;
using HotChocolate.Types;
using Main.Core.Entities.SubEntities;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires
{
    public class QuestionTypeObjectType : EnumType<QuestionType>
    {
        protected override void Configure(IEnumTypeDescriptor<QuestionType> descriptor)
        {
            descriptor.BindValuesExplicitly();
            var values = Enum.GetNames(typeof(QuestionType));
            foreach (var questionType in values)
            {
                bool isObsolete = typeof (QuestionType).GetField(questionType)
                    .GetCustomAttribute(typeof (ObsoleteAttribute)) != null;
                if (!isObsolete)
                {
                    descriptor.Value(Enum.Parse<QuestionType>(questionType));
                }
            }

        }
    }
}