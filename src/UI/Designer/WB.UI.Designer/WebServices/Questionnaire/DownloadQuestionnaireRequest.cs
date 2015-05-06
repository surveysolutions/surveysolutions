using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;

namespace WB.UI.Designer.WebServices.Questionnaire
{
    /// <summary>
    /// The download questionnaire request.
    /// </summary>
    [MessageContract]
    public class DownloadQuestionnaireRequest
    {
        [MessageHeader]
        public Guid QuestionnaireId { get; set; }

        [MessageHeader]
        public QuestionnnaireVersion SupportedQuestionnaireVersion { get; set; }
    }
    [Obsolete("please do not remove this class, it is here for backward compatibility with HQ version smaller then 4")]
    [DataContract]
    public sealed class QuestionnaireVersion
    {
        [DataMember]
        public int Major { get; set; }

        [DataMember]
        public int Minor { get; set; }

        [DataMember]
        public int Patch { get; set; }

        public QuestionnaireVersion()
        {
        }

        public QuestionnaireVersion(int major, int minor, int patch)
        {
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
        }
    }
} 