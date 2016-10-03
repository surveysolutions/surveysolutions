ko.bindingHandlers.numericformatter = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContent) {
        var allBindings = allBindingsAccessor();
        $(element).data('useFormatting', allBindings.useFormatting || false);

        ko.utils.registerEventHandler(element, 'keyup', function () {
            var observable = valueAccessor();
            observable($(element).val());
        });
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContent) {
        var value = ko.utils.unwrapObservable(valueAccessor());
        if (!value)
            return;

        var jElement = $(element);

        var newValue = ko.bindingHandlers.numericformatter.format(value);

        if (jElement.data('useFormatting') && jElement.is('input')) {
            if (newValue !== value) {
                var oldCursorPosition = ko.bindingHandlers.numericformatter.getCursorPosition(element);
                var newPosition = ko.bindingHandlers.numericformatter.getNewCursorPosition(value, newValue, oldCursorPosition);
                
                jElement.val(newValue);
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
        var negativeSign = '-';
        var periodLength = 3;
        var split = val.toString().split(comma).join('').split('.');
        var isNegative = _.startsWith(split[0], negativeSign);
        var numeric = isNegative ? split[0].substr(1) : split[0];
        var numericSign = isNegative ? negativeSign : '';
        var decimal = split.length > 1 ? period + split[1] : '';
        var countOfPeriods = parseInt((numeric.length - 1) / periodLength);
        var separatedNumeric ='';
        for (var i = 0; i < countOfPeriods; i++) {
            var subValue = numeric.substr(numeric.length - ((i + 1) * periodLength), periodLength);
            separatedNumeric = comma + subValue + separatedNumeric;
        }
        return numericSign + numeric.substr(0, numeric.length - countOfPeriods * periodLength) + separatedNumeric + decimal;
    },
    getNewCursorPosition: function (oldText, newText, oldCursorPosition) {
        var newCursorPosition = 0;
        var indexOfOldValue = 0;

        for (var i = 0; i < newText.length; i++) {
            while (newText[i] != oldText[indexOfOldValue] && indexOfOldValue < oldText.length) {

                if (isNaN(parseInt(newText[i])))
                    break;

                indexOfOldValue++;
            }

            if (newText[i] == oldText[indexOfOldValue])
                indexOfOldValue++;

            if (indexOfOldValue >= oldCursorPosition) {
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
