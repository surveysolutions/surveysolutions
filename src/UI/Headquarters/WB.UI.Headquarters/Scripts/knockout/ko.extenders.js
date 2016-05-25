ko.extenders.required = function (target, settings) {

    settings = settings || {};

    target.hasError = ko.observable();
    target.validationMessage = ko.observable();

    target.isValid = function() {
        validate(target());
        return !target.hasError();
    };

    function validate(newValue) {
        target.hasError(newValue ? false : true);
        target.validationMessage(newValue ? "" : settings.message || "This field is required");
    }

    if (settings.shouldValidateOnStart)
        target.isValid();

    target.subscribe(validate);

    return target;
};