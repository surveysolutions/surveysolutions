using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Http;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.Classifications;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Api.Designer.Qbank;
using WB.UI.Designer.Filters;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Designer.Api.Designer
{
    [ApiNoCache]
    [Authorize(Roles = "Administrator")]
    [RoutePrefix("api/import")]
    [CamelCase]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class ImportController : ApiController
    {
        private readonly IClassificationsStorage classificationsStorage;
        private readonly ICommandService commandService;
        protected readonly IMembershipUserService UserHelper;

        public ImportController(IClassificationsStorage classificationsStorage, 
            ICommandService commandService, 
            IMembershipUserService userHelper)
        {
            this.classificationsStorage = classificationsStorage;
            this.commandService = commandService;
            UserHelper = userHelper;
        }

        [HttpPost]
        [Route("questionnaires")]
        public void ImportQuestionnaires()
        {
            var foldersAndQuestionnaires = NewMethod();
            var questionnairesIds = foldersAndQuestionnaires.Select(x => x.Id)
                .Except(foldersAndQuestionnaires.Select(x => x.Pid))
                .ToHashSet();
            
            var mysqlAllQuestions = NewMethod1();

            var mysqlAllSections = NewMethod2();

            var sectionToQuestionnaires = NewMethod3();

            //var questionsWithOptions = NewMethod4();
            //var categoricalQuestions = questionsWithOptions.Select(x => x.Classification_code_id).ToHashSet();

            var mysqlQuestionnaires = foldersAndQuestionnaires.Where(x => questionnairesIds.Contains(x.Id));
            var varNumber = 1;
            foreach (var mysqlQuestionnaire in mysqlQuestionnaires)
            {
                var sectionIds = sectionToQuestionnaires.Where(x => x.Quest_group_id == mysqlQuestionnaire.Id).Select(x => x.Quest_module_id).ToHashSet();
                var mysqlSections = mysqlAllSections.Where(x => sectionIds.Contains(x.Id))
                    .Where(x => x.Published == 1)
                    .OrderBy(x => x.Weight)
                    .ThenBy(x => x.Name)
                    .ToList();

                var sections = new List<IComposite>();
                foreach (var mySqlSection in mysqlSections)
                {
                    var mysqlQuestions = mysqlAllQuestions.Where(x => x.Quest_module_id == mySqlSection.Id)
                        .OrderBy(x => x.Weight)
                        .ThenBy(x => x.Description)
                        .ToList();

                    var children = new List<IComposite>();
                    foreach (var mysqlQuestion in mysqlQuestions)
                    {
                        IComposite item;
                        var variableLabel = mysqlQuestion.Description;
                        var instructions = ((mysqlQuestion.Pre_text ?? string.Empty) + Environment.NewLine + (mysqlQuestion.Post_text?? string.Empty)).Trim();
                        var variable = mysqlQuestion.Name;
                        var title = ((mysqlQuestion.Description?? string.Empty) + Environment.NewLine + (mysqlQuestion.Literal_text?? string.Empty)).Trim();
                        switch (mysqlQuestion.Visual_rep_format)
                        {
                             case 0:
                             case 2:
                                 item = new TextQuestion(title)
                                 {
                                     PublicKey = Guid.NewGuid(),
                                     QuestionType = QuestionType.Text,
                                     QuestionScope = QuestionScope.Interviewer,
                                     StataExportCaption = variable,
                                     VariableLabel = variableLabel,
                                     Instructions = instructions
                                 };
                                 break;
                             case 1:
                                 item = new NumericQuestion(title){
                                     PublicKey = Guid.NewGuid(),
                                     QuestionType = QuestionType.Numeric,
                                     QuestionScope = QuestionScope.Interviewer,
                                     StataExportCaption = variable,
                                     VariableLabel = variableLabel,
                                     Instructions = instructions,
                                     IsInteger = true
                                 };
                                 break;
                             case 3:
                                 item = new SingleQuestion(title)
                                 {
                                     PublicKey = Guid.NewGuid(),
                                     QuestionType = QuestionType.SingleOption,
                                     QuestionScope = QuestionScope.Interviewer, 
                                     StataExportCaption = variable,
                                     VariableLabel = variableLabel,
                                     Instructions = instructions
                                 };
                                 break;
                             default:
                                 item = new StaticText(Guid.NewGuid(), title, null, false, null);
                                 break;
                        }
                        children.Add(item);
                    }

                    var section = new Group(mySqlSection.Name, children);
                    sections.Add(section);
                }

                var questionnaire = new QuestionnaireDocument
                {
                    PublicKey = Guid.Parse(varNumber.ToString().PadLeft(32, '0')),
                    Title = mysqlQuestionnaire.Name,
                    Description = mysqlQuestionnaire.Description,
                    Children = sections.ToReadOnlyCollection(),
                    IsPublic = true,
                    HideIfDisabled = true,
                    VariableName = "Q_" + varNumber.ToString().PadLeft(4),
                    CreatedBy = this.UserHelper.WebUser.UserId,
                    CreationDate = DateTime.UtcNow,
                    IsDeleted = false
                };

                var command = new ImportQuestionnaire(this.UserHelper.WebUser.UserId, questionnaire);
                commandService.Execute(command);
                break;
            }
        }

        private static List<MySqlFoldersAndQuestionnaires> NewMethod()
        {
            var foldersAndQuestionnairesJson =
                File.ReadAllText(HostingEnvironment.MapPath("~/Content/qbank/FoldersAndQuestionnaires.json"));
            var foldersAndQuestionnaires = JsonConvert
                .DeserializeObject<MySqlFoldersAndQuestionnaires[]>(foldersAndQuestionnairesJson)
                .Where(x => x.Published == 1)
                .ToList();
            return foldersAndQuestionnaires;
        }


        private static List<MySqlQuestionsWithOptions> NewMethod4()
        {
            var questionsWithOptionsJson =
                File.ReadAllText(HostingEnvironment.MapPath("~/Content/qbank/QuestionsWithOptions.json"));

            var questionsWithOptions = JsonConvert
                .DeserializeObject<MySqlQuestionsWithOptions[]>(questionsWithOptionsJson)
                .ToList();
            return questionsWithOptions;
        }

        private static List<MySqlSectionToQuestionnaires> NewMethod3()
        {
            var sectionToQuestionnairesJson =
                File.ReadAllText(HostingEnvironment.MapPath("~/Content/qbank/SectionToQuestionnaires.json"));


            var sectionToQuestionnaires = JsonConvert
                .DeserializeObject<MySqlSectionToQuestionnaires[]>(sectionToQuestionnairesJson)
                .ToList();
            return sectionToQuestionnaires;
        }

        private static List<MySqlSections> NewMethod2()
        {
            var sectionsJson = File.ReadAllText(HostingEnvironment.MapPath("~/Content/qbank/Sections.json"));
            var sections = JsonConvert.DeserializeObject<MySqlSections[]>(sectionsJson)
                .Where(x => x.Published == 1)
                .ToList();
            return sections;
        }

        private static List<MySqlQuestions> NewMethod1()
        {
            var questionsJson = File.ReadAllText(HostingEnvironment.MapPath("~/Content/qbank/Questions.json"));
            var questions = JsonConvert.DeserializeObject<MySqlQuestions[]>(questionsJson).ToList();
            return questions;
        }

        [HttpPost]
        [Route("classifications")]
        public void Init()
        {
            var json = File.ReadAllText(HostingEnvironment.MapPath("~/Content/QbankClassifications.json"));
            var entities = JsonConvert.DeserializeObject<MysqlClassificationEntity[]>(json);
            foreach (var entity in entities)
            {
                entity.IdGuid = Guid.NewGuid();
            }

            foreach (var entity in entities)
            {
                if (!entity.Parent.HasValue) continue;
                switch (entity.Type)
                {
                    case ClassificationEntityType.Group:
                        break;
                    case ClassificationEntityType.Classification:
                        var parenGroup = entities.FirstOrDefault(x => x.Id == entity.Parent.Value && x.Type == ClassificationEntityType.Group);
                        entity.ParentGuid = parenGroup?.IdGuid;
                        entity.ClassificationId = entity.IdGuid;
                        break;
                    case ClassificationEntityType.Category:
                        var parenClassification = entities.FirstOrDefault(x => x.Id == entity.Parent.Value && x.Type == ClassificationEntityType.Classification);
                        entity.ParentGuid = parenClassification?.IdGuid;
                        entity.ClassificationId = entity.ParentGuid;
                        break;
                }
            }

            var bdEntities = entities.Select(x => new ClassificationEntity
            {
                Id = x.IdGuid,
                Value = x.Value,
                Title = x.Title,
                Type = x.Type,
                Index = x.Order,
                Parent = x.ParentGuid,
                ClassificationId = x.ClassificationId
            }).ToArray();

            classificationsStorage.Store(bdEntities);
        }
    }
}
