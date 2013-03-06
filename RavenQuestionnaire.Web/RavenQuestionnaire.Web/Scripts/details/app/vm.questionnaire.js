define('vm.questionnaire',
    ['ko', 'underscore', 'config', 'datacontext', 'router', 'messenger', 'store', 'model'],
    function(ko, _, config, datacontext, router, messenger, store, model) {
        var filter = ko.observable('').extend({throttle: 400}),
            isFilterMode = ko.observable(false),
            selectedGroup = ko.observable(),
            selectedQuestion = ko.observable(),
            questionnaire = ko.observable(model.Questionnaire.Nullo),
            chapters = ko.observableArray(),
            
            isInitialized = false;
            activate = function (routeData, callback) {
                messenger.publish.viewModelActivated({ canleaveCallback: canLeave });
                
                if (!isInitialized) {
                    getChapters();
                    questionnaire(datacontext.questionnaire);
                     $('#groups .body').css('top', ($('#groups .title').outerHeight() + 'px'));
                }
                if (routeData.has('question')) {
                    editQuestion(routeData.question);
                }
                if (routeData.has('group')) {
                    editGroup(routeData.group);
                    }
                isInitialized = true;
            },
            canLeave = function() {
                return true;
            },
            getChapters = function () {
                 if (!chapters().length) {
                     chapters(datacontext.groups.getChapters());
                 }
             },
            editQuestion = function(id) {
                var question = datacontext.questions.getLocalById(id);
                question.localPropagatedGroups(datacontext.groups.getPropagateableGroups());
                selectedQuestion(question);
                openDetails("show-question");
            },
            editGroup = function (id) {
                var group = datacontext.groups.getLocalById(id);
                
                selectedGroup(group);
                openDetails("show-group");
            },
            openDetails = function (style) {
                $('#stacks').removeClass("show-question").removeClass("show-group");
                $('#stacks').addClass('detail-visible').addClass(style);
                $('#details-question .body').css('top', ($('#details-question .title').outerHeight() + 'px'));
                $('#details-group .body').css('top', ($('#details-group .title').outerHeight() + 'px'));
            },
            closeDetails = function () {
                $('#stacks').removeClass("show-question").removeClass("show-group");
                $('#stacks').removeClass('detail-visible');
            },
            closeMenu = function () {
                $('#stacks').addClass('menu-hidden');
            },
            showMenu = function () {
                 $('#stacks').removeClass('menu-hidden');
            },
            addQuestion = function (parent) {
                console.log(parent);
                var question = model.Question.Nullo;
                question.id(Math.uuid());
                
                parent.childrenID.push({type:question.type(), id :question.id() });
                parent.fillChildren();
                datacontext.questions.add(question);
            },
            addGroup = function (parent) {
                console.log(parent);
                var group = model.Group.Nullo;
                group.id(Math.uuid());
                debugger;
                parent.childrenID.push({type :group.type(), id :group.id() });
                parent.fillChildren();
                datacontext.groups.add(group);
                
            },
            clearFilter = function() {
                filter('');
            },
            filterContent = function() {
                isFilterMode(filter().trim() !== '');
            },
            init = function () {
                filter.subscribe(filterContent);
             };

        init();

        return {
            activate: activate,
            questionnaire:questionnaire,
            chapters: chapters,
            selectedGroup : selectedGroup,
            selectedQuestion: selectedQuestion,
            closeDetails: closeDetails,
            addQuestion: addQuestion,
            addGroup: addGroup,
            clearFilter: clearFilter,
            filter: filter,
            isFilterMode: isFilterMode
        };
    });