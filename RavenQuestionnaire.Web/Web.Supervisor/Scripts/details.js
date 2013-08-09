/*global ko, crossroads */
var viewModel = null;
ko.bindingHandlers.datepicker = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        //initialize datepicker with some optional options
        var options = allBindingsAccessor().datepickerOptions || {};
        $(element).datepicker(options);

        //when a user changes the date, update the view model
        ko.utils.registerEventHandler(element, "changeDate", function (event) {
            var value = valueAccessor();
            if (ko.isObservable(value)) {
                value(event.date);
            }
        });
    },
    update: function (element, valueAccessor) {
        var widget = $(element).data("datepicker");
        //when the view model is updated, update the widget
        if (widget) {
            widget.date = ko.utils.unwrapObservable(valueAccessor());
            if (widget.date) {
                widget.setValue();
            }
        }
    }
};

ko.bindingHandlers.popover = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            var cssSelectorForPopoverTemplate = ko.utils.unwrapObservable(valueAccessor());
            var popOverTemplate = "<div id='my-knockout-popver'>" + $(cssSelectorForPopoverTemplate).html() + "</div>";
            $(element).popover({ content: popOverTemplate, html:true, trigger: 'manual' });

            $(element).click(function() {
                $(this).popover('toggle');
                var thePopover = document.getElementById("my-knockout-popver");
                ko.applyBindings(viewModel, thePopover);
            });  
        }
};

ko.bindingHandlers.date = {
    update: function(element, valueAccessor, allBindingsAccessor, viewModel) {
        var value = valueAccessor(),
            allBindings = allBindingsAccessor();
        var valueUnwrapped = ko.utils.unwrapObservable(value);
        var pattern = allBindings.datePattern || 'MM/dd/yyyy';
        $(element).text(valueUnwrapped.toString(pattern));
    }
};

Date.prototype.mmddyyyy = function () {
    var yyyy = this.getFullYear().toString();
    var mm = (this.getMonth() + 1).toString(); // getMonth() is zero-based
    var dd = this.getDate().toString();
    return  (mm[1] ? mm : "0" + mm[0]) + '/' + (dd[1] ? dd : "0" +  dd[0]) + '/' + yyyy; // padding
};

