ko.bindingHandlers.datepicker = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        var options = allBindingsAccessor().datepickerOptions || {},
            $el = $(element);

        $el.datepicker(options);

        var previousValue = $el.datepicker("getDate");

        ko.utils.registerEventHandler(element, "hide", function (e) {
            var observable = valueAccessor();
            // hack to prevent toggling selected day error https://github.com/eternicode/bootstrap-datepicker/issues/775
            if (e.date === undefined) {
                observable(previousValue instanceof Date && isFinite(previousValue) ? previousValue : null);
            } else {
                var date = $el.datepicker("getDate");
                previousValue = date;
            }
        });
        
        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
            $el.datepicker("destroy");
        });
    },
    update: function (element, valueAccessor) {
        var value = ko.utils.unwrapObservable(valueAccessor()),
            $el = $(element);

        if (String(value).indexOf('/Date(') == 0) {
            value = new Date(parseInt(value.replace(/\/Date\((.*?)\)\//gi, "$1")));
        }

        var current = $el.datepicker("getDate");

        if (value - current !== 0) {
            $el.datepicker("setDate", value);
        }
    }
};