requirejs.config({
    baseUrl: '../../Scripts/lib',
    paths: {
        app: '../assign/app'
    },
    shim: {
        "amplify": {
            deps: ['jquery'],
            exports: "amplify"
        },
        'knockout': {
            exports: "ko"
        },
        'knockout.validation': {
            deps: ['knockout']
        }
    }
});
requirejs(['jquery', 'knockout', 'amplify', 'lodash', 'app/viewmodel', 'input', 'app/datacontext', 'Math.uuid'],
function ($, ko, amplify, _, viewmodel, input, datacontext) {
    ko.validation.configure({
        insertMessages: false,
        decorateElement: true,
        errorElementClass: 'error'
    });
    
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
    
    $.when(datacontext.parseData(input))
    .done(function () {
        viewmodel.init();
        ko.applyBindings(viewmodel);
    });
    
    
});