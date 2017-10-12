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

        $(element).prop('maxLength', 256);
        $(element).typeahead(options);


        var oldValue;
        $(element).change(function (evt) {
            var selectedItem = $(element).typeahead("getActive");
            if (selectedItem && oldValue != selectedItem) {
                var observable = valueAccessor();
                observable(selectedItem);
            }
            oldValue = selectedItem;
        });
        valueAccessor().extend({ notify: 'always' });
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