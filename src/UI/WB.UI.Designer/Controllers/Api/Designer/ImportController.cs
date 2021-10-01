using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Classifications;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Api.Designer.Qbank;
using WB.UI.Designer.Extensions;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [ResponseCache(NoStore = true)]
    [Authorize(Roles = "Administrator")]
    [Route("api/import")]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class ImportController : Controller
    {
        private readonly IClassificationsStorage classificationsStorage;
        private readonly ICommandService commandService;
        private readonly IPublicFoldersStorage publicFoldersStorage;
        private readonly IWebHostEnvironment hostingEnvironment;

        public ImportController(IClassificationsStorage classificationsStorage, 
            ICommandService commandService, 
            IPublicFoldersStorage publicFoldersStorage,
            IWebHostEnvironment hostingEnvironment)
        {
            this.classificationsStorage = classificationsStorage;
            this.commandService = commandService;
            this.publicFoldersStorage = publicFoldersStorage;
            this.hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        [Route("questionnaires")]
        public void ImportQuestionnaires()
        {
            var foldersAndQuestionnaires = NewMethod();
            var questionnairesIds = foldersAndQuestionnaires.Select(x => x.Id)
                .Except(foldersAndQuestionnaires.Select(x => x.Pid))
                .ToHashSet();

            var mysqlPublicFolders = foldersAndQuestionnaires.Where(x => !questionnairesIds.Contains(x.Id)).ToList();
            mysqlPublicFolders.ForEach(x => x.IdGuid = Guid.Parse(x.Id.ToString().PadLeft(32, '1')));
            var publicFoldersIdMap = mysqlPublicFolders.ToDictionary(x => x.Id, x => x.IdGuid);
            mysqlPublicFolders.ForEach(x =>
                publicFoldersStorage.CreateFolder(x.IdGuid, x.Name, publicFoldersIdMap.ContainsKey(x.Pid) ? publicFoldersIdMap[x.Pid] : (Guid?)null,
                    User.GetId())
            );

            var mysqlAllQuestions = NewMethod1();

            var mysqlAllSections = NewMethod2();

            var sectionToQuestionnaires = NewMethod3();

            var mysqlOptions = NewMethod4();

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
                        var variable = mysqlQuestion.Name ?? String.Empty;
                        var title = ((mysqlQuestion.Description?? string.Empty) + Environment.NewLine + (mysqlQuestion.Literal_text?? string.Empty)).Trim();
                        switch (mysqlQuestion.Visual_rep_format)
                        {
                             case 0:
                             case 2:
                                 item = new TextQuestion(title)
                                 {
                                     PublicKey = Guid.NewGuid(),
                                     QuestionScope = QuestionScope.Interviewer,
                                     StataExportCaption = variable,
                                     VariableLabel = variableLabel,
                                     Instructions = instructions
                                 };
                                 break;
                             case 1:
                                 item = new NumericQuestion(title){
                                     PublicKey = Guid.NewGuid(),
                                     QuestionScope = QuestionScope.Interviewer,
                                     StataExportCaption = variable,
                                     VariableLabel = variableLabel,
                                     Instructions = instructions,
                                     IsInteger = true
                                 };
                                 break;
                             case 3:
                               
                                 if (mysqlQuestion.Classification_id.HasValue && mysqlQuestion.Classification_id.Value!=0)
                                 {
                                     var classificationId = mysqlQuestion.Classification_id.Value;

                                     var options = mysqlOptions.Where(x => x.Classification_id == classificationId)
                                         .OrderBy(x => x.Weight)
                                         .Select(x => new Answer
                                         {
                                             AnswerText = x.Classification_label,
                                             AnswerValue = int.TryParse(x.Value, out int code) 
                                                 ? code.ToString() 
                                                 : ((int)x.Value.ToUpper().Select((c, i) => (c - 'A' + 1)*Math.Pow(10, i)).Sum()).ToString()
                                         }).ToList();
                                         
                                     item = new SingleQuestion(title)
                                     {
                                         PublicKey = Guid.NewGuid(),
                                         QuestionScope = QuestionScope.Interviewer, 
                                         StataExportCaption = variable,
                                         VariableLabel = variableLabel,
                                         Instructions = instructions,
                                         Answers = options
                                     };
                                 }
                                 else
                                 {
                                     item = new TextQuestion(title)
                                     {
                                         PublicKey = Guid.NewGuid(),
                                         QuestionScope = QuestionScope.Interviewer,
                                         StataExportCaption = variable,
                                         VariableLabel = variableLabel,
                                         Instructions = instructions
                                     };
                                 }
                                 
                                 break;
                             default:
                                 item = new StaticText(Guid.NewGuid(), title, string.Empty, false, null);
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
                    Title = mysqlQuestionnaire.Name ?? "",
                    Description = mysqlQuestionnaire.Description,
                    Children = new ReadOnlyCollection<IComposite>(sections),
                    IsPublic = true,
                    HideIfDisabled = true,
                    VariableName = "Q_" + varNumber.ToString().PadLeft(4, '0'),
                    CreatedBy = User.GetId(),
                    CreationDate = DateTime.UtcNow,
                    IsDeleted = false
                };
                
                var command = new ImportQuestionnaire(User.GetId(), questionnaire);
                commandService.Execute(command);
                Guid? folderId = publicFoldersIdMap.ContainsKey(mysqlQuestionnaire.Pid)
                    ? publicFoldersIdMap[mysqlQuestionnaire.Pid]
                    : (Guid?) null;
                publicFoldersStorage.AssignFolderToQuestionnaire(questionnaire.PublicKey, folderId);
           
                varNumber++;
            }
        }

        private List<MySqlFoldersAndQuestionnaires> NewMethod()
        {
            var foldersAndQuestionnairesJson =
                System.IO.File.ReadAllText(this.hostingEnvironment.MapPath("Content/qbank/FoldersAndQuestionnaires.json"));
            var foldersAndQuestionnaires = JsonConvert
                .DeserializeObject<MySqlFoldersAndQuestionnaires[]>(foldersAndQuestionnairesJson)
                .Where(x => x.Published == 1)
                .ToList();
            return foldersAndQuestionnaires;
        }

        private List<MySqlOptions> NewMethod4()
        {
            var questionsWithOptionsJson =
                System.IO.File.ReadAllText(this.hostingEnvironment.MapPath("Content/qbank/Options.json"));

            var questionsWithOptions = JsonConvert
                .DeserializeObject<MySqlOptions[]>(questionsWithOptionsJson)
                .ToList();
            return questionsWithOptions;
        }

        private List<MySqlSectionToQuestionnaires> NewMethod3()
        {
            var sectionToQuestionnairesJson =
                System.IO.File.ReadAllText(this.hostingEnvironment.MapPath("Content/qbank/SectionToQuestionnaires.json"));


            var sectionToQuestionnaires = JsonConvert
                .DeserializeObject<MySqlSectionToQuestionnaires[]>(sectionToQuestionnairesJson)
                .ToList();
            return sectionToQuestionnaires;
        }

        private List<MySqlSections> NewMethod2()
        {
            var sectionsJson = System.IO.File.ReadAllText(this.hostingEnvironment.MapPath("Content/qbank/Sections.json"));
            var sections = JsonConvert.DeserializeObject<MySqlSections[]>(sectionsJson)
                .Where(x => x.Published == 1)
                .ToList();
            return sections;
        }

        private List<MySqlQuestions> NewMethod1()
        {
            var questionsJson = System.IO.File.ReadAllText(this.hostingEnvironment.MapPath("Content/qbank/Questions.json"));
            var questions = JsonConvert.DeserializeObject<MySqlQuestions[]>(questionsJson).ToList();
            return questions;
        }

        [HttpPost]
        [Route("classifications")]
        public void Init()
        {
            var entities = ReadAndParseClassifications();
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
                Title = x.Title ?? "",
                Type = x.Type,
                Index = x.Order,
                Parent = x.ParentGuid,
                ClassificationId = x.ClassificationId
            }).ToArray();

            classificationsStorage.Store(bdEntities);
        }

        private MysqlClassificationEntity[] ReadAndParseClassifications()
        {
            var json = System.IO.File.ReadAllText(this.hostingEnvironment.MapPath("Content/qbank/QbankClassifications.json"));
            var entities = JsonConvert.DeserializeObject<MysqlClassificationEntity[]>(json);
            return entities;
        }
    }

}
