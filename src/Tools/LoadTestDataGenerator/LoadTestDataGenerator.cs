using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Main.Core.Commands.Questionnaire;
using Main.Core.Commands.Questionnaire.Completed;
using Main.Core.Commands.User;
using Main.Core.Documents;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Entities.SubEntities.Complete.Question;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Utility;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Restoring.EventStapshoot;
using Newtonsoft.Json;

namespace LoadTestDataGenerator
{
    public partial class LoadTestDataGenerator : Form
    {
        protected readonly ICommandService CommandService;

        public LoadTestDataGenerator(ICommandService commandService)
        {
            this.CommandService = commandService;
            InitializeComponent();
        }

        private void generate_Click(object sender, EventArgs e)
        {
            this.GenerateSupervisorsEvents();
        }

        private void GenerateSupervisorsEvents()
        {
            var template = this.ReadTemplate(this.templatePath.Text);
            var hq = this.GenerateHeadquarter();
            this.StoreTemplate(template, hq);
            var featuredQuestions = template.GetFeaturedQuestions();
            var supervisors = this.GenerateSupervisors(int.Parse(this.supervisorsCount.Text));
            var interviewers = this.GenerateInterviewers(int.Parse(this.interviewersCount.Text), supervisors);
            var surveyIds = this.GenerateSurveys(int.Parse(this.surveys_amount.Text), template, hq);
            foreach (var surveyId in surveyIds)
            {
                this.FillFeaturedAnswers(surveyId, featuredQuestions, hq);
                var responsible = this.AssignSurveyToOneOfSupervisors(surveyId, supervisors, hq);
                var supervisorTeamMembers = interviewers.Where(x => x.Supervisor.Id == responsible.PublicKey).ToList();
                this.AssignSurveyToOneOfInterviewers(surveyId, supervisorTeamMembers);
                this.MarkSurveysAsReadyForSyncWithStatus(SurveyStatus.Initial, surveyId, responsible);
            }
            foreach (var surveyId in surveyIds)
            {
                this.CreateSnapshoot(surveyId);
            }
        }

        private QuestionnaireDocument ReadTemplate(string path)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            var template = JsonConvert.DeserializeObject<QuestionnaireDocument>(File.OpenText(path).ReadToEnd(), settings);
            return template;
        }

        private UserDocument GenerateHeadquarter()
        {
            var hq = new UserDocument()
                {
                    PublicKey = Guid.NewGuid(),
                    UserName = "hq",
                    Roles = new List<UserRoles>(){ UserRoles.Headquarter}
                };
            CommandService.Execute(new CreateUserCommand(hq.PublicKey, hq.UserName, SimpleHash.ComputeHash(hq.UserName), hq.UserName + "@worldbank.org", hq.Roles.ToArray(), false, null));
            
            return hq;
        }

        private void StoreTemplate(QuestionnaireDocument template, UserDocument initiator)
        {
            this.CommandService.Execute(new ImportQuestionnaireCommand(initiator.PublicKey, template));
        }

        private List<UserDocument> GenerateSupervisors(int count)
        {
            var result = new List<UserDocument>();
            for (int i = 0; i < count; i++)
            {
                var supervisor = new UserDocument()
                {
                    PublicKey = Guid.NewGuid(),
                    UserName = "supervisor" + i,
                    Roles = new List<UserRoles> { UserRoles.Supervisor }
                };
                CommandService.Execute(new CreateUserCommand(supervisor.PublicKey, supervisor.UserName, SimpleHash.ComputeHash(supervisor.UserName), supervisor.UserName + "@worldbank.org", supervisor.Roles.ToArray(), false, null));
                result.Add(supervisor);
            }
            return result;
        }

        private List<UserDocument> GenerateInterviewers(int count, List<UserDocument> supervisors)
        {
            var rand = new Random();
            var result = new List<UserDocument>();
            var supervisorsCount = supervisors.Count;
            for (int i = 0; i < count; i++)
            {
                var supervisor = supervisors[rand.Next(supervisorsCount)];
                var interviewer = new UserDocument()
                {
                    PublicKey = Guid.NewGuid(),
                    UserName = "interviewer" + i,
                    Supervisor = supervisor.ToUserLight(),
                    Roles = new List<UserRoles> { UserRoles.Operator }
                };
                CommandService.Execute(new CreateUserCommand(interviewer.PublicKey, interviewer.UserName, SimpleHash.ComputeHash(interviewer.UserName), interviewer.UserName + "@worldbank.org", interviewer.Roles.ToArray(), false, interviewer.Supervisor));
                result.Add(interviewer);
            }
            return result;
        }

