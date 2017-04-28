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

    ko.bindingHandlers.flatpickr = {
        init: function (element, valueAccessor, allBindingsAccessor) {
            var options = $.extend({
                dateFormat: 'm/d/Y H:i',
                enableTime: true,
                time_24hr: true,
                minuteIncrement: 1
            }, allBindingsAccessor().flatpickrOptions);
            var $el = $(element);
            var picker;

            if (options.wrap) {
                picker = new Flatpickr(element.parentNode, options);
            } else {
                picker = new Flatpickr(element, options);
            }

            // Save instance for update method.
            $el.data('datetimepickr_inst', picker);

            // handle the field changing by registering datepicker's changeDate event
            ko.utils.registerEventHandler(element, "change", function () {
                valueAccessor()(picker.parseDate($el.val()));
            });

            // handle disposal (if KO removes by the template binding)
            ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
                $el.flatpickr("destroy");
            });
        },
        update: function (element, valueAccessor, allBindingsAccessor) {
            // Get datepickr instance.
            var picker = $(element).data('datetimepickr_inst');

            picker.setDate(ko.unwrap(valueAccessor()));
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

    ko.bindingHandlers.selectPicker = {
        after: ['options'],   /* KO 3.0 feature to ensure binding execution order */
        init: function (element, valueAccessor, allBindingsAccessor) {
            var $element = $(element);
            $element.addClass('selectpicker').selectpicker();

            var doRefresh = function () {
                $element.selectpicker('refresh');
            }, subscriptions = [];

            // KO 3 requires subscriptions instead of relying on this binding's update
            // function firing when any other binding on the element is updated.

            // Add them to a subscription array so we can remove them when KO
            // tears down the element.  Otherwise you will have a resource leak.
            var addSubscription = function (bindingKey) {
                var targetObs = allBindingsAccessor.get(bindingKey);

                if (targetObs && ko.isObservable(targetObs)) {
                    subscriptions.push(targetObs.subscribe(doRefresh));
                }
            };

            addSubscription('options');
            addSubscription('value');           // Single
            addSubscription('selectedOptions'); // Multiple

            ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
                while (subscriptions.length) {
                    subscriptions.pop().dispose();
                }
            });
        },
        update: function (element, valueAccessor, allBindingsAccessor) {
        }
    };
})(ko);