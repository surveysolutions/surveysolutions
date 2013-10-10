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