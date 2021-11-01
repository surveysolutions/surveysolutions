using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.Extensions.EnumMapping;
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
using QuestionType = Main.Core.Entities.SubEntities.QuestionType;
using StaticText = Main.Core.Entities.SubEntities.StaticText;
using ValidationCondition = WB.UI.Designer.Code.ImportExport.Models.ValidationCondition;


namespace WB.UI.Designer.Code.ImportExport
{
    public class QuestionnaireAutoMapperProfile : Profile
    {
        public QuestionnaireAutoMapperProfile()
        {
            this.CreateMap<QuestionnaireDocument, Questionnaire>()
                .ForMember(x => x.Id, opts => opts.MapFrom(t => t.PublicKey))
                .ForMember(x => x.CoverPage, x => x.MapFrom(s =>
                    s.Children.Count > 0 && s.Children[0].PublicKey == s.CoverPageSectionId
                        ? (Group)s.Children[0]
                        : null
                ))
                .ForMember(x => x.Children, x => x.MapFrom(s =>
                    s.Children.Count > 0 && s.Children[0].PublicKey == s.CoverPageSectionId
                        ? s.Children.Skip(1)
                        : s.Children));
            this.CreateMap<Questionnaire, QuestionnaireDocument>()
                .ForMember(s => s.PublicKey, opt => opt.MapFrom(t => t.Id))
                .ForMember(s => s.Id, opt => opt.MapFrom(t => t.Id.FormatGuid()))
                .ForMember(s => s.CoverPageSectionId, opt => opt.MapFrom(t => 
                    t.CoverPage != null ? t.CoverPage.Id : Guid.NewGuid()))
                .ForMember(s => s.Children, opt => opt.MapFrom(t =>
                    t.CoverPage != null ? Enumerable.Concat<QuestionnaireEntity>(t.CoverPage.ToEnumerable(), t.Children) : t.Children));
            
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

            this.CreateMap<IComposite, QuestionnaireEntity>()
                .ForMember(d => d.Id, opt => 
                    opt.MapFrom(t => t.PublicKey));
            this.CreateMap<IQuestionnaireEntity, IComposite>();

            this.CreateMap<Group, Models.Group>()
                .IncludeBase<IComposite, QuestionnaireEntity>()
                .ConstructUsing((g, context) => g.IsRoster ? context.Mapper.Map<Roster>(g) : new Models.Group());
            this.CreateMap<Models.Group, Group>()
                .IncludeBase<IQuestionnaireEntity, IComposite>()
                .ForMember(s => s.PublicKey, opt => opt.MapFrom(t => t.Id));

            this.CreateMap<Group, Models.Roster>()
                .IncludeBase<Group, Models.Group>()
                .ForMember(x => x.Title, x => x.MapFrom(s =>
                    s.CustomRosterTitle ? s.Title : s.Title + @" - %rostertitle%"));
            this.CreateMap<Models.Roster, Group>()
                .IncludeBase<Models.Group, Group>()
                .ForMember(s => s.IsRoster, opt => opt.MapFrom(t => true))
                .ForMember(x => x.CustomRosterTitle, x => x.MapFrom(s => true));

            this.CreateMap<Group, Models.CoverPage>()
                .IncludeBase<IComposite, QuestionnaireEntity>();
            this.CreateMap<Models.CoverPage, Group>()
                .IncludeBase<IQuestionnaireEntity, IComposite>()
                .ForMember(s => s.PublicKey, opt => opt.MapFrom(t => t.Id));

            this.CreateMap<Documents.FixedRosterTitle, Models.FixedRosterTitle>();
            this.CreateMap<Models.FixedRosterTitle, Documents.FixedRosterTitle>()
                .ConstructUsing(c => new Documents.FixedRosterTitle(c.Value, c.Title));
            
            //this.CreateMap<Main.Core.Entities.SubEntities.RosterDisplayMode, Models.RosterDisplayMode>()
                //.ConvertUsingEnumMapping(o => o.MapByName())
                //.ReverseMap();
                
            this.CreateMap<StaticText, Models.StaticText>()
                .IncludeBase<IComposite, QuestionnaireEntity>();
            this.CreateMap<Models.StaticText, StaticText>()
                .IncludeBase<IQuestionnaireEntity, IComposite>()
                .ForMember(s => s.PublicKey, opt => opt.MapFrom(t => t.Id))
                .ConstructUsing(s => new StaticText(s.Id, s.Text, s.ConditionExpression ?? String.Empty, s.HideIfDisabled, null, s.AttachmentName, null));

            this.CreateMap<QuestionnaireEntities.Variable, Models.Variable>()
                .IncludeBase<IComposite, QuestionnaireEntity>()
                .ForMember(s => s.VariableType, o => o.MapFrom(d => d.Type));
            this.CreateMap<Models.Variable, QuestionnaireEntities.Variable>()
                .IncludeBase<IQuestionnaireEntity, IComposite>()
                .ForMember(s => s.PublicKey, opt => opt.MapFrom(t => t.Id))
                .ForMember(s => s.Name, opt => opt.MapFrom(t => t.VariableName))
                .ForMember(s => s.Type, o => o.MapFrom(d => d.VariableType))
                .ConstructUsing(v => new QuestionnaireEntities.Variable(v.Id, null, null));
            
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
                .ForMember(s => s.PublicKey, opt => opt.MapFrom(t => t.Id))
                .AfterMap((s, d) => d.Properties!.HideInstructions = s.HideInstructions ?? false);

            this.CreateMap<TextQuestion, Models.Question.TextQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();
            this.CreateMap<Models.Question.TextQuestion, TextQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>()
                .AfterMap((s, d) => d.QuestionType = QuestionType.Text);

            this.CreateMap<NumericQuestion, Models.Question.NumericQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>()
                .ForMember(s => s.UseThousandsSeparator, d => 
                    d.MapFrom(t => t.Properties != null ? t.Properties.UseFormatting : false))
                .ForMember(s => s.DecimalPlaces, o => o.MapFrom(d => d.CountOfDecimalPlaces));
            this.CreateMap<Models.Question.NumericQuestion, NumericQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>()
                .ForMember(s => s.UseFormatting, d => d.MapFrom(t => t.UseThousandsSeparator))
                .ForMember(s => s.CountOfDecimalPlaces, d => d.MapFrom(t => t.DecimalPlaces))
                .AfterMap((s, d) => d.Properties!.UseFormatting = s.UseThousandsSeparator)
                .AfterMap((s, d) => d.QuestionType = QuestionType.Numeric);
            
            this.CreateMap<AreaQuestion, Models.Question.AreaQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>()
                .ForMember(s => s.GeometryType, d => d.MapFrom(t => t.Properties != null ? t.Properties.GeometryType : GeometryType.Point));
            this.CreateMap<Models.Question.AreaQuestion, AreaQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>()
                .AfterMap((s, d) => d.Properties!.GeometryType = (GeometryType?)(s.GeometryType ?? null))
                .AfterMap((s, d) => d.QuestionType = QuestionType.Area);

            this.CreateMap<AudioQuestion, Models.Question.AudioQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();
            this.CreateMap<Models.Question.AudioQuestion, AudioQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>()
                .AfterMap((s, d) => d.QuestionType = QuestionType.Audio);

            this.CreateMap<MultimediaQuestion, Models.Question.PictureQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();
            this.CreateMap<Models.Question.PictureQuestion, MultimediaQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>()
                .AfterMap((s, d) => d.QuestionType = QuestionType.Multimedia);

            this.CreateMap<SingleQuestion, Models.Question.SingleQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>()
                .ForMember(s => s.LinkedToId, t => t.MapFrom(o =>
                    o.LinkedToQuestionId ?? o.LinkedToRosterId))
                .AfterMap((s, t) => 
                    {
                        if (s.LinkedToQuestionId.HasValue || s.LinkedToRosterId.HasValue)
                            t.FilterExpression = s.LinkedFilterExpression;
                        else
                            t.FilterExpression = s.Properties?.OptionsFilterExpression;
                    });
            this.CreateMap<Models.Question.SingleQuestion, SingleQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>()
                .ForMember(s => s.LinkedToQuestionId, o => o.MapFrom(t => t.LinkedToId))
                .ForMember(s => s.LinkedToRosterId, o => o.MapFrom(t => t.LinkedToId))
                .AfterMap((s, d) =>
                {
                    if (s.LinkedToId.HasValue)
                        d.LinkedFilterExpression = s.FilterExpression;
                    else
                        d.Properties!.OptionsFilterExpression = s.FilterExpression;
                })
                .AfterMap((s, d) => d.QuestionType = QuestionType.SingleOption);

            this.CreateMap<MultyOptionsQuestion, Models.Question.MultiOptionsQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>()
                .ForMember(s => s.LinkedToId, t => t.MapFrom(o =>
                    o.LinkedToQuestionId ?? o.LinkedToRosterId))
                .AfterMap((s, t) => 
                {
                    if (s.LinkedToQuestionId.HasValue || s.LinkedToRosterId.HasValue)
                        t.FilterExpression = s.LinkedFilterExpression;
                    else
                        t.FilterExpression = s.Properties?.OptionsFilterExpression;
                });
            this.CreateMap<Models.Question.MultiOptionsQuestion, MultyOptionsQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>()
                .ForMember(s => s.LinkedToQuestionId, o => o.MapFrom(t => t.LinkedToId))
                .ForMember(s => s.LinkedToRosterId, o => o.MapFrom(t => t.LinkedToId))
                .AfterMap((s, d) =>
                {
                    if (s.LinkedToId.HasValue)
                        d.LinkedFilterExpression = s.FilterExpression;
                    else
                        d.Properties!.OptionsFilterExpression = s.FilterExpression;
                })
                .AfterMap((s, d) => d.QuestionType = QuestionType.MultyOption);

            this.CreateMap<TextListQuestion, Models.Question.TextListQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>()
                .ForMember(s => s.MaxItemsCount, o => o.MapFrom(d => d.MaxAnswerCount));
            this.CreateMap<Models.Question.TextListQuestion, TextListQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>()
                .AfterMap((s, d) => d.QuestionType = QuestionType.TextList)
                .ForMember(s => s.MaxAnswerCount, o => o.MapFrom(d => d.MaxItemsCount));

            this.CreateMap<DateTimeQuestion, Models.Question.DateTimeQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>()
                .ForMember(s => s.DefaultDate, d => 
                    d.MapFrom(t => t.Properties != null ? t.Properties.DefaultDate : null));
            this.CreateMap<Models.Question.DateTimeQuestion, DateTimeQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>()
                .AfterMap((s, d) => d.Properties!.DefaultDate = s.DefaultDate)
                .AfterMap((s, d) => d.QuestionType = QuestionType.DateTime);

            this.CreateMap<GpsCoordinateQuestion, Models.Question.GpsCoordinateQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();
            this.CreateMap<Models.Question.GpsCoordinateQuestion, GpsCoordinateQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>()
                .AfterMap((s, d) => d.QuestionType = QuestionType.GpsCoordinates);

            this.CreateMap<QRBarcodeQuestion, Models.Question.QRBarcodeQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>();
            this.CreateMap<Models.Question.QRBarcodeQuestion, QRBarcodeQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>()
                .AfterMap((s, d) => d.QuestionType = QuestionType.QRBarcode);
            
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