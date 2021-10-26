using System;
using System.Globalization;
using AutoMapper;
using DocumentFormat.OpenXml.EMMA;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.UI.Designer.Code.ImportExport.Models;
using WB.Core.SharedKernels.QuestionnaireEntities;
using Answer = Main.Core.Entities.SubEntities.Answer;
using Group = Main.Core.Entities.SubEntities.Group;
using QuestionProperties = WB.Core.SharedKernels.QuestionnaireEntities.QuestionProperties;
using Documents = WB.Core.SharedKernels.SurveySolutions.Documents;
using IQuestionnaireEntity = WB.UI.Designer.Code.ImportExport.Models.IQuestionnaireEntity;
using QuestionnaireEntities = WB.Core.SharedKernels.QuestionnaireEntities;
using StaticText = Main.Core.Entities.SubEntities.StaticText;


namespace WB.UI.Designer.Code.ImportExport
{
    public class QuestionnaireAutoMapperProfile : Profile
    {
        public QuestionnaireAutoMapperProfile()
        {
            this.CreateMap<QuestionnaireDocument, Questionnaire>()
                .ForMember(x => x.Id, opts => opts.MapFrom(t => t.PublicKey));
            this.CreateMap<Questionnaire, QuestionnaireDocument>()
                .ForMember(s => s.PublicKey, opt => opt.MapFrom(t => t.Id))
                .ForMember(s => s.Id, opt => opt.MapFrom(t => t.Id.FormatGuid()))
                /*.ForMember(s => s.CoverPageSectionId, opt => opt.MapFrom(t => 
                    t.Children != null && t.Children.Count > 0 ? t.Children[0].PublicKey : Guid.NewGuid()))*/;
            
            this.CreateMap<WB.Core.SharedKernels.Questionnaire.Documents.QuestionnaireMetaInfo, Models.QuestionnaireMetaInfo>();
            this.CreateMap<Models.QuestionnaireMetaInfo, WB.Core.SharedKernels.Questionnaire.Documents.QuestionnaireMetaInfo>();

            this.CreateMap<Documents.Attachment, Models.Attachment>();
            this.CreateMap<Models.Attachment, Documents.Attachment>();

            this.CreateMap<Documents.LookupTable, Models.LookupTable>();
            this.CreateMap<Models.LookupTable, Documents.LookupTable>();

            this.CreateMap<Documents.Translation, Models.Translation>();
            this.CreateMap<Models.Translation, Documents.Translation>();

            this.CreateMap<Documents.Categories, Models.Categories>();
            this.CreateMap<Models.Categories, Documents.Categories>();

            this.CreateMap<Documents.Macro, Models.Macro>();
            this.CreateMap<Models.Macro, Documents.Macro>();

            this.CreateMap<IComposite, QuestionnaireEntity>();
            this.CreateMap<IQuestionnaireEntity, IComposite>();

            this.CreateMap<Group, Models.Group>()
                .IncludeBase<IComposite, QuestionnaireEntity>();
            //this.CreateMap<Group, QuestionnaireEntity>().As<Models.Group>();
            this.CreateMap<Models.Group, Group>()
                .IncludeBase<IQuestionnaireEntity, IComposite>();
            //this.CreateMap<Models.Group, IComposite>().As<Group>();

            this.CreateMap<Documents.FixedRosterTitle, Models.FixedRosterTitle>();
            this.CreateMap<Models.FixedRosterTitle, Documents.FixedRosterTitle>()
                .ConstructUsing(c => new Documents.FixedRosterTitle(c.Value, c.Title));
                
            this.CreateMap<StaticText, Models.StaticText>()
                .IncludeBase<IComposite, QuestionnaireEntity>();
            this.CreateMap<Models.StaticText, StaticText>()
                .IncludeBase<IQuestionnaireEntity, IComposite>();
            
            this.CreateMap<QuestionnaireEntities.Variable, Models.Variable>()
                .IncludeBase<IComposite, QuestionnaireEntity>();
            this.CreateMap<Models.Variable, QuestionnaireEntities.Variable>()
                .IncludeBase<IQuestionnaireEntity, IComposite>()
                .ConstructUsing(v => new QuestionnaireEntities.Variable(v.PublicKey, null, null));
            
            //this.CreateMap<QuestionProperties, Models.QuestionProperties>();
            //this.CreateMap<Models.QuestionProperties, QuestionProperties>();
            
            this.CreateMap<QuestionnaireEntities.ValidationCondition, Models.ValidationCondition>();
            this.CreateMap<Models.ValidationCondition, QuestionnaireEntities.ValidationCondition>();
            
            this.CreateMap<AbstractQuestion, Models.Question.AbstractQuestion>()
                .IncludeBase<IComposite, QuestionnaireEntity>()
                .ForMember(d => d.HideInstructions, t => 
                    t.MapFrom(p => p.Properties == null ? false : p.Properties.HideInstructions));
            this.CreateMap<Models.Question.AbstractQuestion, AbstractQuestion>()
                .IncludeBase<IQuestionnaireEntity, IComposite>()
                .ForMember(aq => aq.StataExportCaption, aq=> 
                    aq.MapFrom(x => x.VariableName))
                // .ForMember(d => d.Properties!.HideInstructions, s =>
                //     s.MapFrom(p => p.HideInstructions))
                //.BeforeMap((s, d) => d.Properties = new QuestionProperties(false, false))
                .AfterMap((s, d) => d.Properties!.HideInstructions = s.HideInstructions ?? false);

            this.CreateMap<TextQuestion, Models.Question.TextQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();
            //this.CreateMap<TextQuestion, QuestionnaireEntity>().As<Models.Question.TextQuestion>();
            this.CreateMap<Models.Question.TextQuestion, TextQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>();

            this.CreateMap<NumericQuestion, Models.Question.NumericQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>()
                .ForMember(s => s.UseFormatting, d => 
                    d.MapFrom(t => t.Properties != null ? t.Properties.UseFormatting : false));
            this.CreateMap<Models.Question.NumericQuestion, NumericQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>()
                .ForMember(s => s.UseFormatting, d => d.MapFrom(t => t.UseFormatting))
                .AfterMap((s, d) => d.Properties!.UseFormatting = s.UseFormatting);
            
            this.CreateMap<AreaQuestion, Models.Question.AreaQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>()
                .ForMember(s => s.GeometryType, d => d.MapFrom(t => t.Properties != null ? t.Properties.GeometryType : GeometryType.Point));
            this.CreateMap<Models.Question.AreaQuestion, AreaQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>()
                //.ForMember(s => s.Properties!.GeometryType, d => d.MapFrom(t => t.GeometryType));
                .AfterMap((s, d) => d.Properties!.GeometryType = (GeometryType)(s.GeometryType ?? Models.Question.GeometryType.Polygon));

            this.CreateMap<AudioQuestion, Models.Question.AudioQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();
            this.CreateMap<Models.Question.AudioQuestion, AudioQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>();

            this.CreateMap<SingleQuestion, Models.Question.SingleQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>()
                .ForMember(s => s.OptionsFilterExpression, d =>
                    d.MapFrom(t => t.Properties != null ? t.Properties.OptionsFilterExpression : null));
            this.CreateMap<Models.Question.SingleQuestion, SingleQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>()
                //.ForMember(s => s.Properties!.OptionsFilterExpression, d => d.MapFrom(t => t.OptionsFilterExpression));
                .AfterMap((s, d) => d.Properties!.OptionsFilterExpression = s.OptionsFilterExpression);

            this.CreateMap<MultimediaQuestion, Models.Question.MultimediaQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();
            this.CreateMap<Models.Question.MultimediaQuestion, MultimediaQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>();

            this.CreateMap<MultyOptionsQuestion, Models.Question.MultiOptionsQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>()
                .ForMember(s => s.OptionsFilterExpression, d => 
                    d.MapFrom(t => t.Properties != null ? t.Properties.OptionsFilterExpression : null));
            this.CreateMap<Models.Question.MultiOptionsQuestion, MultyOptionsQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>()
                //.ForMember(s => s.Properties!.OptionsFilterExpression, 
                //    d => d.MapFrom(t => t.OptionsFilterExpression));
                .AfterMap((s, d) => d.Properties!.OptionsFilterExpression = s.OptionsFilterExpression);

            this.CreateMap<TextListQuestion, Models.Question.TextListQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();
            this.CreateMap<Models.Question.TextListQuestion, TextListQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>();

            this.CreateMap<DateTimeQuestion, Models.Question.DateTimeQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>()
                .ForMember(s => s.DefaultDate, d => 
                    d.MapFrom(t => t.Properties != null ? t.Properties.DefaultDate : null));
            this.CreateMap<Models.Question.DateTimeQuestion, DateTimeQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>()
                //.ForMember(s => s.Properties!.DefaultDate, d =>
                //    d.MapFrom(t => t.DefaultDate));
                .AfterMap((s, d) => d.Properties!.DefaultDate = s.DefaultDate);

            this.CreateMap<GpsCoordinateQuestion, Models.Question.GpsCoordinateQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();
            this.CreateMap<Models.Question.GpsCoordinateQuestion, GpsCoordinateQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>();

            this.CreateMap<QRBarcodeQuestion, Models.Question.QRBarcodeQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();
            this.CreateMap<Models.Question.QRBarcodeQuestion, QRBarcodeQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>();
            
            this.CreateMap<Answer, Models.Answer>()
                .ForMember(a => a.Code, opt => 
                    opt.MapFrom(x => x.AnswerCode ?? decimal.Parse(x.AnswerValue, NumberStyles.Number, CultureInfo.InvariantCulture)))
                .ForMember(a => a.ParentCode, opt => 
                    opt.MapFrom((answer, mAnswer) => answer.ParentCode ?? (decimal.TryParse(answer.ParentValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parentValue) ? parentValue : (decimal?)null)))
                .ForMember(a => a.Text, opt => 
                    opt.MapFrom(answer => answer.AnswerText));
            this.CreateMap<Models.Answer, Answer>()
                .ForMember(a => a.AnswerValue, opt => 
                    opt.MapFrom(x => x.Code.ToString()))
                .ForMember(a => a.ParentValue, opt => 
                    opt.MapFrom(answer => answer.ParentCode.ToString()))
                .ForMember(a => a.AnswerText, opt => 
                    opt.MapFrom(answer => answer.Text));
        }
    }
}