using System;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;

namespace WB.Core.BoundedContexts.Headquarters.Views.PreloadedData
{
    public class PreloadedDataVerificationErrorsView
    {
        public static PreloadedDataVerificationErrorsView CreatePrerequisitesError(
            Guid questionnaireId,
            long version,
            string questionnaireTitle,
            string error,
            PreloadedContentType preloadedContentType,
            string fileName = null)
            => new PreloadedDataVerificationErrorsView(
                questionnaireId,
                version,
                questionnaireTitle,
                new[] { new PreloadedDataVerificationError("PL0000", error) },
                false,
                null,
                preloadedContentType,
                fileName);

        public PreloadedDataVerificationErrorsView(
            Guid questionnaireId, 
            long version, 
            string questionnaireTitle,
            PreloadedDataVerificationError[] errors,
            bool wasSupervsorProvided, 
            string id, 
            PreloadedContentType preloadedContentType, 
            string fileName = null)
        {
            this.PreloadedContentType = preloadedContentType;
            this.FileName = fileName;
            this.Id = id;
            this.QuestionnaireId = questionnaireId;
            this.Version = version;
            this.Errors = errors;
            this.WasSupervsorProvided = wasSupervsorProvided;
            this.QuestionnaireTitle = questionnaireTitle;
        }

        public string Id { get; private set; }

        public Guid QuestionnaireId { get; private set; }

        public long Version { get; private set; }

        public PreloadedDataVerificationError[] Errors { get; private set; }

        public PreloadedContentType PreloadedContentType { get; private set; }
        public string FileName { get; set; }

        public bool WasSupervsorProvided { get; set; }
        public string QuestionnaireTitle { get; set; }
    }
}
