/*global ko, crossroads */



(function () {
    'use strict';

    var keyMap = {};
    var screenId = 1;
    var empty = "00000000-0000-0000-0000-000000000000";
    var answerMap = [];
    var questionMap = [];

    var Key = function (publicKey, propagatekey, isPropagated) {
        var self = this;

        self.publicKey = publicKey;
        self.propagatekey = propagatekey;
        self.isPropagated = isPropagated;
        self.toString = function () {
            return self.publicKey + self.propagatekey;
        }
        self.id = 0;

        if (keyMap[self.toString()] == undefined) {
            self.id = screenId++;
            keyMap[self.toString()] = self.id;
        } else {
            self.id = keyMap[self.toString()];
        }
    }

    var Totals = function (totals) {
        var self = this;
        self.answered = totals.Answered;
        self.enabled = totals.Enablad;
        self.total = totals.Total;
        self.counters = ko.computed(function () {
            return self.answered + '/' + self.enabled;
        });
    }
    // represent a single todo item
    var MenuItem = function (key, title, level, totals) {
        var self = this;

        self.key = key;
        self.title = title;
        self.isCurrent = ko.observable(false);
        self.level = level * 1;
        self.totals = totals;
    };

    var Screen = function (model, key, title, questions, childScreenKeys) {
        var self = this;
        var model = model;

        self.key = key;
        self.title = title;
        self.questions = questions;
        self.isVisible = ko.observable(true);
        self.childScreenKeys = childScreenKeys;
        self.hasQuestions = !(childScreenKeys.length > 0);

        self.setVisible = function (key) {
            self.isVisible(true);
            self.filterAnswers(key);

            for (var i = 0; i < self.childScreenKeys.length; i++) {
                var child = self.childScreenKeys[i];

                if (child.isPropagated) {
                    child = new Key(child.publicKey, empty, false);
                }

                var s = ko.utils.arrayFirst(model.screens(), function (screen) {
                    return screen.key.id == child.id;
                });

                if (s != null) {
                    s.setVisible();
                    s.filterAnswers(key);
                }
            }
        }

        self.filterAnswers = function (key) {
            if (!key)
                return;

            if (key.isPropagated == false) {
                return
            }

            if (self.hasQuestions) {
                ko.utils.arrayForEach(self.questions(), function (item) {
                    item.filterAnswers(key);
                });
            }
        };

        self.visibleCount = ko.computed(function () {
            var count = 0;
            ko.utils.arrayForEach(self.questions(), function (item) {
                count = Math.max(count, item.visibleCount());
            });
            return count;
        });

        self.flagedCount = ko.computed(function () {
            var count = 0;
            ko.utils.arrayForEach(self.questions(), function (item) {
                count += item.flagedCount();
            });
            return count;
        });

        self.answeredCount = ko.computed(function () {
            var count = 0;
            ko.utils.arrayForEach(self.questions(), function (item) {
                count += item.answeredCount();
            });
            return count;
        });

        self.invalidCount = ko.computed(function () {
            var count = 0;
            ko.utils.arrayForEach(self.questions(), function (item) {
                count += item.invalidCount();
            });
            return count;
        });

        self.editableCount = ko.computed(function () {
            var count = 0;
            ko.utils.arrayForEach(self.questions(), function (item) {
                count += item.editableCount();
            });
            return count;
        });

        self.enabledCount = ko.computed(function () {
            var count = 0;
            ko.utils.arrayForEach(self.questions(), function (item) {
                count += item.enabledCount();
            });
            return count;
        });
    };

    var Question = function (key, title, answers) {
        var self = this;
        self.key = key;
        self.title = title;
        self.answers = answers;
        self.isVisible = ko.observable(true);
        self.visibleCount = ko.computed(function () {
            var count = 0;
            ko.utils.arrayForEach(self.answers(), function (item) {
                if (item.isVisible() == true)
                    count++;
            });
            return count;
        });

        self.flagedCount = ko.computed(function () {
            var count = 0;
            ko.utils.arrayForEach(self.answers(), function (item) {
                if (item.isFlaged() == true)
                    count++;
            });
            return count;
        });


        self.answeredCount = ko.computed(function () {
            var count = 0;
            ko.utils.arrayForEach(self.answers(), function (item) {
                if (item.isAnswered() == true)
                    count++;
            });
            return count;
        });

        self.invalidCount = ko.computed(function () {
            var count = 0;
            ko.utils.arrayForEach(self.answers(), function (item) {
                if (item.isValid() == false)
                    count++;
            });
            return count;
        });

        self.editableCount = ko.computed(function () {
            var count = 0;
            ko.utils.arrayForEach(self.answers(), function (item) {
                if (item.isReadonly == false)
                    count++;
            });
            return count;
        });

        self.enabledCount = ko.computed(function () {
            var count = 0;
            ko.utils.arrayForEach(self.answers(), function (item) {
                if (item.isEnabled() == true)
                    count++;
            });
            return count;
        });

        self.height = ko.observable();
        self.filterAnswers = function (key) {
            if (!key)
                return;

            if (self.answers().length > 0) {
                ko.utils.arrayForEach(self.answers(), function (item) {
                    if (item.key.propagatekey != key.propagatekey)
                        item.isVisible(false);
                });
            }
        };
        questionMap.push(self);
    };

    var Answer = function (key, surveyKey, parentKey, type, question, answer, answerOptions, isReadonly, isValid, isEnabled, isAnswered, comments) {
        var self = this;
        self.key = key;
        self.surveyKey = surveyKey;
        self.question = question;
        self.answer = ko.observable(answer);
        self.answerOptions = answerOptions;

        self.selectedOption = ko.observable();
        self.selectedOptions = ko.observableArray();

        self.isVisible = ko.observable(true);
        self.isValid = ko.observable(isValid);
        self.isEnabled = ko.observable(isEnabled);
        self.isReadonly = isReadonly;
        self.isFlaged = ko.observable(false);
        self.type = type;
        self.comments = ko.observableArray(comments || []);
        self.isAnswered = ko.observable(isAnswered);
        self.currentComment = ko.observable('');

        self.addComment = function () {
            var current = self.currentComment().trim();
            if (current) {
                self.comments.push(new Comment(current));
                self.currentComment('', undefined, new Date());

                var request = dataHelper.createAddCommentRequest();
                request.data = {
                    surveyKey: self.surveyKey,
                    questionKey: key.publicKey,
                    questionPropagationKey: key.propagatekey,
                    comment: current
                };

                request = $.ajax(request);

                request.done(function (msg) {
                    console.log("comment saved");
                });

                request.fail(function (jqXHR, textStatus) {
                    console.log("comment error");
                });
            }
        };

        self.subscribe = function () {
            self.selectedOption.subscribe(function (value) {
                var newAnswer = '';
                switch (self.displayMode()) {
                    case "SingleOption":
                        var selected = ko.utils.arrayFirst(self.answerOptions, function (option) { return option.value == value; }) || undefined;
                        if (selected != undefined) {
                            newAnswer = selected.title;
                        }
                        break;
                    case "Numeric":
                        newAnswer = value;
                        break;
                    case "Text":
                        newAnswer = value;
                        break;
                    case "AutoPropagate":
                        newAnswer = value;
                        break;
                    case "DateTime":
                        //parse date
                        newAnswer = value;
                        break;
                }
                if (newAnswer.trim() != '') {
                    self.answer(newAnswer);
                    self.isAnswered(true);
                }
                else {
                    self.answer('');
                    self.isAnswered(false);
                }
            });

            self.selectedOptions.subscribe(function (values) {
                var newAnswer = '';
                ko.utils.arrayForEach(values, function (value) {
                    var selected = ko.utils.arrayFirst(self.answerOptions, function (option) { return option.value == value; }) || undefined;

                    if (selected != undefined) {
                        newAnswer += selected.title + ", ";
                    }
                });

                newAnswer = newAnswer.trim();
                newAnswer = newAnswer.substring(0, newAnswer.length - 1);

                if (newAnswer != '') {
                    self.answer(newAnswer);
                    self.isAnswered(true);
                }
                else {
                    self.answer('');
                    self.isAnswered(false);
                }
            });
        };

        var processOptions = function () {
            switch (self.displayMode()) {
                case "SingleOption":
                    var selected = ko.utils.arrayFirst(self.answerOptions, function (option) { return option.isSelected; }) || undefined;
                    if (selected != undefined) {
                        self.selectedOption(selected.value);
                    }
                    break;
                case "MultyOption":
                    var selected = ko.utils.arrayFilter(self.answerOptions, function (option) { return option.isSelected; }) || undefined;
                    if (selected != undefined) {
                        self.selectedOptions(ko.utils.arrayMap(selected, function (option) {
                            return option.value;
                        }));
                    }
                    break;
                case "Numeric":
                    self.selectedOption(answer);
                    break;
                case "Text":
                    self.selectedOption(answer);
                    break;
                case "AutoPropagate":
                    self.selectedOption(answer);
                    break;
                case "DateTime":
                    //parse date
                    self.selectedOption(new Date(answer));
                    break;
            }
        };

        self.displayMode = function () {
            var tmpl = "";
            switch (self.type) {
                case 0: tmpl = "SingleOption"; break;
                case 1: tmpl = "SingleOption"; break;
                case 2: tmpl = "SingleOption"; break;
                case 3: tmpl = "MultyOption"; break;
                case 4: tmpl = "Numeric"; break;
                case 5: tmpl = "DateTime"; break;
                case 6: tmpl = "Text"; break; //GpsCoordinates
                case 7: tmpl = "Text"; break;
                case 8: tmpl = "AutoPropagate"; break;
            }
            return tmpl;
        };

        processOptions();
        answerMap.push(self);
    };

    var AnswerOption = function (key, value, title, isSelected) {
        var self = this;
        self.key = key;
        self.value = value;
        self.title = title;
        self.isSelected = isSelected;
    }

    var Comment = function (text, user, date) {
        var self = this;
        self.text = text;
        self.user = user;
        self.date = date;
    }

    var User = function (key, name) {
        var self = this;
        self.key = key;
        self.name = name;
    }

    // our main view model
    var SurveyModel = function (questionnaire) {
        var self = this;

        self.showMode = ko.observable('all');
        self.isMenuHidden = ko.observable(false);

        self.title = questionnaire.Title;

        self.user = new User(questionnaire.User.Id, questionnaire.User.Name);

        // map array of passed in todos to an observableArray of Todo objects
        self.menu = ko.observableArray(ko.utils.arrayMap(questionnaire.Navigation.Menu, function (item) {
            return new MenuItem(new Key(item.Key.PublicKey, item.Key.PropagationKey, item.Key.IsPropagated),
                                item.GroupText,
                                item.Level,
                                new Totals(item.Totals));
        }));

        self.screens = ko.observableArray(ko.utils.arrayMap(questionnaire.Screens, function (screen) {

            var questions = ko.observableArray(ko.utils.arrayMap(screen.Questions, function (question) {

                var answers = ko.observableArray(ko.utils.arrayMap(question.Answers, function (answer) {

                    var comments = (answer.Comments == null || answer.Comments.trim() == '') ? [] : [new Comment(answer.Comments)];

                    var options = ko.utils.arrayMap(answer.AnswerOptions, function (option) {
                        return new AnswerOption(option.PublicKey, option.AnswerValue, option.Title, option.Selected);
                    });

                    return new Answer(
                        new Key(answer.Key.PublicKey, answer.Key.PropagationKey, answer.Key.IsPropagated),
                        questionnaire.PublicKey,
                        new Key(answer.ParentKey.PublicKey, answer.ParentKey.PropagationKey, answer.ParentKey.IsPropagated),
                        answer.Type,
                        question.Title,
                        answer.Answer,
                        options,
                        answer.IsReadOnly,
                        answer.Valid,
                        answer.Enabled,
                        answer.Answered,
                        comments);
                }));

                return new Question(question.PublicKey, question.Title, answers);
            }));

            var childScreenKeys = ko.utils.arrayMap(screen.ChildrenKeys, function (child) {
                return new Key(child.PublicKey, child.PropagationKey, child.IsPropagated);
            });

            return new Screen(self, new Key(screen.Key.PublicKey, screen.Key.PropagationKey, screen.Key.IsPropagated), screen.Title, questions, childScreenKeys);
        }));

        self.currentAnswer = ko.observable();
        self.currentScreenKey = ko.observable();

        self.filteredScreen = ko.computed(function () {

            ko.utils.arrayForEach(self.screens(), function (item) {
                item.isVisible(false);
            });

            ko.utils.arrayForEach(answerMap, function (item) {
                item.isVisible(true);
            });

            ko.utils.arrayForEach(questionMap, function (item) {
                item.isVisible(true);
            });

            if (self.showMode() != 'group') {
                ko.utils.arrayForEach(self.menu(), function (item) {
                    item.isCurrent(false);
                });

                //self.currentScreenKey(undefined);
            }

            switch (self.showMode()) {
                case 'group':
                    if (!self.currentScreenKey()) {
                        return self.screens();
                    }

                    var id = self.currentScreenKey().id;
                    if (self.currentScreenKey().isPropagated) {
                        id = (new Key(self.currentScreenKey().publicKey, empty, false)).id;
                    }

                    var screen = ko.utils.arrayFirst(self.screens(), function (screen) {
                        return screen.key.id == id;
                    });

                    screen.setVisible(self.currentScreenKey());

                    return ko.utils.arrayFilter(self.screens(), function (screen) {
                        return screen.isVisible() == true;
                    });

                case 'flaged':
                    ko.utils.arrayForEach(answerMap, function (item) {
                        item.isVisible(item.isFlaged() ? true : false);
                    });

                    ko.utils.arrayForEach(questionMap, function (item) {
                        item.isVisible(item.flagedCount() > 0);
                    });

                    return self.screens().filter(function (screen) {
                        return screen.flagedCount() > 0;
                    });
                case 'answered':
                    ko.utils.arrayForEach(answerMap, function (item) {
                        item.isVisible(item.isAnswered() ? true : false);
                    });

                    ko.utils.arrayForEach(questionMap, function (item) {
                        item.isVisible(item.answeredCount() > 0);
                    });

                    return self.screens().filter(function (screen) {
                        return screen.answeredCount() > 0;
                    });
                case 'invalid':
                    ko.utils.arrayForEach(answerMap, function (item) {
                        item.isVisible(item.isValid() ? true : false);
                    });

                    ko.utils.arrayForEach(questionMap, function (item) {
                        item.isVisible(item.invalidCount() > 0);
                    });

                    return self.screens().filter(function (screen) {
                        return screen.invalidCount() > 0;
                    });
                case 'supervisor':
                    ko.utils.arrayForEach(answerMap, function (item) {
                        item.isVisible(!item.isReadonly ? true : false);
                    });

                    ko.utils.arrayForEach(questionMap, function (item) {
                        item.isVisible(item.editableCount() > 0);
                    });

                    return self.screens().filter(function (screen) {
                        return screen.editableCount() > 0;
                    });
                case 'enabled':
                    ko.utils.arrayForEach(answerMap, function (item) {
                        item.isVisible(item.isEnabled ? true : false);
                    });

                    ko.utils.arrayForEach(questionMap, function (item) {
                        item.isVisible(item.enabledCount() > 0);
                    });

                    return self.screens().filter(function (screen) {
                        return screen.enabledCount() > 0;
                    });
                default:
                    return self.screens().filter(function (screen) {
                        return true;
                    });
            }
        });

        self.selectMenuItem = function (menuItem) {
            ko.utils.arrayForEach(this.menu(), function (item) {
                item.isCurrent(false);
            });

            self.currentScreenKey(menuItem.key);

            menuItem.isCurrent(true);

            self.showMode('group');
        } .bind(self);

        self.flagAnswer = function (answer, event) {
            answer.isFlaged(!answer.isFlaged());
        } .bind(self);

        self.flagedCount = ko.computed(function () {
            var count = 0;
            ko.utils.arrayForEach(self.screens(), function (item) {
                count += item.flagedCount();
            });
            return '(' + count + ')';
        });

        self.answeredCount = ko.computed(function () {
            var count = 0;
            ko.utils.arrayForEach(self.screens(), function (item) {
                count += item.answeredCount();
            });
            return '(' + count + ')';
        });

        self.invalidCount = ko.computed(function () {
            var count = 0;
            ko.utils.arrayForEach(self.screens(), function (item) {
                count += item.invalidCount();
            });
            return '(' + count + ')';
        });

        self.editableCount = ko.computed(function () {
            var count = 0;
            ko.utils.arrayForEach(self.screens(), function (item) {
                count += item.editableCount();
            });
            return '(' + count + ')';
        });

        self.enabledCount = ko.computed(function () {
            var count = 0;
            ko.utils.arrayForEach(self.screens(), function (item) {
                count += item.enabledCount();
            });
            return '(' + count + ')';
        });

        self.openDetails = function (answer, event) {
            self.currentAnswer(answer);
            $('#stacks').addClass('detail-visible');
            event.stopPropagation();

            $('#details .body').css('top', ($('#details .title').outerHeight() + 'px'));
        };

        self.closeDetails = function (answer, event) {
            $('#stacks').removeClass('detail-visible');
            self.currentAnswer(undefined);
            event.stopPropagation();
        };

        self.closeMenu = function () {
            $('#stacks').addClass('menu-hidden');
            self.isMenuHidden(true);
        };

        self.openMenu = function () {
            $('#stacks').removeClass('menu-hidden');
            self.isMenuHidden(false);
        };

        var createRequest = function () {
            var request = $.ajax({
                // url: href,
                type: "POST",
                data: {},
                dataType: "html"
            });

            request.done(function (msg) {
            });

            request.fail(function (jqXHR, textStatus) {
            });

            return request;
        }
    };

    $(document).ready(function () {
        // bind a new instance of our view model to the page
        var viewModel = new SurveyModel(questionnaire || {});
        ko.applyBindings(viewModel);

        $('#groups .body').css('top', ($('#groups .title').outerHeight() + 'px'));

        ko.utils.arrayForEach(answerMap, function (item) {
            item.subscribe();
        });

        // set up filter routing
        Router({ '/:filter': viewModel.showMode }).init();
    });

} ());