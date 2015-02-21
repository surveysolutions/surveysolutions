ko.validation.init({ insertMessages: false, decorateElement: true, errorElementClass: 'has-error' });
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

ko.validation.rules['numericValidator'] = {
    validator: function (val, countOfDecimalPlaces) {
        var newValue = val.split(',').join('');

        if (isNaN(newValue)) {
            this.message = 'Please enter a number';
            return false;
        }
        if (countOfDecimalPlaces===true)
            return true;
        var stringVal = (newValue || '').toString();
        if (stringVal.indexOf(".") == -1) {
            return true;
        }

        if (countOfDecimalPlaces <= 0) {
            this.message = 'Only integer values are allowed';
            return false;
        }

        var countOfDecimalDigits = stringVal.substring(stringVal.indexOf(".") + 1).length;
        if (countOfDecimalDigits > countOfDecimalPlaces) {
            this.message = 'According to questionnaire, count of decimal places should not be greater than ' + countOfDecimalPlaces;
            return false;

        }
        return true;
    },
    message: 'Count of decimal places should not be greater than value set in questionnaire'
};

ko.validation.rules['digit'] = {
    validator: function (value, validate) {
        return _.isEmpty(value) || (validate && /^-?\d+$/.test(value));
    },
    message: 'Please enter a digit'
};
ko.bindingHandlers.maskFormatter = {
    
    init: function (element, valueAccessor) {
      
        var value = ko.utils.unwrapObservable(valueAccessor());
        if (!value)
            return;

        $.mask.definitions = {
            '#': "[0-9]",
            '~': "[A-Za-z]",
            '*': "[A-Za-z0-9]"
        };

        $(element).mask(value);
    }
};
ko.bindingHandlers.typeahead = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        var $element = $(element);
        var allBindings = allBindingsAccessor();
        var source = ko.toJS(ko.utils.unwrapObservable(valueAccessor()));

        var states = new Bloodhound({
            datumTokenizer: Bloodhound.tokenizers.obj.whitespace('label'),
            queryTokenizer: Bloodhound.tokenizers.whitespace,
            local: source,
            limit: 10
        });

        states.initialize();

        $element
            .attr('autocomplete', 'off')
            .attr("value", allBindings.value()) // for IE, i love you!
            .typeahead({
                hint: true,
                highlight: true,
                minLength: 1
            },
            {
                name: 'states',
                displayKey: 'label',
                source: states.ttAdapter()
            }).on('typeahead:selected', function(obj, datum) {
                allBindings.id(datum.value);
                $element.change();
            })
            .val(allBindings.value());
    }
};
ko.bindingHandlers.numericformatter = {
    init: function (element, valueAccessor) {
        ko.utils.registerEventHandler(element, 'change', function () {
            var observable = valueAccessor();
            observable($(element).val());
        });
    },
    update: function (element, valueAccessor) {
        var value = ko.utils.unwrapObservable(valueAccessor());
        if (!value)
            return;

        var jElement = $(element);

        var newValue = ko.bindingHandlers.numericformatter.format(value.split(',').join(''));

        if (jElement.is('input')) {
            if (newValue !== value) {
                jElement.val(newValue);

                var oldCursorPosition = ko.bindingHandlers.numericformatter.getCursorPosition(element);
                var newPosition = ko.bindingHandlers.numericformatter.getNewCursorPosition(value, newValue, oldCursorPosition);
                
                ko.bindingHandlers.numericformatter.selectRange(element, newPosition);
            }
        }

        if ($.isFunction(jElement.text) && newValue !== value) {
            jElement.text(newValue);
        }
    },
    endWith: function (str, symbol) {
        return str.indexOf(symbol, str.length - 1) !== -1;
    },
    format: function (val, comma, period) {
        comma = comma || ',';
        period = period || '.';
        var periodLength = 3;
        var split = val.toString().split('.');
        var numeric = split[0];
        var decimal = split.length > 1 ? period + split[1] : '';
        var countOfPeriods = parseInt((numeric.length - 1) / periodLength);
        var separatedNumeric ='';
        for (var i = 0; i < countOfPeriods; i++) {
            var subValue = numeric.substr(numeric.length - ((i + 1) * periodLength), periodLength);
            separatedNumeric = comma + subValue + separatedNumeric;
        }
        return numeric.substr(0, numeric.length - countOfPeriods * periodLength) + separatedNumeric + decimal;
    },
    getNewCursorPosition: function (oldText, newText, oldCursorPosition) {
        var newCursorPosition = newText.length;
        var indexOfOldValue = 0;

        for (var i = 0; i < newText.length; i++) {
            while (newText[i] != oldText[indexOfOldValue]) {

                if (isNaN(parseInt(newText[i])))
                    break;

                indexOfOldValue++;
            }

            if (indexOfOldValue + 1 >= oldCursorPosition) {
                newCursorPosition = i + 1;
                break;
            }
        }

        return newCursorPosition;
    },
    selectRange: function (input, start, end) {
        if (!end) end = start;

        if (input.setSelectionRange) {
            input.focus();
            input.setSelectionRange(start, end);
        } else if (input.createTextRange) {
            var range = input.createTextRange();
            range.collapse(true);
            range.moveEnd('character', end);
            range.moveStart('character', start);
            range.select();
        }
    },
    getCursorPosition: function (input) {
        if (!input) return; // No (input) element found
        if ('selectionStart' in input) {
            // Standard-compliant browsers
            return input.selectionStart;
        } else if (document.selection) {
            // IE
            input.focus();
            var sel = document.selection.createRange();
            var selLen = document.selection.createRange().text.length;
            sel.moveStart('character', -input.value.length);
            return sel.text.length - selLen;
        }
    }
};
(function () {
    ko.validation.registerExtenders();
}());