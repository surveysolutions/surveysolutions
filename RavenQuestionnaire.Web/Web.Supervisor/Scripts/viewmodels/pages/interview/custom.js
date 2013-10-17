ko.validation.init({ insertMessages: false, decorateElement: true, errorElementClass: 'error' });

ko.bindingHandlers.datepicker = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        //initialize datepicker with some optional options
        var options = allBindingsAccessor().datepickerOptions || {};
        $(element).datepicker(options).on("changeDate", function (ev) {
            var observable = valueAccessor();
            observable(ev.date);
        });
    },
    update: function (element, valueAccessor) {
        var value = ko.utils.unwrapObservable(valueAccessor());
        $(element).datepicker("setValue", value);
    }
};

ko.validation.rules['notempty'] = {
    validator: function (array) {
        return array.length > 0;
    },
    message: 'The array must contain at least one valid element.'
};

ko.validation.rules['precision'] = {
    validator: function (val, countOfDecimalPlaces) {
        var stringVal = (val || '').toString();
        if (stringVal.indexOf(".") == -1) {
            return true;
        }
        var countOfDecimalDigits = stringVal.substring(stringVal.indexOf(".") + 1).length;
        if (countOfDecimalDigits > countOfDecimalPlaces) {
            this.message = 'According to questionnaire, count of decimal places should not be greater than ' + countOfDecimalPlaces;
            return false;

        }
        return true;
    },
    message: 'Count of decimal places should not be greater than value set in questionnaire'
};

ko.validation.rules['digit'] = {
    validator: function (value, validate) {
        return _.isEmpty(value) || (validate && /^-?\d+$/.test(value));
    },
    message: 'Please enter a digit'
};

(function () {
    ko.validation.registerExtenders();
}());