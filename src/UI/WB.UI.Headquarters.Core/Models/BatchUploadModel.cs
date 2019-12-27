﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Models
{
    public class BatchUploadModel
    {
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }

        public string QuestionnaireTitle { get; set; }

        //[Required(ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = nameof(FieldsAndValidations.BatchUploadModel_Required))]
        //[ValidateFile(ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = nameof(FieldsAndValidations.BatchUploadModel_ValidationErrorMessage))]
        //[Display(ResourceType = typeof(FieldsAndValidations), Name = nameof(FieldsAndValidations.BatchUploadModel_FileName))]
        //public HttpPostedFileBase File { get; set; }
        public List<FeaturedQuestionItem> FeaturedQuestions { get; set; }
        public int ClientTimezoneOffset { get; set; }
        public IList<string> HiddenQuestions { get; set; }
        public IList<string> RosterSizeQuestions { get; set; }
        [Required(ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = nameof(FieldsAndValidations.MandatoryField))]
        public Guid ResponsibleId { get; set; }
    }
}
