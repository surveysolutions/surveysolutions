using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ImportExport.Models;
using WB.Core.BoundedContexts.Designer.ImportExport.Models.Question;
using WB.Core.GenericSubdomains.Portable;
using AbstractQuestion = Main.Core.Entities.SubEntities.AbstractQuestion;
using Answer = Main.Core.Entities.SubEntities.Answer;
using AreaQuestion = Main.Core.Entities.SubEntities.Question.AreaQuestion;
using AudioQuestion = Main.Core.Entities.SubEntities.Question.AudioQuestion;
using DateTimeQuestion = Main.Core.Entities.SubEntities.Question.DateTimeQuestion;
using Group = Main.Core.Entities.SubEntities.Group;
using Documents = WB.Core.SharedKernels.SurveySolutions.Documents;
using GeometryType = WB.Core.SharedKernels.Questionnaire.Documents.GeometryType;
using GpsCoordinateQuestion = Main.Core.Entities.SubEntities.Question.GpsCoordinateQuestion;
using IQuestion = Main.Core.Entities.SubEntities.IQuestion;
using IQuestionnaireEntity = WB.Core.BoundedContexts.Designer.ImportExport.Models.IQuestionnaireEntity;
using NumericQuestion = Main.Core.Entities.SubEntities.Question.NumericQuestion;
using QRBarcodeQuestion = Main.Core.Entities.SubEntities.Question.QRBarcodeQuestion;
using QuestionnaireEntities = WB.Core.SharedKernels.QuestionnaireEntities;
using QuestionType = Main.Core.Entities.SubEntities.QuestionType;
using SingleQuestion = Main.Core.Entities.SubEntities.Question.SingleQuestion;
using StaticText = Main.Core.Entities.SubEntities.StaticText;
using TextListQuestion = Main.Core.Entities.SubEntities.Question.TextListQuestion;
using TextQuestion = Main.Core.Entities.SubEntities.Question.TextQuestion;


