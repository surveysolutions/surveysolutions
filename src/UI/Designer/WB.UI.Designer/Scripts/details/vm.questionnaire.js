define('vm.questionnaire',
    ['ko', 'underscore', 'config', 'datacontext', 'router', 'messenger', 'store', 'model'],
    function (ko, _, config, datacontext, router, messenger, store, model) {
        var filter = ko.observable('').extend({ throttle: 400 }),
            isFilterMode = ko.observable(false),
            selectedGroup = ko.observable(),
            selectedQuestion = ko.observable(),
            questionnaire = ko.observable(model.Questionnaire.Nullo),
            chapters = ko.observableArray(),
            errors = ko.observableArray(),
            isInitialized = false;
        activate = function (routeData, callback) {
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
        canLeave = function () {
            return true;
        },
        getChapters = function () {
            if (!chapters().length) {
                chapters(datacontext.groups.getChapters());
            }
        },
        editQuestion = function (id) {
            var question = datacontext.questions.getLocalById(id);
            if (question.isNullo) {
                return;
            }
            question.isSelected(true);
            question.localPropagatedGroups(datacontext.groups.getPropagateableGroups());
            selectedQuestion(question);
            selectedQuestion.valueHasMutated();
            openDetails("show-question");
        },
        editGroup = function (id) {
            var group = datacontext.groups.getLocalById(id);
            if (group.isNullo) {
                return;
            }
            group.isSelected(true);
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
        showOutput = function () {
            $('#stacks').addClass('output-visible');
        },
        hideOutput = function () {
            $('#stacks').removeClass('output-visible');
        },
        addQuestion = function (parent) {
            console.log(parent);
            var question = new model.Question();
            question.parent(parent);

            datacontext.questions.add(question);

            parent.childrenID.push({ type: question.type(), id: question.id() });
            parent.fillChildren();
            router.navigateTo(question.getHref());
        },
        addChapter = function () {
            var group = new model.Group();
            group.level(0);
            group.parent(null);
            datacontext.groups.add(group);
            chapters.push(group);
            router.navigateTo(group.getHref());
        },
        addGroup = function (parent) {
            console.log(parent);
            var group = new model.Group();
            group.parent(parent);
            datacontext.groups.add(group);
            parent.childrenID.push({ type: group.type(), id: group.id() });
            parent.fillChildren();
            router.navigateTo(group.getHref());
        },
        deleteGroup = function (item) {
            datacontext.sendCommand(
               config.commands.deleteGroup,
               item,
               {
                   success: function () {
                       datacontext.groups.removeById(item.id());
                       
                       var parent = item.parent();
                       if (!_.isUndefined(parent)) {
                           var child = _.find(parent.childrenID(), { 'id': item.id() });
                           parent.childrenID.remove(child);
                           parent.fillChildren();
                           datacontext.questions.cleanTriggers(child);
                           router.navigateTo(parent.getHref());
                       } else {
                           router.navigateTo(config.hashes.details);
                       }
                   },
                   error: function (d) {
                       console.log(d);
                       errors.removeAll();
                       errors.push(d);
                       showOutput();
                   }
               });
        },
        deleteQuestion = function (item) {
            datacontext.sendCommand(
               config.commands.deleteGroup,
               item,
               {
                   success: function () {
                       var parent = item.parent();
                       var child = _.find(parent.childrenID(), { 'id': item.id() });
                       parent.childrenID.remove(child);
                       parent.fillChildren();
                       router.navigateTo(parent.getHref());
                   },
                   error: function (d) {
                       console.log(d);
                       errors.removeAll();
                       errors.push(d);
                       showOutput();
                   }
               });
            
           
        },
        saveGroup = function (group) {
            console.log(group);
            datacontext.sendCommand(
                group.isNew() ? config.commands.createGroup : config.commands.updateGroup,
                group,
                {
                    success: function () {
                        group.isNew(false);
                        group.dirtyFlag().reset();
                    },
                    error: function (d) {
                        console.log(d);
                        errors.removeAll();
                        errors.push(d);
                        showOutput();
                    }
                });
        },
        saveQuestion = function () {

        },
        clearFilter = function () {
            filter('');
        },
        filterContent = function () {
            isFilterMode(filter().trim() !== '');
        },
        afterMove = function (arg) {
            console.log(arg);
        },
        isMovementPossible = function (arg) {
            console.log(arg.targetParent().title());
            console.log("souce : " + arg.item.type());

            arg.cancelDrop = true;
        },
        init = function () {
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
            addChapter: addChapter,
            clearFilter: clearFilter,
            filter: filter,
            isFilterMode: isFilterMode,
            afterMove: afterMove,
            isMovementPossible: isMovementPossible,
            saveGroup: saveGroup,
            deleteGroup: deleteGroup,
            saveQuestion: saveQuestion,
            deleteQuestion: deleteQuestion,
            showOutput: showOutput,
            hideOutput: hideOutput,
            errors: errors
        };
    });