using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class ReadSideRepositoryDataExportWriter : IDataExportWriter
    {
        private readonly IReadSideRepositoryWriter<InterviewExportedDataRecord> interviewExportedDataStorage;
        private readonly IReadSideRepositoryWriter<InterviewHistory> interviewActionsDataStorage;
        private readonly IJsonUtils jsonUtils;

        public ReadSideRepositoryDataExportWriter(
            IReadSideRepositoryWriter<InterviewExportedDataRecord> interviewExportedDataStorage, 
            IReadSideRepositoryWriter<InterviewHistory> interviewActionsDataStorage, IJsonUtils jsonUtils)
        {
            this.interviewExportedDataStorage = interviewExportedDataStorage;
            this.interviewActionsDataStorage = interviewActionsDataStorage;
            this.jsonUtils = jsonUtils;
        }

        public void AddActionRecord(InterviewActionExportView action, Guid questionnaireId, long questionnaireVersion)
        {
            var history = interviewActionsDataStorage.GetById(action.InterviewId);
            if (history == null)
            {
                history = new InterviewHistory()
                {
                    InterviewId = action.InterviewId,
                    QuestionnaireId = questionnaireId,
                    QuestionnaireVersion = questionnaireVersion
                };
            }

            history.InterviewActions.Add(CreateInterviewAction(action));

            interviewActionsDataStorage.Store(history, history.InterviewId);

            var interviewExportedData = interviewExportedDataStorage.GetById(history.InterviewId);
            if (interviewExportedData != null)
            {
                interviewExportedData.LastAction = action.Action.ToString();
                interviewExportedDataStorage.Store(interviewExportedData, history.InterviewId);
            }
        }

        public void AddOrUpdateInterviewRecords(InterviewDataExportView item, Guid questionnaireId, long questionnaireVersion)
        {
            DeleteInterviewExportedDataByInterviewId(item.InterviewId);

            var interviewExportedData = CreateInterviewExportedData(item, questionnaireId, questionnaireVersion);

            interviewExportedDataStorage.Store(interviewExportedData, item.InterviewId);
        }

        public void DeleteInterviewRecords(Guid interviewId)
        {
            DeleteInterviewExportedDataByInterviewId(interviewId);
            DeleteInterviewActionsByInterviewId(interviewId.FormatGuid());
        }

        private void DeleteInterviewExportedDataByInterviewId(Guid interviewId)
        {
            interviewExportedDataStorage.Remove(interviewId);
        }

        private void DeleteInterviewActionsByInterviewId(string interviewId)
        {
            interviewActionsDataStorage.Remove(interviewId);
        }

        private InterviewAction CreateInterviewAction(InterviewActionExportView action)
        {
            return new InterviewAction()
            {
                Action = action.Action.ToString(),
                Originator = action.Originator,
                Role = action.Role,
                Timestamp = action.Timestamp
            };
        }

        private InterviewExportedDataRecord CreateInterviewExportedData(InterviewDataExportView interviewDataExportView, Guid questionnaireId, long questionnaireVersion)
        {
            var interviewData = new Dictionary<string, string[]>();
            
            var stringSeparator = ExportFileSettings.SeparatorOfExportedDataFile.ToString();
            foreach (var interviewDataExportLevelView in interviewDataExportView.Levels)
            {
                var recordsByLevel = new List<string>();
                foreach (var interviewDataExportRecord in interviewDataExportLevelView.Records)
                {
                    var parametersToConcatenate = new List<string> {interviewDataExportRecord.RecordId};
                    
                    parametersToConcatenate.AddRange(interviewDataExportRecord.ReferenceValues);

                    foreach (var exportedQuestion in interviewDataExportRecord.Questions)
                    {
                        parametersToConcatenate.AddRange(exportedQuestion.Answers.Select(itemValue => string.IsNullOrEmpty(itemValue) ? "" : itemValue));
                    }

                    parametersToConcatenate.AddRange(interviewDataExportRecord.ParentRecordIds);

                    recordsByLevel.Add(string.Join(stringSeparator,
                            parametersToConcatenate.Select(v => v.Replace(stringSeparator, ""))));
                }
                interviewData.Add(interviewDataExportLevelView.LevelName, recordsByLevel.ToArray());
            }
            var interviewExportedData = new InterviewExportedDataRecord
            {
                InterviewId = interviewDataExportView.InterviewId.FormatGuid(),
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                Data = jsonUtils.SerializeToByteArray(interviewData)
            };

            return interviewExportedData;
        }

        public void Clear()
        {
            ((IReadSideRepositoryCleaner)interviewActionsDataStorage).Clear();
            ((IReadSideRepositoryCleaner)interviewExportedDataStorage).Clear();
        }

        public Type ViewType
        {
            get { return GetType(); }
        }

        public string GetReadableStatus()
        {
            var status1 = ((IChacheableRepositoryWriter) interviewActionsDataStorage).GetReadableStatus();
            var status2 = ((IChacheableRepositoryWriter)interviewExportedDataStorage).GetReadableStatus();
            return status1 + Environment.NewLine + status2;
        }

        public void EnableCache()
        {
            ((IChacheableRepositoryWriter)interviewActionsDataStorage).EnableCache();
            ((IChacheableRepositoryWriter)interviewExportedDataStorage).EnableCache();
        }

        public void DisableCache()
        {
            ((IChacheableRepositoryWriter)interviewActionsDataStorage).DisableCache();
            ((IChacheableRepositoryWriter)interviewExportedDataStorage).DisableCache();
        }

        public bool IsCacheEnabled
        {
            get { return ((IChacheableRepositoryWriter) interviewActionsDataStorage).IsCacheEnabled; }
        }
    }

    public class InterviewExportedDataRecord : IView
    {
        public virtual string InterviewId { get; set; }
        public virtual Guid QuestionnaireId { get; set; }
        public virtual long QuestionnaireVersion { get; set; }
        public virtual byte[] Data { get; set; }
        public virtual string LastAction { get; set; }
    }

    public class InterviewExportedDataMap : ClassMapping<InterviewExportedDataRecord>
    {
        public InterviewExportedDataMap()
        {
            Table("InterviewExportedData");

            Id(x => x.InterviewId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });
            Property(x => x.Data);
            Property(x => x.QuestionnaireId);
            Property(x => x.QuestionnaireVersion);
            Property(x => x.LastAction);
        }
    }

    public class InterviewHistory : IView
    {
        public InterviewHistory()
        {
            this.InterviewActions = new HashSet<InterviewAction>();
        }

        public virtual string InterviewId { get; set; }
        public virtual Guid QuestionnaireId { get; set; }
        public virtual long QuestionnaireVersion { get; set; }
        public virtual ISet<InterviewAction> InterviewActions { get; set; }
    }

    public class InterviewAction
    {
        public virtual int Id { get; set; }
        public virtual string Action { get; set; }
        public virtual string Originator { get; set; }
        public virtual string Role { get; set; }
        public virtual DateTime Timestamp { get; set; }
        public virtual InterviewHistory History { get; set; }
    }

    public class InterviewActionsMap : ClassMapping<InterviewHistory>
    {
        public InterviewActionsMap()
        {
            Table("InterviewHistory");

            Id(x => x.InterviewId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });
            Property(x => x.QuestionnaireId);
            Property(x => x.QuestionnaireVersion);

            Set(x => x.InterviewActions, set =>
            {
                set.Key(key => key.Column("InterviewId"));
                set.Lazy(CollectionLazy.NoLazy);
                set.Cascade(Cascade.All | Cascade.DeleteOrphans);
            },
            relation => relation.OneToMany());
        }
    }

    public class InterviewActionMap : ClassMapping<InterviewAction>
    {
        public InterviewActionMap()
        {
            Id(x => x.Id, Idmap => Idmap.Generator(Generators.HighLow));
            Property(x => x.Action);
            Property(x => x.Originator);
            Property(x => x.Role);
            Property(x => x.Timestamp);
            ManyToOne(x => x.History, mto =>
            {
                mto.Index("InterviewHistorys_InterviewActions");
                mto.Cascade(Cascade.None);
            });
        }
    }
}
