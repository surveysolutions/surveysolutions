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
				return _.reduce(questions(), function(count, question) {
					return count + (question.isFlagged() ? 1: 0);
				}, 0);
			}),

			answeredCount = ko.computed(function () {
				return _.reduce(questions(), function(count, question) {
					return count + (question.isAnswered() ? 1: 0);
				}, 0);
			}),
            commentedCount = ko.computed(function () {
                return _.reduce(questions(), function (count, question) {
                    return count + (question.comments.length > 0 ? 1 : 0);
                }, 0);
            }),
            
			invalidCount = ko.computed(function () {
				var count = 0;
				
				return count;
			}),

			editableCount = ko.computed(function () {
				var count = 0;
				
				return count;
			}),

			enabledCount = ko.computed(function () {
				var count = 0;
				
				return count;
			}),
			filter = ko.observable('all'),
            init = function () {
                questionnaire(datacontext.questionnaire);
				groups(datacontext.groups.getAllLocal());
				questions(datacontext.questions.getAllLocal());
				 Router({
                    '/:filter': filter,
                    '/group/:groupId/:propId': function (groupId, propId) {
                        console.log('group was selected');
                    }
                }).init();
            };
        return {
            filter: filter,
            commentedCount : commentedCount,
			flagedCount: flagedCount,
			answeredCount:answeredCount,
			invalidCount:invalidCount,
			editableCount:editableCount,
			enabledCount:enabledCount,
			questionnaire : questionnaire,
			groups : groups,
            toggleFilter: toggleFilter,
            isFilterOpen: isFilterOpen,
            isSaving : isSaving,
            init: init
        };
    });