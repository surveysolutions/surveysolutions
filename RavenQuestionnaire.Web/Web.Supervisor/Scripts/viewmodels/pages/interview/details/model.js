﻿Model = function () {
    var QuestionModel = function () {
        var self = this;
        self.uiId = ko.observable();
        self.id = ko.observable();
        self.variable = ko.observable();
        self.isCapital = ko.observable();
        self.comments = ko.observableArray();
        self.isReadonly = ko.observable(true);
        self.isEnabled = ko.observable();
        self.isFeatured = ko.observable();
        self.isFlagged = ko.observable();
        self.isMandatory = ko.observable();
        self.rosterVector = ko.observable();
        self.questionType = ko.observable();
        self.title = ko.observable();
        self.isInvalid = ko.observable(true);
        self.isVisible = ko.observable(true);
        self.isSelected = ko.observable(false);
        self.isAnswered = ko.observable(false);
        self.validationMessage = ko.observable('');
        self.validationExpression = ko.observable('');
        self.scope = ko.observable();
        self.markerStyle = ko.computed(function () {
            if (self.isInvalid()) {
                return "invalid";
            }
            if (self.scope() == "Supervisor") {
                return "supervisor";
            }
            return "";
        });
        self.matchFilter = function (filter) {
            switch (filter) {
                case "all":
                    self.isVisible(true);
                    break;
                case "flaged":
                    self.isVisible(self.isFlagged());
                    break;
                case "commented":
                    self.isVisible(self.comments().length > 0);
                    break;
                case "answered":
                    self.isVisible(self.isAnswered());
                    break;
                case "invalid":
                    self.isVisible(self.isInvalid());
                    break;
                case "supervisor":
                    self.isVisible(self.scope() == "Supervisor");
                    break;
                case "enabled":
                    self.isVisible(self.isEnabled());
                    break;
            }
        };


        return self;
    };
    var Option = function (questionId) {
        var self = this;
        self.label = ko.observable();
        self.value = ko.observable();
        self.isSelected = ko.observable(false);
        self.optionFor = ko.computed(function () {
            return 'option-' + questionId + '-' + self.value();
        });
        self.orderNo = ko.observable(undefined);
        return self;
    };

    var model = {
        Option: Option,
        GpsQuestion: function () {
            var self = this;
            ko.utils.extend(self, new QuestionModel());
            self.latitude = ko.observable();
            self.longitude = ko.observable();
            self.accuracy = ko.observable();
            self.altitude = ko.observable();
            self.timestamp = ko.observable();
            return self;
        },
        TextQuestion: function () {
            var self = this;
            ko.utils.extend(self, new QuestionModel());
            self.answer = ko.observable().extend({ required: true });
            self.errors = ko.validation.group(self);
            self.isValid = ko.observable(true);
        },
        QRBarcodeQuestion: function () {
            var self = this;
            ko.utils.extend(self, new QuestionModel());
            self.answer = ko.observable();
            self.errors = ko.validation.group(self);
            self.isValid = ko.observable(true);
        },
        TextListQuestion: function () {
            var self = this;
            ko.utils.extend(self, new QuestionModel());

            self.options = ko.observableArray();
            self.errors = ko.validation.group(self);
            self.isValid = ko.observable(true);
        },
        NumericQuestion: function (isInteger, countOfDecimalPlaces) {
            var self = this;
            ko.utils.extend(self, new QuestionModel());
            self.isInteger = ko.observable(isInteger);
            self.countOfDecimalPlaces = ko.observable(countOfDecimalPlaces);
            self.answer = ko.observable().extend({ required: true, number: true });

            if (isInteger) {
                self.answer.extend({ digit: true });
            }
            else if (!_.isNull(self.countOfDecimalPlaces())) {
                self.answer.extend({ precision: self.countOfDecimalPlaces() });
            }

            self.errors = ko.validation.group(self);
            return self;
        },
        DateTimeQuestion: function () {
            var self = this;
            ko.utils.extend(self, new QuestionModel());
            self.answer = ko.observable();
            return self;
        },
        SingleOptionQuestion: function () {
            var self = this;
            ko.utils.extend(self, new QuestionModel());
            self.selectedOption = ko.observable().extend({
                validation: [{
                    validator: function (val) {
                        if (_.isNull(val) || _.isUndefined(val) || _.isEmpty(val))
                            return false;
                        return true;
                    },
                    message: 'At least one option should be checked'
                }]
            });
            self.options = ko.observableArray();
            self.answer = ko.computed(function () {
                var o = _.find(ko.toJS(self.options), function (option) {
                    return (self.selectedOption() + "") == (option.value + "");
                });
                return _.isEmpty(o) ? "" : o.label;
            });
            self.errors = ko.validation.group(self);
            return self;
        },
        MultyOptionQuestion: function(areAnswersOrdered, maxAllowedAnswers) {
            var self = this;
            ko.utils.extend(self, new QuestionModel());
            self.areAnswersOrdered = ko.observable(areAnswersOrdered);
            self.maxAllowedAnswers = ko.observable(maxAllowedAnswers);
            self.orderedOptionsSelection = ko.observableArray([]);
            self.selectedOptionsCount = 0;
            self.selectedOptions = ko.observableArray([]).extend({
                validation: [
                    {
                        validator: function(val) {
                            if (_.isNull(val) || _.isUndefined(val) || _.isEmpty(val))
                                return false;
                            return val.length > 0;
                        },
                        message: 'At least one option should be checked'
                    },
                    {
                        validator: function(val) {
                            if (_.isUndefined(self.maxAllowedAnswers()) || _.isNull(self.maxAllowedAnswers())) {
                                return true;
                            }

                            return val.length <= self.maxAllowedAnswers();
                        },
                        message: 'Number of selected answers more than number of maximum permitted answers'
                    }]
            });
            self.orderSelectedOptions = function () {
                if (self.selectedOptionsCount != self.selectedOptions().length) {
                    if (self.selectedOptionsCount > self.selectedOptions().length) {
                        _.each(self.orderedOptionsSelection(), function (answer) {
                            if (!_.contains(self.selectedOptions(), answer)) {
                                self.orderedOptionsSelection.remove(answer);
                            }
                        });
                    }
                    _.each(self.options(), function (option) {
                        var orderIndex = self.orderedOptionsSelection().indexOf(option.value());
                        if (_.contains(self.selectedOptions(), option.value())) {
                            if (_.isUndefined(option.orderNo())) {
                                option.orderNo(self.selectedOptions().length);
                                self.orderedOptionsSelection.push(option.value());
                            } else {
                                if (orderIndex > -1) {
                                    option.orderNo(orderIndex + 1);
                                }
                            }
                        } else {
                            if (self.selectedOptionsCount > self.selectedOptions().length) {
                                if (orderIndex == -1) {
                                    option.orderNo(undefined);
                                }
                            }
                        }
                    });
                    self.selectedOptionsCount = self.selectedOptions().length;
                }
            };
            if (self.areAnswersOrdered()) {
                self.selectedOptions.subscribe(function() {
                    self.orderSelectedOptions();
                });
            }
            
            self.options = ko.observableArray();
            self.answer = ko.computed(function () {
                var selected = _.filter(ko.toJS(self.options), function (option) {
                    return _.contains(self.selectedOptions(), option.value + "");
                });
                self.selectedOptions(_.map(selected, 'value'));
                var a = _.reduce(selected, function (result, option) {
                    return result + option.label + ", ";
                }, "").trim();
                if (_.isEmpty(a) == false) {
                    a = a.substring(0, a.length - 1);
                }
                return a;
            });
            self.errors = ko.validation.group(self);
            return self;
        },
        Group: function () {
            var self = this;

            self.uiId = ko.observable();
            self.id = ko.observable();
            self.depth = ko.observable();
            self.isSelected = ko.observable(false);

            self.css = ko.computed(function () {
                return "level" + self.depth() + (self.isSelected() ? " selected" : "");
            });
            self.href = ko.computed(function () {
                return "#group/" + self.uiId();
            });
            self.title = ko.observable();
            self.parentId = ko.observable();
            self.rosterVector = ko.observable();
            self.questions = ko.observableArray();
            self.isVisible = ko.observable(true);

            self.visibleQuestionsCount = ko.computed(function () {
                return _.reduce(self.questions(), function (count, question) {
                    return count + (question.isVisible() ? 1 : 0);
                }, 0);
            });
            return self;
        },
        Interview: function () {
            var self = this;
            self.id = ko.observable();
            self.title = ko.observable();
            self.questionnaireId = ko.observable();
            self.status = ko.observable();
            self.responsible = ko.observable();
            return self;
        },
        User: function () {
            var self = this;
            self.id = ko.observable();
            self.name = ko.observable();
            return self;
        },

        Comment: function () {
            var self = this;
            self.id = ko.observable();
            self.text = ko.observable();
            self.date = ko.observable();
            self.userName = ko.observable();
            self.userId = ko.observable();

            var update = function () {
                self.date.valueHasMutated();
            };

            setInterval(update, 10000);

            return self;
        }
    };
    return model;
};