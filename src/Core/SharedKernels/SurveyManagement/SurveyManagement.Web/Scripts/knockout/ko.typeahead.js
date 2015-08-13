ko.bindingHandlers.typeahead = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        //initialize with some optional options
        var options = allBindingsAccessor().typeaheadOptions || {};
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
            var options = allBindingsAccessor().typeaheadOptions || {};
            $(element).val(options.displayText(value));
        }
            
    }
};