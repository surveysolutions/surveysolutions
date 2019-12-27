namespace WB.UI.Headquarters.Models
{
    public class UserBatchUploadModel
    {
        //[Required(ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = nameof(FieldsAndValidations.UserBatchUploadModel_Required_ErrorMessage))]
        //[ValidateFile(ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = nameof(FieldsAndValidations.UserBatchUploadModel_Validation_ErrorMessage))]
        //[Display(ResourceType = typeof(FieldsAndValidations), Name = nameof(FieldsAndValidations.UserBatchUploadModel_File_CSV_File))]
        //public HttpPostedFileBase File { get; set; }

        public string[] AvaliableDataColumnNames { get; set; }
    }
}
