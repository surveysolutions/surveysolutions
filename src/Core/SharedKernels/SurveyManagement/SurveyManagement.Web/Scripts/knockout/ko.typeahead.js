ko.bindingHandlers.typeahead = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        //initialize with some optional options
        var options = allBindingsAccessor().typeaheadOptions || {};
        $(element).typeahead(options);
        $(element).change(function () {
            var selectedItem = $(element).typeahead("getActive");
            if (selectedItem) {
                var observable = valueAccessor();
                observable(selectedItem);
            }
        });
        if (options.shouldOpenOnParentClick) {
            $(element).parent().click(function() {
                $(element).focus();
            });
        }
        
    },
    update: function (element, valueAccessor, allBindingsAccessor) {
        var options = allBindingsAccessor().typeaheadOptions || {};
        var value = ko.utils.unwrapObservable(valueAccessor());
        $(element).val(options.displayText(value));
    }
};

!function ($) {
    "use strict";
    var BetterTypeahead = {
        focus: function (e) {
            this.focused = true;

            if (!this.mousedover) {
                this.lookup('');
            }
        }
    };
    $.extend($.fn.typeahead.Constructor.prototype, BetterTypeahead);
}(window.jQuery);