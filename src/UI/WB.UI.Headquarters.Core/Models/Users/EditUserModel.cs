using System;
using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.DataAnnotations;

namespace WB.UI.Headquarters.Models.Users
{
    public class EditUserModel
    {
        public const string PersonNameRegex = @"^[\p{L} '.-]+$";
        public const int PersonNameMaxLength = 100;
        public const int PhoneNumberLength = 15;

        public Guid UserId { get; set; }

        [EmailAddress(ErrorMessageResourceName = nameof(FieldsAndValidations.EmailErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessage = null)]
        public string Email { get; set; }

        [StringLength(PersonNameMaxLength, ErrorMessageResourceName = nameof(FieldsAndValidations.PersonNameErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessage = null)]
        [ServerSideOnlyRegularExpression(PersonNameRegex, ErrorMessageResourceName = nameof(FieldsAndValidations.PersonNameAllowedCharactersErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        public string PersonName { get; set; }

        [Phone(ErrorMessageResourceName = nameof(FieldsAndValidations.PhoneErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessage = null)]
        [StringLength(PhoneNumberLength, ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = nameof(FieldsAndValidations.PhoneErrorLength))]
        public string PhoneNumber { get; set; }

        public bool IsLockedByHeadquarters { get; set; }
        public bool IsLockedBySupervisor { get; set; }
        public bool IsAllowRelink { get; set; }
    }
}