        private IEnumerable<Guid> GenerateSurveys(int count, QuestionnaireDocument template, UserDocument initiator)
        {
            var result = new List<Guid>();
            for (int i = 0; i < count; i++)
            {
                var id = Guid.NewGuid();
                this.CommandService.Execute(new CreateCompleteQuestionnaireCommand(id, template.PublicKey, initiator.ToUserLight()));
                result.Add(id);
            }
            return result;
        }

        private void FillFeaturedAnswers(Guid surveyId, IEnumerable<IQuestion> featuredQuestions, UserDocument initiator)
        {
            foreach (var question in featuredQuestions)
            {
                this.CommandService.Execute(new SetAnswerCommand(surveyId, question.PublicKey, GetDummyAnswers(question), GetDummyAnswer(question), null));
            }
        }

        private static string GetDummyAnswer(IQuestion q)
        {
            var rand = GetRandom();
            if (q is IMultyOptionsQuestion)
            {
                return null;
            }
            if (q is ISingleQuestion)
            {
                var question = (SingleQuestion)q;
                var answersCount = question.Answers.Count;
                return question.Answers[rand.Next(answersCount)].PublicKey.ToString();
            }
            if (q is IDateTimeQuestion)
            {
                return (new DateTime(rand.Next(1940, 2003), rand.Next(1, 13), rand.Next(1, 29))).ToString(CultureInfo.InvariantCulture);
            }
            if (q is INumericQuestion)
            {
                return rand.Next(100).ToString(CultureInfo.InvariantCulture);
            }
            if (q is IAutoPropagate)
            {
                var question = (AutoPropagateQuestion)q;
                var maxValue = question.MaxValue;
                return rand.Next(maxValue/2, maxValue + 1).ToString(CultureInfo.InvariantCulture);
            }
            if (q is ITextCompleteQuestion)
            {
                return "value " + rand.Next();
            }
            return "";
        }

        private List<Guid> GetDummyAnswers(IQuestion q)
        {
            if (q is IMultyOptionsQuestion)
            {
                var random = GetRandom();
                var rand = random;
                var question = (MultyOptionsQuestion)q;
                var answersCount = question.Answers.Count;
                var selectedAnswersCount = rand.Next(1, answersCount + 1);
                var result = new List<Guid>();
                for (int i = 0; i < selectedAnswersCount; i++)
                {
                    var answer = question.Answers[rand.Next(0, answersCount)];
                    result.Add(answer.PublicKey);
                }
                return result.Distinct().ToList();
            }
            return new List<Guid>();
        }

        private static Random GetRandom()
        {
            var random = new Random((int) (new DateTime()).Ticks);
            return random;
        }

        private UserDocument AssignSurveyToOneOfSupervisors(Guid surveyId, List<UserDocument> supervisors, UserDocument hq)
        {
            var rand = GetRandom();
            var responsibleSupervisor = supervisors[rand.Next(supervisors.Count)];
            this.CommandService.Execute(new ChangeAssignmentCommand(surveyId, responsibleSupervisor.ToUserLight()));
            return responsibleSupervisor;
        }

        private void AssignSurveyToOneOfInterviewers(Guid surveyId, List<UserDocument> interviewers)
        {
            var rand = GetRandom();
            var responsibleInterviewer = interviewers[rand.Next(interviewers.Count)];
            this.CommandService.Execute(new ChangeAssignmentCommand(surveyId, responsibleInterviewer.ToUserLight()));
        }

        private void MarkSurveysAsReadyForSyncWithStatus(SurveyStatus surveyStatus, Guid surveyId, UserDocument initiator)
        {
            this.CommandService.Execute(new ChangeStatusCommand(){
                           CompleteQuestionnaireId = surveyId,
                           Status = surveyStatus,
                           Responsible = initiator.ToUserLight()
                       });
        }

        private void CreateSnapshoot(Guid surveyId)
        {
            this.CommandService.Execute(new CreateSnapshotForAR(surveyId, typeof(CompleteQuestionnaireAR)));
        }

        private void templatePath_Enter(object sender, EventArgs e)
        {
            var templateFileChooser = new OpenFileDialog
                {
                    InitialDirectory = "d:\\",
                    Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
                    FilterIndex = 2,
                    RestoreDirectory = true
                };

            if (templateFileChooser.ShowDialog() == DialogResult.OK)
            {
                templatePath.Text = templateFileChooser.FileName;
            }
        }
    }
}
