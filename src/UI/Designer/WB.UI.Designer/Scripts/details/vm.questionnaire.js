define('vm.questionnaire',
    ['ko', 'underscore', 'config', 'datacontext', 'router', 'messenger', 'store', 'model'],
    function(ko, _, config, datacontext, router, messenger, store, model) {
        var filter = ko.observable('').extend({ throttle: 400 }),
            isFilterMode = ko.observable(false),
            selectedGroup = ko.observable(),
            selectedQuestion = ko.observable(),
            questionnaire = ko.observable(model.Questionnaire.Nullo),
            chapters = ko.observableArray(),
            isInitialized = false;
        activate = function(routeData, callback) {
            messenger.publish.viewModelActivated({ canleaveCallback: canLeave });

            if (!isInitialized) {
                getChapters();
                questionnaire(datacontext.questionnaire);
                $('#groups .body').css('top', ($('#groups .title').outerHeight() + 'px'));
            } 
            if (!_.isUndefined(selectedGroup())) {
                selectedGroup().isSelected(false);
            }
            if (!_.isUndefined(selectedQuestion())) {
                selectedQuestion().isSelected(false);
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
        getChapters = function() {
            if (!chapters().length) {
                chapters(datacontext.groups.getChapters());
            }
        },
        editQuestion = function(id) {
            var question = datacontext.questions.getLocalById(id);
            question.isSelected(true);
            question.localPropagatedGroups(datacontext.groups.getPropagateableGroups());
            selectedQuestion(question);
            selectedQuestion.valueHasMutated();
            openDetails("show-question");
        },
        editGroup = function(id) {
            var group = datacontext.groups.getLocalById(id);
            group.isSelected(true);
            selectedGroup(group);
            openDetails("show-group");
        },
        openDetails = function(style) {
            $('#stacks').removeClass("show-question").removeClass("show-group");
            $('#stacks').addClass('detail-visible').addClass(style);
            $('#details-question .body').css('top', ($('#details-question .title').outerHeight() + 'px'));
            $('#details-group .body').css('top', ($('#details-group .title').outerHeight() + 'px'));
        },
        closeDetails = function() {
            $('#stacks').removeClass("show-question").removeClass("show-group");
            $('#stacks').removeClass('detail-visible');
        },
        closeMenu = function() {
            $('#stacks').addClass('menu-hidden');
        },
        showMenu = function() {
            $('#stacks').removeClass('menu-hidden');
        },
        addQuestion = function(parent) {
            console.log(parent);
            var question = new model.Question();
            datacontext.questions.add(question);
            parent.childrenID.push({ type: question.type(), id: question.id() });
            parent.fillChildren();
            router.navigateTo(question.getHref());
        },
        addGroup = function(parent) {
            console.log(parent);
            var group = new model.Group();
            debugger;
            datacontext.groups.add(group);
            parent.childrenID.push({ type: group.type(), id: group.id() });
            parent.fillChildren();
            router.navigateTo(group.getHref());
        },
        deleteGroup = function() {
            
        },
        deleteQuestion = function() {
            
        },
        saveGroup = function(group) {
            console.log(group);
            datacontext.groups.update(group, {
                success: function (d) {
                    console.log("ok");
                },
                error: function(d) {
                    console.log("fail");
                }
            });
        },
        saveQuestion = function() {
            
        },
        clearFilter = function() {
            filter('');
        },
        filterContent = function() {
            isFilterMode(filter().trim() !== '');
        },
        afterMove = function(arg) {
            console.log(arg);
        },
        isMovementPossible = function (arg) {
            console.log(arg.targetParent().title());
            console.log("souce : " + arg.item.type());
            
            arg.cancelDrop = true;
        },
        init = function() {
            filter.subscribe(filterContent);
            
        };

        init();

        return {
            activate: activate,
            questionnaire: questionnaire,
            chapters: chapters,
            selectedGroup: selectedGroup,
            selectedQuestion: selectedQuestion,
            closeDetails: closeDetails,
            addQuestion: addQuestion,
            addGroup: addGroup,
            clearFilter: clearFilter,
            filter: filter,
            isFilterMode: isFilterMode,
            afterMove: afterMove,
            isMovementPossible: isMovementPossible,
            saveGroup: saveGroup,
            deleteGroup: deleteGroup,
            saveQuestion: saveQuestion,
            deleteQuestion: deleteQuestion
        };
    });