﻿using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class GetQuestionnaireListRequest : ICommunicationMessage
    {
        public List<QuestionnaireIdentity> Questionnaires { get; set; } = new List<QuestionnaireIdentity>();
    }

    public class GetQuestionnaireListResponse : ICommunicationMessage
    {
        public List<QuestionnaireIdentity> Questionnaires { get; set; } = new List<QuestionnaireIdentity>();
    }
    
    public class GetQuestionnaireTranslationRequest : ICommunicationMessage
    {
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
    }

    public class GetQuestionnaireTranslationResponse : ICommunicationMessage
    {
        public List<TranslationDto> Translations { get; set; }
    }

    public class GetQuestionnaireReusableCategoriesRequest : ICommunicationMessage
    {
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
    }

    public class GetQuestionnaireReusableCategoriesResponse : ICommunicationMessage
    {
        public List<ReusableCategoriesDto> Categories { get; set; }
    }

    public class GetCompanyLogoRequest : ICommunicationMessage
    {
        public string Etag { get; set; }
    }

    public class GetCompanyLogoResponse : ICommunicationMessage
    {
        public CompanyLogoInfo LogoInfo { get; set; }
    }

    public class GetAttachmentContentsRequest : ICommunicationMessage
    {
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }

    }

    public class GetAttachmentContentsResponse : ICommunicationMessage
    {
        public List<string> AttachmentContents { get; set; }
    }

    public class GetAttachmentContentRequest : ICommunicationMessage
    {
        public string ContentId { get; set; }
    }

    public class GetAttachmentContentResponse : ICommunicationMessage
    {
        public AttachmentContent Content { get; set; }
    }

    public class UploadAuditLogEntityRequest : ICommunicationMessage
    {
        public AuditLogEntitiesApiView AuditLogEntity { get; set; }
    }

    public class UploadGeoTrackingPackageRequest : ICommunicationMessage
    {
        public GeoTrackingPackageApiView Package { get; set; }
    }

    public class UploadInterviewRequest : ICommunicationMessage
    {
        public string InterviewKey { get; set; }
        public InterviewPackageApiView Interview { get; set; }
    }

    public class GetInterviewUploadStateRequest : ICommunicationMessage
    {
        public Guid InterviewId { get; set; }
        public EventStreamSignatureTag Check { get; set; }
    }

    public class GetInterviewSyncInfoPackageRequest : ICommunicationMessage
    {
        public Guid InterviewId { get; set; }
        public InterviewSyncInfoPackage SyncInfoPackage { get; set; }
    }

    public class InterviewSyncInfoPackageResponse : ICommunicationMessage
    {
        public SyncInfoPackageResponse SyncInfoPackageResponse { get; set; }
    }

    public class GetInterviewUploadStateResponse : ICommunicationMessage
    {
        public Guid InterviewId { get; set; }
        public InterviewUploadState UploadState { get; set; }
    }

    public class GetAssignmentRequest : ICommunicationMessage
    {
        public int Id { get; set; }
    }
    public class GetAssignmentsRequest : ICommunicationMessage
    {
        public Guid UserId { get; set; }
    }

    public class GetAssignmentResponse : ICommunicationMessage
    {
        public AssignmentApiDocument Assignment { get; set; }
    }

    public class GetAssignmentsResponse : ICommunicationMessage
    {
        public List<AssignmentApiView> Assignments { get; set; }
    }

    public class LogAssignmentAsHandledRequest : ICommunicationMessage
    {
        public int Id { get; set; }
    }

    public class GetPublicKeyForEncryptionRequest: ICommunicationMessage { }

    public class GetPublicKeyForEncryptionResponse : ICommunicationMessage
    {
        public string PublicKey { get; set; }
    }

    public class UploadInterviewImageRequest : ICommunicationMessage
    {
        public PostFileApiView InterviewImage { get; set; }
    }
    public class UploadInterviewAudioRequest : ICommunicationMessage
    {
        public PostFileApiView InterviewAudio { get; set; }
    }

    public class UploadInterviewAudioAuditRequest : ICommunicationMessage
    {
        public PostFileApiView InterviewAudio { get; set; }
    }

    public class UploadDeviceInfoRequest : ICommunicationMessage
    {
        public Guid UserId { get; set; }
        public DeviceInfoApiView DeviceInfo { get; set; }
    }

    public class GetQuestionnairesWebModeRequest: ICommunicationMessage { }

    public class GetQuestionnairesWebModeResponse : ICommunicationMessage
    {
        public List<QuestionnaireIdentity> Questionnaires { get; set; } = new List<QuestionnaireIdentity>();
    }
    
    public class GetQuestionnairesSettingsRequest: ICommunicationMessage { }

    public class GetQuestionnairesSettingsResponse : ICommunicationMessage
    {
        public List<QuestionnaireSettingsApiView> QuestionnairesSettings { get; set; } = new List<QuestionnaireSettingsApiView>();
    }
}