(function () {
    'use strict';

    var keyMap = {};
    var screenId = 1;
    var empty = "00000000-0000-0000-0000-000000000000";
    var answerMap = [];
    var questionMap = [];
    var currentUser = undefined;

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
        self.getHref = function() {
            return '#/group/' + self.key.publicKey + '/' + self.key.propagatekey;
        };
    };

    var ScreenCaption = function(key, title) {
        var self = this;
        self.key = key;
        self.title = title;
        self.isVisible = ko.observable(true);
    };
    
    var Screen = function (model, key, title, questions, childScreenKeys, captions) {
        var self = this;
        var model = model;

        self.key = key;
        self.title = title;
        self.questions = questions;
        self.isVisible = ko.observable(true);
        self.childScreenKeys = childScreenKeys;
        self.hasQuestions = (self.questions().length > 0);
        self.captions = captions;

        self.setVisible = function(key) {
            self.isVisible(true);
            ko.utils.arrayForEach(self.captions, function(item) {
                    item.isVisible(true);
                });
            if (!!key) {
                ko.utils.arrayForEach(self.captions, function(item) {
                    item.isVisible(key.propagatekey == item.key.propagatekey);
                });
            }

            for (var i = 0; i < self.childScreenKeys.length; i++) {
                var child = self.childScreenKeys[i];

                if (child.isPropagated) {
                    child = new Key(child.publicKey, empty, false);
                }

                var s = ko.utils.arrayFirst(model.screens(), function(screen) {
                    return screen.key.id == child.id;
                });

                if (s != null) {
                    s.setVisible();
                }
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
        self.isActive = ko.observable(false);
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

        questionMap.push(self);
    };

    var Answer = function (key, surveyKey, parentKey, type, question, answer, answerOptions, isReadonly, isValid, isEnabled, isAnswered, isFlaged, comments) {
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
        self.isFlaged = ko.observable(isFlaged);
        
        self.markerStyle = function() {
            var style = "question-marker ";
            if (self.isValid()==false) {
                return style + "invalid";
            }
            if (self.isReadonly == false) {
                 return style + "supervisor";
            }
            return '';
        };
        
        self.type = type;
        self.comments = ko.observableArray(comments || []);
        self.isAnswered = ko.observable(isAnswered);
        self.answerKeys = [];
        self.currentComment = ko.observable('');

        self.addComment = function () {
            var current = self.currentComment().trim();
            if (current) {
                self.comments.push(new Comment(current, currentUser, new Date()));
                self.currentComment('', undefined, new Date());

                var request = dataHelper.createAddCommentRequest();
                request.data = JSON.stringify({
                    surveyKey: self.surveyKey,
                    questionKey: key.publicKey,
                    questionPropagationKey: key.propagatekey,
                    comment: current
                });

                request = $.ajax(request);

                request.done(function (msg) {
                    if (msg.status != 'ok') {
                        console.log("comment not saved");
                    }
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
                    case "SingleImage":
                    case "SingleOption":
                        var selected = ko.utils.arrayFirst(self.answerOptions, function (option) { return option.value == value; }) || undefined;
                        if (selected != undefined) {
                            newAnswer = selected.title;
                            self.answerKeys = [];
                            self.answerKeys.push(selected.key);
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
                        newAnswer = value.mmddyyyy();
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

                answerQuestion();
            });

            self.selectedOptions.subscribe(function (values) {
                var newAnswer = '';
                self.answerKeys = [];

                ko.utils.arrayForEach(values, function (value) {
                    var selected = ko.utils.arrayFirst(self.answerOptions, function (option) { return option.value == value; }) || undefined;

                    if (selected != undefined) {
                        newAnswer += selected.title + ", ";
                        self.answerKeys.push(selected.key);
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

                answerQuestion();
            });
        };

        var answerQuestion = function () {

            var request = dataHelper.createAnswerQuestionRequest();
            request.data = JSON.stringify({
                surveyKey: self.surveyKey,
                questionKey: key.publicKey,
                questionPropagationKey: key.propagatekey,
                answers: self.answerKeys,
                answerValue: self.answer()
            });

            request = $.ajax(request);

            request.done(function (data) {
                console.log("answers saved");
            });

            request.fail(function (data) {
                console.log("answers error");
            });
        };

        var processOptions = function() {
            switch (self.displayMode()) {
            case "SingleImage":
            case "SingleOption":
                var selected = ko.utils.arrayFirst(self.answerOptions, function(option) { return option.isSelected; }) || undefined;
                if (selected != undefined) {
                    self.selectedOption(selected.value);
                }
                break;
            case "MultyImage":
            case "MultyOption":
                var selected = ko.utils.arrayFilter(self.answerOptions, function(option) { return option.isSelected; }) || undefined;
                if (selected != undefined) {
                    self.selectedOptions(ko.utils.arrayMap(selected, function(option) {
                        return option.value;
                    }));
                }
                break;
            case "Numeric":
                self.selectedOption(self.answer());
                break;
            case "Text":
                self.selectedOption(self.answer());
                break;
            case "AutoPropagate":
                self.selectedOption(self.answer());
                break;
            case "DateTime":
                //parse date
                if (!!self.answer() == false) {
                    self.selectedOption(new Date());
                } else {
                    var date = new Date(Date.parse(self.answer()));
                    self.answer(date.mmddyyyy());
                    date = new Date(date.valueOf() + date.getTimezoneOffset() * 60000);
                    self.selectedOption(new Date(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate() + 1, date.getUTCHours(), date.getUTCMinutes(), date.getUTCSeconds()));
                }
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
                case 9:tmpl = "SingleImage"; break;
                case 10:tmpl = "MultyImage"; break;
            }
            return tmpl;
        };

        processOptions();
        answerMap.push(self);
    };

    var AnswerOption = function(key, parentKey, value, title, isSelected, image) {
        var self = this;
        self.key = key;
        self.parentKey = parentKey;
        self.value = value;
        self.title = title;
        self.image = "/Resource/Thumb/" + image;
        self.isSelected = isSelected;
        self.optionFor = "optionFor" + parentKey.id + "_" + key;
        self.groupName = 'optionFor' + parentKey.id;
    };

    var Comment = function(text, user, date) {
        var self = this;
        self.text = text;
        self.user = user;
        self.date = date;
    };

    var User = function(key, name) {
        var self = this;
        self.key = key;
        self.name = name;
    };
    
    var Status = function(key, name, comment) {
        var self = this;
        self.key = key;
        self.name = name;
        self.comment = comment;
    };

    var StatusHistory = function (user, status, date, comment) {
        var self = this;
        self.user = user;
        self.status = status;
        self.date = parseDate(date);
        self.comment = comment;
    };

    var parseDate = function(date) {
        return new Date(parseInt(date.substr(6)));
    };
    
    // our main view model
    var SurveyModel = function (questionnaire) {

        var self = this;

        self.statusHistory = ko.utils.arrayMap(questionnaire.StatusHistory, function (history) {
            return new StatusHistory(history.UserName, history.StatusName, history.ChangeDate, history.Comment);
        });
        
        self.showMode = ko.observable('all');
        self.isMenuHidden = ko.observable(false);

        self.title = questionnaire.Title;

        self.user = new User(questionnaire.User.Id, questionnaire.User.Name);
        if (!!questionnaire.Responsible) {
            self.responsible = new User(questionnaire.Responsible.Id, questionnaire.Responsible.Name);
        } else {
            self.responsible = new User(empty, 'nobody');
        }
        
        
        currentUser = self.user;
        
        self.status = new Status(questionnaire.Status.PublicId, questionnaire.Status.Name, questionnaire.ChangeComment);

        self.menu = ko.observableArray(ko.utils.arrayMap(questionnaire.Navigation.Menu, function (item) {
            return new MenuItem(new Key(item.Key.PublicKey, item.Key.PropagationKey, item.Key.IsPropagated),
                                item.GroupText,
                                item.Level,
                                new Totals(item.Totals));
        }));

        self.screens = ko.observableArray(ko.utils.arrayMap(questionnaire.Screens, function (screen) {

            var questions = ko.observableArray(ko.utils.arrayMap(screen.Questions, function (question) {

                var answers = ko.observableArray(ko.utils.arrayMap(question.Answers, function (answer) {
                    var answerKey = new Key(answer.Key.PublicKey, answer.Key.PropagationKey, answer.Key.IsPropagated);
                    
                    var comments = ko.utils.arrayMap(answer.Comments, function(comment) {
                        return new Comment(comment.Comment, new User(comment.User.Id,comment.User.Name), comment.CommentDate);
                    });

                    var isImageType = false;
                    var options = ko.utils.arrayMap(answer.AnswerOptions, function(option) {
                        isImageType = isImageType || (option.AnswerType == 1);
                        return new AnswerOption(option.PublicKey, answerKey, option.AnswerValue, option.Title, option.Selected, option.AnswerImage);
                    });

                    var type = answer.Type;
                    if (isImageType) {
                        switch (type) {
                            case 0:
                            case 1:
                            case 2: type = 9;break;
                            case 3: type = 10; break;
                        }
                    }
                    return new Answer(
                        answerKey,
                        questionnaire.PublicKey,
                        new Key(answer.ParentKey.PublicKey, answer.ParentKey.PropagationKey, answer.ParentKey.IsPropagated),
                        type,
                        question.Title,
                        answer.Answer,
                        options,
                        answer.IsReadOnly,
                        answer.Valid,
                        answer.Enabled,
                        answer.Answered,
                        answer.IsFlaged,
                        comments);
                }));

                return new Question(question.PublicKey, question.Title, answers);
            }));

            var childScreenKeys = ko.utils.arrayMap(screen.ChildrenKeys, function (child) {
                return new Key(child.PublicKey, child.PropagationKey, child.IsPropagated);
            });

            var captions = [];
            for(var key in screen.Captions) {
                captions.push(new ScreenCaption(new Key(screen.Key.PublicKey, key, true), screen.Captions[key]));
            };

            return new Screen(self, new Key(screen.Key.PublicKey, screen.Key.PropagationKey, screen.Key.IsPropagated), screen.Title, questions, childScreenKeys, captions);
        }));

        self.currentAnswer = ko.observable();
        self.currentScreenKey = ko.observable();

        self.filteredScreen = ko.computed(function () {

            ko.utils.arrayForEach(self.screens(), function (item) {
                item.isVisible(false);
                ko.utils.arrayForEach(item.captions, function (caption) {
                    caption.isVisible(true);
                });
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
                    ko.utils.arrayForEach(questionMap, function (item) {
                        item.isVisible(item.flagedCount() > 0);
                    });

                    return self.screens().filter(function (screen) {
                        return screen.flagedCount() > 0;
                    });
                case 'answered':
                    ko.utils.arrayForEach(questionMap, function (item) {
                        item.isVisible(item.answeredCount() > 0);
                    });

                    return self.screens().filter(function (screen) {
                        return screen.answeredCount() > 0;
                    });
                case 'invalid':
                    ko.utils.arrayForEach(questionMap, function (item) {
                        item.isVisible(item.invalidCount() > 0);
                    });

                    return self.screens().filter(function (screen) {
                        return screen.invalidCount() > 0;
                    });
                case 'supervisor':
                    ko.utils.arrayForEach(questionMap, function (item) {
                        item.isVisible(item.editableCount() > 0);
                    });

                    return self.screens().filter(function (screen) {
                        return screen.editableCount() > 0;
                    });
                case 'enabled':
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

        self.flagAnswer = function (answer, event) {
            answer.isFlaged(!answer.isFlaged());

            var request = dataHelper.createFlagQuestionRequest();

            request.data = JSON.stringify({
                surveyKey: answer.surveyKey,
                questionKey: answer.key.publicKey,
                questionPropagationKey: answer.key.propagatekey,
                isFlaged: answer.isFlaged()
            });

            request = $.ajax(request);

            request.done(function (msg) {
                console.log("comment saved");
            });

            request.fail(function (jqXHR, textStatus) {
                console.log("comment error");
            });

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
            if (!!self.currentAnswer()) {
                $('#question-' + self.currentAnswer().key.publicKey).removeClass('active');
            }
            
            self.currentAnswer(answer);
            
            $('#question-' + answer.key.publicKey).addClass('active');
            
            $('#stacks').addClass('detail-visible');
            event.stopPropagation();

            $('#details .body').css('top', ($('#details .title').outerHeight() + 'px'));

           $('[data-toggle="dropdown"]').parent().removeClass('open');
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
    };

    $(document).ready(function () {
        
        var setMinHeight = function () {
            var windowHeight = $(window).height();
            var navigationHeight = $('.navbar.navbar-fixed-top').height();
            $('#stacks').css('min-height', (windowHeight - navigationHeight) + 'px');
            $('#wrapper').css('margin-top', navigationHeight + 'px');
            $('#umbrella').css('top', navigationHeight + 'px');

        };
        
        setMinHeight();
        $(window).resize(function () {
            setMinHeight();
        });
        
        // bind a new instance of our view model to the page
        viewModel = new SurveyModel(questionnaire || {});
        ko.applyBindings(viewModel);

        $('#groups .body').css('top', ($('#groups .title').outerHeight() + 'px'));

        ko.utils.arrayForEach(answerMap, function (item) {
            item.subscribe();
        });

        // set up filter routing
        Router({
            '/:filter': viewModel.showMode,
            '/group/:groupId/:propId': function (groupId, propId) {
                console.log('group was selected');
                
                ko.utils.arrayForEach(viewModel.menu(), function (item) {
                    item.isCurrent(false);
                });

                var key = groupId + propId;
                
                if (keyMap[key] != undefined) {
                    var id = keyMap[key];
                    
                    var menuItem = ko.utils.arrayFirst(viewModel.menu(), function(item) {
                        return item.key.id == id;
                    });
                    
                    viewModel.currentScreenKey(menuItem.key);
                    
                    menuItem.isCurrent(true);
                    
                    viewModel.showMode('group');
                }
            }
        }).init();

        $('#umbrella').hide();
    });

} ());