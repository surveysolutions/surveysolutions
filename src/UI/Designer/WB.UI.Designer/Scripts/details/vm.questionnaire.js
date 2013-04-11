define('vm.questionnaire',
    ['ko', 'underscore', 'config', 'datacontext', 'router', 'messenger', 'store', 'model', 'bootbox', 'ace/theme/designer', 'ace/mode/ncalc'],
    function (ko, _, config, datacontext, router, messenger, store, model, bootbox, ncalc_theme, ncalc_mode) {
        var filter = ko.observable('')/*.extend({ throttle: 400 })*/,
            isFilterMode = ko.observable(false),
            selectedGroup = ko.observable(),
            selectedQuestion = ko.observable(),
            questionnaire = ko.observable(model.Questionnaire.Nullo),
            chapters = ko.observableArray(),
            errors = ko.observableArray(),
            searchResult = ko.observableArray(),
            statistics = {
                questions: ko.observable(),
                groups: ko.observable(),
            };
        isInitialized = false;
        activate = function (routeData, callback) {
            messenger.publish.viewModelActivated({ canleaveCallback: canLeave });

            if (!isInitialized) {
                getChapters();
                questionnaire(datacontext.questionnaire);
                calcStatistics();
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

            $("a[data-toggle=popover]")
                    .popover()
                    .click(function (e) {
                        e.preventDefault();
                    });
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
            var question = new model.Question();
            question.parent(parent);

            datacontext.questions.add(question);

            parent.childrenID.push({ type: question.type(), id: question.id() });
            parent.fillChildren();
            router.navigateTo(question.getHref());
            calcStatistics();
        },
        addChapter = function () {
            var group = new model.Group();
            group.level(0);
            group.title('New Chapter');
            group.parent(null);
            datacontext.groups.add(group);
            chapters.push(group);
            router.navigateTo(group.getHref());
            calcStatistics();
        },
        addGroup = function (parent) {
            var group = new model.Group();
            group.parent(parent);
            datacontext.groups.add(group);
            parent.childrenID.push({ type: group.type(), id: group.id() });
            parent.fillChildren();
            router.navigateTo(group.getHref());
            calcStatistics();
        },
        deleteGroup = function (item) {
            bootbox.confirm("Are you sure you want to delete this question?", function (result) {
                if (result == false)
                    return;

                if (item.isNew()) {
                    deleteGroupSuccessCallback(item);
                } else {
                    datacontext.sendCommand(
                        config.commands.deleteGroup,
                        item,
                        {
                            success: function () {
                                deleteGroupSuccessCallback(item);
                            },
                            error: function (d) {
                                errors.removeAll();
                                errors.push(d);
                                showOutput();
                            }
                        });
                }
            });
        },
        deleteGroupSuccessCallback = function (item) {
            datacontext.groups.removeGroup(item.id());

            var parent = item.parent();
            if (!(_.isUndefined(parent) || (_.isNull(parent)))) {
                var child = _.find(parent.childrenID(), { 'id': item.id() });
                parent.childrenID.remove(child);
                parent.fillChildren();
                datacontext.questions.cleanTriggers(child);
                router.navigateTo(parent.getHref());
            } else {
                chapters(datacontext.groups.getChapters());
                router.navigateTo(config.hashes.details);
            }
            calcStatistics();
            hideOutput();
        },
        deleteQuestion = function (item) {
            bootbox.confirm("Are you sure you want to delete this question?", function (result) {
                if (result == false)
                    return;

                if (item.isNew()) {
                    deleteQuestionSuccessCallback(item);
                } else {
                    datacontext.sendCommand(
                        config.commands.deleteQuestion,
                        item,
                        {
                            success: function () {
                                deleteQuestionSuccessCallback(item);

                            },
                            error: function (d) {
                                errors.removeAll();
                                errors.push(d);
                                showOutput();
                            }
                        });
                }
            });
        },
        deleteQuestionSuccessCallback = function (item) {
            datacontext.questions.removeById(item.id());

            var parent = item.parent();
            var child = _.find(parent.childrenID(), { 'id': item.id() });
            parent.childrenID.remove(child);
            parent.fillChildren();
            calcStatistics();

            if (isFilterMode() == true) {
                filter.valueHasMutated();
            } else {
                router.navigateTo(parent.getHref());
            }

            hideOutput();
        },
        saveGroup = function (group) {
            datacontext.sendCommand(
                group.isNew() ? config.commands.createGroup : config.commands.updateGroup,
                group,
                {
                    success: function () {
                        group.isNew(false);
                        group.dirtyFlag().reset();
                        calcStatistics();
                        hideOutput();
                    },
                    error: function (d) {
                        errors.removeAll();
                        errors.push(d);
                        showOutput();
                    }
                });
        },
        saveQuestion = function (question) {
            datacontext.sendCommand(
                question.isNew() ? config.commands.createQuestion : config.commands.updateQuestion,
                question,
                {
                    success: function () {
                        question.isNew(false);
                        question.dirtyFlag().reset();
                        calcStatistics();
                        hideOutput();
                    },
                    error: function (d) {
                        errors.removeAll();
                        errors.push(d);
                        showOutput();
                    }
                });
        },
        clearFilter = function () {
            filter('');
        },
        filterContent = function () {
            var query = filter().trim().toLowerCase();
            isFilterMode(query !== '');
            searchResult.removeAll();
            if (query != '') {
                searchResult(datacontext.questions.search(query));
            }
        },
        isMovementPossible = function (arg) {

            var fromId = arg.sourceParent.id;
            var toId = arg.targetParent.id;
            var moveItemType = arg.item.type().replace('View','').toLowerCase();
            var isDropedInChapter = (_.isNull(toId) || _.isUndefined(toId));
            var isDraggedFromChapter = (_.isNull(fromId) || _.isUndefined(fromId));
            
            if (isDropedInChapter && moveItemType == "question") {
                arg.cancelDrop = true;
                config.logger(config.warnings.cantMoveQuestionOutsideGroup);
                return;
            }
            var target = datacontext.groups.getLocalById(toId);
            var source = datacontext.groups.getLocalById(fromId);
            
            if (isDropedInChapter && moveItemType == "group" && arg.item.gtype() !== "None") {
                arg.cancelDrop = true;
                config.logger(config.warnings.propagatedGroupCantBecomeChapter);
                return;
            }
            
            if (isDropedInChapter && target.gtype() !== "None" && moveItemType == "group") {
                arg.cancelDrop = true;
                config.logger(config.warnings.cantMoveGroupIntoPropagatedGroup);
                return;
            }

            var item = arg.item;

            var moveCommand = {
                targetGroupId: toId,
                targetIndex: arg.targetIndex
            };
            moveCommand[moveItemType + 'Id'] = item.id();
            
            datacontext.sendCommand(
               config.commands[moveItemType + "Move"],
               moveCommand,
               {
                   success: function (d) {
                       if (isDraggedFromChapter) {
                           var child = _.find(datacontext.questionnaire.childrenID(), { 'id': item.id() });
                           datacontext.questionnaire.childrenID.remove(child);
                           chapters(datacontext.groups.getChapters());
                       } else {
                           var child = _.find(source.childrenID(), { 'id': item.id() });
                           source.childrenID.remove(child);
                           source.fillChildren();
                       }
                       
                       if (isDropedInChapter) {
                           item.level(0);
                           datacontext.questionnaire.childrenID.splice(arg.targetIndex, 0, { type: item.type(), id: item.id() });
                           chapters(datacontext.groups.getChapters());
                       } else {
                           if (moveItemType == "group") {
                               item.level(target.level() + 1);
                           }
                           target.childrenID.splice(arg.targetIndex, 0, { type: item.type(), id: item.id() });
                           target.fillChildren();
                       }
                   },
                   error: function (d) {
                       arg.cancelDrop = true;
                   }
               });
        },
        calcStatistics = function () {
            statistics.questions(datacontext.questions.getAllLocal().length);
            statistics.groups(datacontext.groups.getAllLocal().length);
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
            isMovementPossible: isMovementPossible,
            saveGroup: saveGroup,
            deleteGroup: deleteGroup,
            saveQuestion: saveQuestion,
            deleteQuestion: deleteQuestion,
            showOutput: showOutput,
            hideOutput: hideOutput,
            errors: errors,
            statistics: statistics,
            searchResult: searchResult
        };
    });