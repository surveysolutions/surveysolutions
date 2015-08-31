ko.bindingHandlers.typeahead = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        //initialize with some optional options
        var options = allBindingsAccessor().typeaheadOptions || {};
        options = $.extend({}, options, {
            items: options.items || 12,
            minLength: options.minLength || 0,
            autoSelect: options.autoSelect || false,
            showHintOnFocus: options.showHintOnFocus || true,
            showLoadMore: options.showLoadMore || true
        });
        if (options.showLoadMore && options.displayText) {
            options.extendedDisplayText = options.displayText;
            delete options.displayText;
        }

        $(element).typeahead(options);
        $(element).change(function () {
            var selectedItem = $(element).typeahead("getActive");
            if (selectedItem) {
                var observable = valueAccessor();
                observable(selectedItem);
            }
        });
    },
    update: function (element, valueAccessor, allBindingsAccessor) {
        var value = ko.utils.unwrapObservable(valueAccessor());
        if (_.isUndefined(value)) {
            $(element).val("");
            return;
        }

        var options = allBindingsAccessor().typeaheadOptions || {};
        $(element).val(options.displayText(value));
    }
};