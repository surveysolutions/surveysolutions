using System.Globalization;
using AutoMapper;
using DocumentFormat.OpenXml.EMMA;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.UI.Designer.Code.ImportExport.Models;
using WB.Core.SharedKernels.QuestionnaireEntities;
using Answer = Main.Core.Entities.SubEntities.Answer;
using Group = Main.Core.Entities.SubEntities.Group;
using QuestionProperties = WB.Core.SharedKernels.QuestionnaireEntities.QuestionProperties;
using Documents = WB.Core.SharedKernels.SurveySolutions.Documents;
using QuestionnaireEntities = WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.UI.Designer.Code.ImportExport
{
    public class QuestionnaireAutoMapperProfile : Profile
    {
        public QuestionnaireAutoMapperProfile()
        {
            this.CreateMap<QuestionnaireDocument, Questionnaire>();
            //.ForMember(x => x.Children, opts => opts.Ignore());
            this.CreateMap<Questionnaire, QuestionnaireDocument>();
            
            this.CreateMap<WB.Core.SharedKernels.Questionnaire.Documents.QuestionnaireMetaInfo, Models.QuestionnaireMetaInfo>();
            this.CreateMap<Models.QuestionnaireMetaInfo, WB.Core.SharedKernels.Questionnaire.Documents.QuestionnaireMetaInfo>();

            this.CreateMap<IComposite, QuestionnaireEntity>();
            this.CreateMap<IQuestionnaireEntity, IComposite>();

            this.CreateMap<Group, Models.Group>()
                .IncludeBase<IComposite, QuestionnaireEntity>();
            //this.CreateMap<Group, QuestionnaireEntity>().As<Models.Group>();

            this.CreateMap<Documents.FixedRosterTitle, Models.FixedRosterTitle>();
            this.CreateMap<Models.FixedRosterTitle, Documents.FixedRosterTitle>()
                .ConstructUsing(c => new Documents.FixedRosterTitle(c.Value, c.Title));
                
            this.CreateMap<Models.Group, Group>()
                .IncludeBase<IQuestionnaireEntity, IComposite>();
            //this.CreateMap<Models.Group, IComposite>().As<Group>();
            
            this.CreateMap<QuestionProperties, Models.QuestionProperties>();
            this.CreateMap<Models.QuestionProperties, QuestionProperties>();
            
            this.CreateMap<QuestionnaireEntities.ValidationCondition, Models.ValidationCondition>();
            this.CreateMap<Models.ValidationCondition, QuestionnaireEntities.ValidationCondition>();
            
            this.CreateMap<AbstractQuestion, Models.Question.AbstractQuestion>()
                .IncludeBase<IComposite, QuestionnaireEntity>();
            this.CreateMap<Models.Question.AbstractQuestion, AbstractQuestion>()
                .IncludeBase<IQuestionnaireEntity, IComposite>()
                .ForMember(aq => aq.StataExportCaption, aq=> aq.MapFrom(x => x.VariableName));

            this.CreateMap<TextQuestion, Models.Question.TextQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();
            //this.CreateMap<TextQuestion, QuestionnaireEntity>().As<Models.Question.TextQuestion>();
            this.CreateMap<Models.Question.TextQuestion, TextQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>();

            this.CreateMap<NumericQuestion, Models.Question.NumericQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();

            this.CreateMap<AreaQuestion, Models.Question.AreaQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();

            this.CreateMap<AudioQuestion, Models.Question.AudioQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();

            this.CreateMap<SingleQuestion, Models.Question.SingleQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();

            this.CreateMap<MultimediaQuestion, Models.Question.MultimediaQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();

            this.CreateMap<MultyOptionsQuestion, Models.Question.MultiOptionsQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();

            this.CreateMap<TextListQuestion, Models.Question.TextListQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();

            this.CreateMap<DateTimeQuestion, Models.Question.DateTimeQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();

            this.CreateMap<GpsCoordinateQuestion, Models.Question.GpsCoordinateQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();

            this.CreateMap<QRBarcodeQuestion, Models.Question.QRBarcodeQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();
            
            this.CreateMap<Answer, Models.Answer>()
                .ForMember(a => a.Code, opt => 
                    opt.MapFrom(x => x.AnswerCode ?? decimal.Parse(x.AnswerValue, NumberStyles.Number, CultureInfo.InvariantCulture)))
                .ForMember(a => a.ParentCode, opt => 
                    opt.MapFrom((answer, mAnswer) => answer.ParentCode ?? (decimal.TryParse(answer.ParentValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parentValue) ? parentValue : (decimal?)null)));
        }
    }
}