namespace WB.Core.BoundedContexts.Designer.ImportExport
{
    public class QuestionnaireAutoMapperProfile : Profile
    {
        public QuestionnaireAutoMapperProfile()
        {
            this.CreateMap<QuestionnaireDocument, Questionnaire>()
                .ForMember(x => x.Id, opts => opts.MapFrom(t => 
                    t.PublicKey))
                .ForMember(x => x.CoverPage, x => x.MapFrom(s =>
                    s.Children.Count > 0 && s.Children[0].PublicKey == s.CoverPageSectionId
                        ? (Group)s.Children[0]
                        : new Group("Cover", 
                            ((IComposite)s).TreeToEnumerableDepthFirst(c => c.Children)
                            .Where(e => IsIdentified(e))
                            .ToList())
                        {
                            PublicKey = s.CoverPageSectionId,
                        }
                ))
                .ForMember(x => x.Children, x => x.MapFrom(s =>
                    s.Children.Count > 0 && s.Children[0].PublicKey == s.CoverPageSectionId
                        ? s.Children.Skip(1)
                        : s.Children))
                .ForMember(x => x.DefaultTranslation, x => x.MapFrom(s => 
                    s.DefaultTranslation.HasValue 
                        ? s.Translations.FirstOrDefault(t => t.Id == s.DefaultTranslation.Value)?.Name 
                        : null));
            this.CreateMap<Questionnaire, QuestionnaireDocument>()
                .ForMember(s => s.PublicKey, opt => opt.MapFrom(t => t.Id))
                .ForMember(s => s.Id, opt => opt.MapFrom(t => t.Id.FormatGuid()))
                .ForMember(s => s.Children, opt => opt.MapFrom(t =>
                    Enumerable.Concat<QuestionnaireEntity>((t.CoverPage != null ? t.CoverPage : new CoverPage() { Id = Guid.NewGuid(), Title = "Cover" }).ToEnumerable(), t.Children)))
                .AfterMap((s, d, context) =>
                {
                    if (s.CoverPage != null)
                        d.CoverPageSectionId = d.Children[0].PublicKey;
                    else
                        d.CoverPageSectionId = Guid.NewGuid();
                })
                .AfterMap((s, d, context) =>
                {
                    if (!string.IsNullOrWhiteSpace(s.DefaultTranslation))
                    {
                        d.DefaultTranslation = d.Translations.FirstOrDefault(t => t.Name == s.DefaultTranslation)?.Id;
                    }
                });
            
            this.CreateMap<WB.Core.SharedKernels.Questionnaire.Documents.QuestionnaireMetaInfo, Models.QuestionnaireMetaInfo>();
            this.CreateMap<Models.QuestionnaireMetaInfo, WB.Core.SharedKernels.Questionnaire.Documents.QuestionnaireMetaInfo>();

            this.CreateMap<Documents.Attachment, Models.Attachment>();
            this.CreateMap<Models.Attachment, Documents.Attachment>()
                .ForMember(x => x.AttachmentId, x => x.MapFrom(s => Guid.NewGuid()));

            this.CreateMap<Documents.LookupTable, Models.LookupTable>();
            this.CreateMap<Models.LookupTable, Documents.LookupTable>();

            this.CreateMap<KeyValuePair<Guid, Documents.LookupTable>, Models.LookupTable>()
                .ConstructUsing((kv, context) => context.Mapper.Map<Documents.LookupTable, Models.LookupTable>(kv.Value));
            this.CreateMap<Models.LookupTable, KeyValuePair<Guid, Documents.LookupTable>>()
                .ConstructUsing((v, context) => new KeyValuePair<Guid, Documents.LookupTable>(Guid.NewGuid(), context.Mapper.Map<Models.LookupTable, Documents.LookupTable>(v)));

            this.CreateMap<Documents.Translation, Models.Translation>();
            this.CreateMap<Models.Translation, Documents.Translation>()
                .ForMember(x => x.Id, x => x.MapFrom(s => Guid.NewGuid()));

            this.CreateMap<Documents.Categories, Models.Categories>();
            this.CreateMap<Models.Categories, Documents.Categories>()
                .ForMember(x => x.Id, x => x.MapFrom(s =>
                     s.Id.HasValue ? s.Id.Value : Guid.NewGuid()));

            this.CreateMap<Documents.Macro, Models.Macro>();
            this.CreateMap<Models.Macro, Documents.Macro>();

            this.CreateMap<KeyValuePair<Guid, Documents.Macro>, Models.Macro>()
                .ConstructUsing((kv, context) => context.Mapper.Map<Documents.Macro, Models.Macro>(kv.Value));
            this.CreateMap<Models.Macro, KeyValuePair<Guid, Documents.Macro>>()
                .ConstructUsing((v, context) => new KeyValuePair<Guid, Documents.Macro>(Guid.NewGuid(), context.Mapper.Map<Models.Macro, Documents.Macro>(v)));

            this.CreateMap<IComposite, QuestionnaireEntity>()
                .ForMember(s => s.Id, opt => opt.MapFrom(t => 
                    t.PublicKey));
            this.CreateMap<IQuestionnaireEntity, IComposite>();

            this.CreateMap<Group, Models.Group>()
                .IncludeBase<IComposite, QuestionnaireEntity>()
                .ForMember(x => x.Children, x => x.MapFrom(s => 
                    s.Children.Where(e => !IsIdentified(e))))
                .ConstructUsing((g, context) => g.IsRoster ? context.Mapper.Map<Roster>(g) : new Models.Group());
            this.CreateMap<Models.Group, Group>()
                .IncludeBase<IQuestionnaireEntity, IComposite>()
                .ForMember(x => x.PublicKey, x => x.MapFrom((s, t, value, context) =>
                    GetIdOrGenerate(s.VariableName, s.Id, context)));

            this.CreateMap<Group, Models.Roster>()
                .IncludeBase<Group, Models.Group>()
                .ForMember(x => x.Title, x => x.MapFrom(s =>
                    s.CustomRosterTitle 
                    || s.DisplayMode == Main.Core.Entities.SubEntities.RosterDisplayMode.Matrix 
                    || s.DisplayMode == Main.Core.Entities.SubEntities.RosterDisplayMode.Table  
                        ? s.Title : s.Title + @" - %rostertitle%"))
                .ForMember(x => x.RosterSizeQuestion, x => x.MapFrom((s, t, value, context) =>
                    GetVarName(s.RosterSizeQuestionId, context)))
                .ForMember(x => x.RosterTitleQuestion, x => x.MapFrom((s, t, value, context) =>
                    GetVarName(s.RosterTitleQuestionId, context)));
            this.CreateMap<Models.Roster, Group>()
                .IncludeBase<Models.Group, Group>()
                .ForMember(s => s.IsRoster, opt => opt.MapFrom(t => true))
                .ForMember(x => x.CustomRosterTitle, x => x.MapFrom(s => 
                    s.DisplayMode != RosterDisplayMode.Table && s.DisplayMode != RosterDisplayMode.Matrix))
                .ForMember(x => x.RosterSizeQuestionId, x => x.MapFrom((s, t, value, context) =>
                    GetId(s.RosterSizeQuestion, context)))
                .ForMember(x => x.RosterTitleQuestionId, x => x.MapFrom((s, t, value, context) =>
                    GetId(s.RosterTitleQuestion, context)));

            this.CreateMap<Group, Models.CoverPage>()
                .IncludeBase<IComposite, QuestionnaireEntity>();
            this.CreateMap<Models.CoverPage, Group>()
                .IncludeBase<IQuestionnaireEntity, IComposite>()
                .ForMember(s => s.PublicKey, opt => opt.MapFrom((s, d, value, context) => 
                    GetIdOrGenerate(s.VariableName, s.Id, context)));

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
                .ForMember(s => s.PublicKey, opt => opt.MapFrom(t => 
                    t.Id.HasValue ? t.Id.Value : Guid.NewGuid()))
                .ConstructUsing(s => new StaticText(Guid.Empty, s.Text, s.ConditionExpression ?? String.Empty, s.HideIfDisabled, null, s.AttachmentName, null));

            this.CreateMap<QuestionnaireEntities.Variable, Models.Variable>()
                .IncludeBase<IComposite, QuestionnaireEntity>()
                .ForMember(s => s.VariableType, o => o.MapFrom(d => d.Type));
            this.CreateMap<Models.Variable, QuestionnaireEntities.Variable>()
                .IncludeBase<IQuestionnaireEntity, IComposite>()
                .ForMember(s => s.PublicKey, opt => opt.MapFrom((s, d, value, context) => 
                    GetIdOrGenerate(s.VariableName, s.Id, context)))
                .ForMember(s => s.Name, opt => opt.MapFrom(t => t.VariableName))
                .ForMember(s => s.Type, o => o.MapFrom(d => d.VariableType))
                .ConstructUsing((v, context) => new QuestionnaireEntities.Variable(Guid.Empty, null, null));
            
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
                .ForMember(s => s.PublicKey, opt => opt.MapFrom((s, d, value, context) => 
                    GetIdOrGenerate(s.VariableName, s.Id, context)))
                .AfterMap((s, d) => d.Properties!.HideInstructions = s.HideInstructions);

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
                .ForMember(x => x.DisplayMode, x => x.MapFrom(s =>
                    s.CascadeFromQuestionId.HasValue ? SingleOptionDisplayMode.Cascading : 
                        (s.IsFilteredCombobox == true ? SingleOptionDisplayMode.Combobox : SingleOptionDisplayMode.Radio)))
                .ForMember(s => s.LinkedTo, t => t.MapFrom((s, d, value, context) => 
                    GetVarName(s.LinkedToQuestionId ?? s.LinkedToRosterId, context)))
                .ForMember(s => s.CascadeFromQuestion, t => t.MapFrom((s, d, value, context) => 
                    GetVarName(s.CascadeFromQuestionId, context)))
                .AfterMap((s, t) => 
                    {
                        if (s.LinkedToQuestionId.HasValue || s.LinkedToRosterId.HasValue)
                            t.FilterExpression = s.LinkedFilterExpression;
                        else
                            t.FilterExpression = s.Properties?.OptionsFilterExpression;
                    });
            this.CreateMap<Models.Question.SingleQuestion, SingleQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>()
                .ForMember(x => x.IsFilteredCombobox, x => x.MapFrom(s => 
                    s.DisplayMode == SingleOptionDisplayMode.Combobox ? true : (bool?)null))
                .ForMember(s => s.LinkedToQuestionId, o => o.MapFrom((s, d, value, context) 
                    => GetId(s.LinkedTo, context)))
                .ForMember(s => s.LinkedToRosterId, o => o.MapFrom((s, d, value, context) 
                    => GetId(s.LinkedTo, context)))
                .ForMember(s => s.CascadeFromQuestionId, o => o.MapFrom((s, d, value, context) 
                    => GetId(s.CascadeFromQuestion, context)))
                .AfterMap((s, d) =>
                {
                    if (!s.LinkedTo.IsNullOrEmpty())
                        d.LinkedFilterExpression = s.FilterExpression;
                    else
                        d.Properties!.OptionsFilterExpression = s.FilterExpression;
                })
                .AfterMap((s, d) => d.QuestionType = QuestionType.SingleOption);

            this.CreateMap<MultyOptionsQuestion, Models.Question.MultiOptionsQuestion>()
                .IncludeBase<AbstractQuestion, Models.Question.AbstractQuestion>()
                .ForMember(x => x.DisplayMode, x => x.MapFrom(s =>
                    s.YesNoView ? MultiOptionsDisplayMode.YesNo : 
                        (s.IsFilteredCombobox == true ? MultiOptionsDisplayMode.Combobox : MultiOptionsDisplayMode.Checkboxes)))
                .ForMember(s => s.LinkedTo, t => t.MapFrom((s, d, value, context) => 
                    GetVarName(s.LinkedToQuestionId ?? s.LinkedToRosterId, context)))
                .AfterMap((s, t) => 
                {
                    if (s.LinkedToQuestionId.HasValue || s.LinkedToRosterId.HasValue)
                        t.FilterExpression = s.LinkedFilterExpression;
                    else
                        t.FilterExpression = s.Properties?.OptionsFilterExpression;
                });
            this.CreateMap<Models.Question.MultiOptionsQuestion, MultyOptionsQuestion>()
                .IncludeBase<Models.Question.AbstractQuestion, AbstractQuestion>()
                .ForMember(x => x.YesNoView, x => x.MapFrom(s => s.DisplayMode == MultiOptionsDisplayMode.YesNo))
                .ForMember(x => x.IsFilteredCombobox, x => x.MapFrom(s => 
                    s.DisplayMode == MultiOptionsDisplayMode.Combobox ? true : (bool?)null))
                .ForMember(s => s.LinkedToQuestionId, o => o.MapFrom((s, d, value, context) 
                    => GetId(s.LinkedTo, context)))
                .ForMember(s => s.LinkedToRosterId, o => o.MapFrom((s, d, value, context) 
                    => GetId(s.LinkedTo, context)))
                .AfterMap((s, d) =>
                {
                    if (!s.LinkedTo.IsNullOrEmpty())
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
        
        private string? GetVarName(Guid? id, ResolutionContext context)
        {
            if (!id.HasValue)
                return null;
            var varName = ((Dictionary<Guid, string>)context.Items[ImportExportQuestionnaireConstants.MapCollectionName])[id.Value];
            if (varName?.Trim().IsNullOrEmpty() ?? true)
                return null;
            return varName;
        }

        private Guid GetIdOrGenerate(string? variableName, Guid? id, ResolutionContext context)
        {
            if (id.HasValue)
                return id.Value;
            if (variableName == null || variableName.Trim().IsNullOrEmpty())
                return Guid.NewGuid();
            return ((Dictionary<string, Guid>)context.Items[ImportExportQuestionnaireConstants.MapCollectionName])[variableName];
        }

        private Guid? GetId(string? variableName, ResolutionContext context)
        {
            if (variableName == null || variableName.Trim().IsNullOrEmpty())
                return null;
            return ((Dictionary<string, Guid>)context.Items[ImportExportQuestionnaireConstants.MapCollectionName])[variableName];
        }

        private bool IsIdentified(IComposite composite)
        {
            if (composite is IQuestion question)
                return question.Featured;
            return false;
        }
    }
}