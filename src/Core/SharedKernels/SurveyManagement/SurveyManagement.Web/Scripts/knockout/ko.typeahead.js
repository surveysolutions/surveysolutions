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
    },
    update: function (element, valueAccessor, allBindingsAccessor) {
        var options = allBindingsAccessor().typeaheadOptions || {};
        var value = ko.utils.unwrapObservable(valueAccessor());
        $(element).val(options.displayText(value));
    }
};