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
        var comma = ',';
        var period = '.';

        var newValue = val.split(comma).join('');

        if (isNaN(newValue)) {
            this.message = 'Please enter a number';
            return false;
        }
        if (countOfDecimalPlaces===true)
            return true;
        var stringVal = (newValue || '').toString();
        if (stringVal.indexOf(period) == -1) {
            return true;
        }

        if (countOfDecimalPlaces <= 0) {
            this.message = 'Only integer values are allowed';
            return false;
        }

        var countOfDecimalDigits = stringVal.substring(stringVal.indexOf(period) + 1).length;
        if (countOfDecimalDigits > countOfDecimalPlaces) {
            this.message = 'According to questionnaire, count of decimal places should not be greater than ' + countOfDecimalPlaces;
            return false;

        }
        return true;
    },
    message: 'Count of decimal places should not be greater than value set in questionnaire'
};

ko.validation.rules['numberLengthValidator'] = {
    validator: function (val, numberType) {
        var comma = ',';
        var period = '.';

        var isIntegerType = numberType == 'integer';
        var isRealType = numberType == 'real';
        if (!isIntegerType && !isRealType)
            return true;

        var value = val.split(comma).join('');

        var isNumeric = $.isNumeric(value);
        var isIntegerNumeric = Math.floor(value) == value && isNumeric;

        if (isIntegerType && !isIntegerNumeric) {
            this.message = 'Please enter a integer value.'
            return false;
        }
        else if (isRealType && !isNumeric) {
            this.message = 'Please enter a real value.'
            return false;
        }

        if (isIntegerType) {
            var integerNumber = parseInt(value);
            var isInteger = integerNumber <= 2147483647 && integerNumber >= -2147483648;
            if (!isInteger) {
                this.message = 'Please enter a value between -2147483648 and 2147483647.'
                return false;
            }
        } else if (isRealType) {
            var decimalCharPosition = value.indexOf(period);
            if (decimalCharPosition != -1) {
                var countOfDecimalDigits = value.substring(decimalCharPosition + 1).length;
                if (countOfDecimalDigits > 15) {
                    this.message = 'Count of decimal places should not be greater than 15.'
                    return false;
                }
            }

            var countOfDecimalSymbols = decimalCharPosition == -1 ? value.length : value.length - 1;
            var isReal = countOfDecimalSymbols <= 28;
            if (!isReal) {
                this.message = 'Count of digits should not be greater than 28.'
                return false;
            }
        }

        return true;
    },
    message: 'Please enter a number value.'
};

ko.validation.rules['digit'] = {
    validator: function (value, validate) {
        return _.isEmpty(value) || (validate && /^-?\d+$/.test(value));
    },
    message: 'Please enter a digit'
};

ko.validation.rules['gps_latitude'] = {
    validator: function (value, validate) {
        if (!validate)
            return true;
        if (_.isEmpty(value))
            return true;

        if (isNaN(value)) {
            return false;
        }
        var latitude = parseFloat(value);
        return latitude > -90 && latitude < 90;
    },
    message: 'Please enter valid latitude'
};
ko.validation.rules['gps_longitude'] = {
    validator: function (value, validate) {
        if (!validate)
            return true;

        if (_.isEmpty(value))
            return true;

        if (isNaN(value)) {
            return false;
        }

        var longitude = parseFloat(value);
        return longitude > -180 && longitude < 180;
    },
    message: 'Please enter valid longitude'
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
ko.bindingHandlers.simpletypeahead = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        var $element = $(element);
        var allBindings = allBindingsAccessor();
        var source = ko.toJS(ko.utils.unwrapObservable(valueAccessor()));

        var states = new Bloodhound({
            datumTokenizer: function (item) {
                var tokens = Bloodhound.tokenizers.whitespace(item.label);
                $.each(tokens, function (k, v) {
                    var i = 0;
                    while ((i + 1) < v.length) {
                        tokens.push(v.substr(i, v.length));
                        i++;
                    }
                });
                return tokens;
            },
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
(function () {
    ko.validation.registerExtenders();
}());