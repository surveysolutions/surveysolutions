using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview.Impl
{
    internal class SampleWebInterviewService : ISampleWebInterviewService
    {
        protected internal const string AssignmentLink = "assignment__link";
        protected internal const string AssignmentId = "assignment__id";
        protected internal const string AssignmentPassword = "assignment__password";
        protected internal const string AssignmentEmail = "assignment__email";
        private readonly IInvitationService invitationService;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public SampleWebInterviewService(IQuestionnaireStorage questionnaireStorage, IInvitationService invitationService)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.invitationService = invitationService;
        }

        public byte[] Generate(QuestionnaireIdentity questionnaire, string baseUrl)
        {
            var invitationsToExport = this.GetInvitations(questionnaire);

            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                IgnoreQuotes = false,
                Delimiter = "\t",
                MissingFieldFound = null
            };

            var questionnaireDocument = this.questionnaireStorage.GetQuestionnaire(questionnaire, null);

            using MemoryStream output = new MemoryStream();
            using (StreamWriter streamWriter = new StreamWriter(output))
            {
                using (CsvWriter csvWriter = new CsvWriter(streamWriter, csvConfiguration))
                {
                    var header = this.WriteHeaderRow(csvWriter, questionnaireDocument, invitationsToExport);
                    foreach (Invitation invitation in invitationsToExport)
                    {
                        PushAssignment(header, invitation, questionnaireDocument, csvWriter, baseUrl);
                    }
                }
            }

            return output.ToArray();
        }

        private List<Invitation> GetInvitations(QuestionnaireIdentity questionnaireIdentity)
        {
            return invitationService.GetInvitationsToExport(questionnaireIdentity);
        }

        private List<string> WriteHeaderRow(CsvWriter csvWriter, IQuestionnaire questionnaire, List<Invitation> sampleAssignment)
        {
            List<string> header = new List<string>();
            header.Add(AssignmentLink);
            header.Add(AssignmentId);
            header.Add(AssignmentEmail);
            header.Add(AssignmentPassword);

            foreach (var identifyingAnswer in sampleAssignment.SelectMany(x => x.Assignment.IdentifyingData))
            {
                if (identifyingAnswer != null)
                {
                    var variableName = questionnaire.GetQuestionVariableName(identifyingAnswer.Identity.Id);
                    if (!header.Contains(variableName))
                    {
                        header.Add(variableName);
                    }
                }
            }

            foreach (var column in header)
            {
                csvWriter.WriteField(column);
            }

            csvWriter.NextRecord();
            return header;
        }

        private static void PushAssignment(List<string> header,
            Invitation invitation,
            IQuestionnaire questionnaire,
            CsvWriter csvWriter,
            string baseUrl)
        {
            foreach (var columnName in header)
            {
                if (columnName.Equals(AssignmentLink, StringComparison.Ordinal))
                {
                    csvWriter.WriteField($"{baseUrl}/{invitation.Token}/Start");
                }
                else if (columnName.Equals(AssignmentId, StringComparison.Ordinal))
                {
                    csvWriter.WriteField(invitation.AssignmentId);
                }
                else if (columnName.Equals(AssignmentEmail, StringComparison.Ordinal))
                {
                    csvWriter.WriteField(invitation.Assignment.Email);
                }
                else if (columnName.Equals(AssignmentPassword, StringComparison.Ordinal))
                {
                    csvWriter.WriteField(invitation.Assignment.Password);
                }
                else
                {
                    var questionId = questionnaire.GetQuestionIdByVariable(columnName);
                    var answer = invitation.Assignment.IdentifyingData.FirstOrDefault(x => x.Identity.Id == questionId);
                    csvWriter.WriteField(answer?.Answer ?? string.Empty);
                }
            }
            csvWriter.NextRecord();
        }
    }
}
