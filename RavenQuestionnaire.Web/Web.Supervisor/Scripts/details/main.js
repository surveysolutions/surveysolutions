requirejs.config({
    baseUrl: '../../Scripts/lib',
    paths: {
        app: '../details/app'
    },
    shim: {
        'jquery': {
            exports: "$"
        },
        "amplify": {
            deps: ['jquery'],
            exports: "amplify"
        },
        'knockout': {
            exports: "ko"
        },
        'knockout.validation': {
            deps: ['knockout']
        },
        'director': {
            exports: "Router",
            deps: ['jquery'],
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
            $(element).datepicker(options);

            //when a user changes the date, update the view model
            ko.utils.registerEventHandler(element, "changeDate", function (event) {
                var value = valueAccessor();
                if (ko.isObservable(value)) {
                    value(event.date);
                }
            });
        },
        update: function (element, valueAccessor) {
            var widget = $(element).data("datepicker");
            //when the view model is updated, update the widget
            if (widget) {
                widget.date = ko.utils.unwrapObservable(valueAccessor());
                if (widget.date) {
                    widget.setValue();
                }
            }
        }
    };

    ko.bindingHandlers.popover = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            var cssSelectorForPopoverTemplate = ko.utils.unwrapObservable(valueAccessor());
            var popOverTemplate = "<div id='my-knockout-popver'>" + $(cssSelectorForPopoverTemplate).html() + "</div>";
            $(element).popover({ content: popOverTemplate, html: true, trigger: 'manual' });

            $(element).click(function () {
                $(this).popover('toggle');
                var thePopover = document.getElementById("my-knockout-popver");
                ko.applyBindings(viewModel, thePopover);
            });
        }
    };

    ko.bindingHandlers.date = {
        update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            var value = valueAccessor(),
                allBindings = allBindingsAccessor();
            var valueUnwrapped = ko.utils.unwrapObservable(value);
            var pattern = allBindings.datePattern || 'MM/dd/yyyy';
            $(element).text(valueUnwrapped.toString(pattern));
        }
    };

    Date.prototype.mmddyyyy = function () {
        var yyyy = this.getFullYear().toString();
        var mm = (this.getMonth() + 1).toString(); // getMonth() is zero-based
        var dd = this.getDate().toString();
        return (mm[1] ? mm : "0" + mm[0]) + '/' + (dd[1] ? dd : "0" + dd[0]) + '/' + yyyy; // padding
    };
    
    ko.validation.rules['notempty'] = {
        validator: function (array) {
            return array.length > 0;
        },
        message: 'The array must contain at least one valid element.'
    };

    /*
    $.when(datacontext.parseData(input))
    .done(function () {
        $('#umbrella').attr('data-bind', 'visible:isSaving');
        viewmodel.init();
        ko.applyBindings(viewmodel);
    });
    */
    
});