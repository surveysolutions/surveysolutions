define('app/viewmodel', ['knockout', 'app/datacontext', 'director', 'input', 'app/config'],
    function (ko, datacontext, Router, input, config) {
        var
			questionnaire = ko.observable(),
			groups = ko.observableArray(),
			questions = ko.observableArray(),
			isFilterOpen = ko.observable(true),
            toggleFilter = function () {
                if (isFilterOpen()) {
                    $('#wrapper').addClass('menu-hidden');
                } else {
                    $('#wrapper').removeClass('menu-hidden');
                }
                isFilterOpen(!isFilterOpen());
            },
            isSaving = ko.observable(false),

			flagedCount = ko.computed(function () {
			    return _.reduce(questions(), function (count, question) {
			        return count + (question.isFlagged() ? 1 : 0);
			    }, 0);
			}),

			answeredCount = ko.computed(function () {
			    return _.reduce(questions(), function (count, question) {
			        return count + (question.isAnswered() ? 1 : 0);
			    }, 0);
			}),
            commentedCount = ko.computed(function () {
                return _.reduce(questions(), function (count, question) {
                    return count + (question.comments.length > 0 ? 1 : 0);
                }, 0);
            }),

			invalidCount = ko.computed(function () {
			    return _.reduce(questions(), function (count, question) {
			        return count + (question.isValid() ? 0 : 1);
			    }, 0);
			}),

			editableCount = ko.computed(function () {
			    return _.reduce(questions(), function (count, question) {
			        return count + (question.scope() == 1 ? 1 : 0);
			    }, 0);
			}),

			enabledCount = ko.computed(function () {
			    return _.reduce(questions(), function (count, question) {
			        return count + (question.isEnabled() ? 1 : 0);
			    }, 0);
			}),
			filter = ko.observable('all'),
            applyQuestionFilter = function(f) {
                filter(f);
                _.each(groups(), function (group) {
                    group.isVisible(true);
                });
                _.each(questions(), function (question) {
                    question.matchFilter(f);
                });
            },
            init = function () {
                questionnaire(datacontext.questionnaire);
                groups(datacontext.groups.getAllLocal());
                questions(datacontext.questions.getAllLocal());
                Router({
                    '/group/:groupId': function (groupId) {
                        applyQuestionFilter('all');
                        var visibleGroupsIds = [groupId];
                        _.each(groups(), function (group) {
                            if (_.contains(visibleGroupsIds, group.uiId())) {
                                group.isVisible(true);
                            } else if (_.contains(visibleGroupsIds, group.parentId())) {
                                visibleGroupsIds.push(group.uiId());
                                group.isVisible(true);
                            } else {
                                group.isVisible(false);
                            }
                        });
                    },
                    '/:filter': function (f) {
                        applyQuestionFilter(f);
                    }
                }).init();
            };
        
        return {
            filter: filter,
            commentedCount: commentedCount,
            flagedCount: flagedCount,
            answeredCount: answeredCount,
            invalidCount: invalidCount,
            editableCount: editableCount,
            enabledCount: enabledCount,
            questionnaire: questionnaire,
            groups: groups,
            toggleFilter: toggleFilter,
            isFilterOpen: isFilterOpen,
            isSaving: isSaving,
            init: init
        };
    });