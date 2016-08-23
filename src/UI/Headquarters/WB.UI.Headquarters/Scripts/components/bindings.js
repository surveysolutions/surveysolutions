(function(ko) {
    ko.bindingHandlers.enterKey = {
        init: function (element, valueAccessor, allBindings, data, context) {
            var wrapper = function (wrappedData, event) {
                if (event.keyCode === 13 || event.which === 13) {
                    valueAccessor().call(this, wrappedData, event);
                }
            };
            ko.applyBindingsToNode(element, { event: { keyup: wrapper } }, context);
        }
    };

    ko.bindingHandlers.sortby = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            var listView = bindingContext.$root;

            var value = valueAccessor();

            var sort = "sorting";
            var sortAsc = "sorting-up";
            var sortDesc = "sorting-down";

            var refreshUI = function () {

                $(element).addClass(sort);

                $(element).removeClass(sortAsc);
                $(element).removeClass(sortDesc);

                if (listView.SortOrder() == value) {
                    if (listView.SortDirection()) {
                        $(element).addClass(sortAsc);
                    } else {
                        $(element).addClass(sortDesc);
                    }
                }
            };

            listView.SortOrder.subscribe(refreshUI);
            listView.SortDirection.subscribe(refreshUI);

            $(element).click(function () {
                listView.sort(value);
            });

            refreshUI();
        }
    };

    ko.bindingHandlers.date = {
        update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            var value = valueAccessor();
            var allBindings = allBindingsAccessor();
            var valueUnwrapped = ko.utils.unwrapObservable(value);

            var pattern = allBindings.format || 'MM/DD/YYYY';

            var output = "";
            if (valueUnwrapped !== null && valueUnwrapped !== undefined && valueUnwrapped.length > 0) {
                output = moment(valueUnwrapped).format(pattern);
            }

            if ($(element).is("input") === true) {
                $(element).val(output);
            } else {
                $(element).text(output);
            }
        }
    };
})(ko);