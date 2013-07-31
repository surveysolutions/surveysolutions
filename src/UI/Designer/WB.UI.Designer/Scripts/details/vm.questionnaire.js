define('vm.questionnaire',
    ['ko', 'underscore', 'config', 'utils', 'datacontext', 'router', 'model', 'bootbox'],
    function (ko, _, config, utils, datacontext, router, model, bootbox) {
        var filter = ko.observable('')/*.extend({ throttle: 400 })*/,
            isFilterMode = ko.observable(false),
            selectedGroup = ko.observable(),
            selectedQuestion = ko.observable(),
            questionnaire = ko.observable(model.Questionnaire.Nullo),
            chapters = ko.observableArray(),
            errors = ko.observableArray(),
            searchResult = ko.observableArray(),
            statistics = new model.Statistic(),
            isInitialized = false,
            cloneQuestion = function (question) {
                if (question.isNew())
                    return;
                var parent = question.parent();
                var index = question.index();
                var clonedQuestion = question.clone();

                datacontext.questions.add(clonedQuestion);

                parent.childrenID.splice(index + 1, 0, { type: clonedQuestion.type(), id: clonedQuestion.id() });
                parent.fillChildren();
                router.navigateTo(clonedQuestion.getHref());
                calcStatistics();
            },
            cloneGroup = function (group) {
                if (group.isNew())
                    return;

                var clonedGroup = group.clone();
                datacontext.groups.add(clonedGroup);
                clonedGroup.fillChildren();

                if (group.hasParent()) {
                    var parent = group.parent();
                    var index = group.index();
                    parent.childrenID.splice(index + 1, 0, { type: clonedGroup.type(), id: clonedGroup.id() });
                    parent.fillChildren();
                } else {
                    var item = utils.findById(datacontext.questionnaire.childrenID(), group.id());
                    datacontext.questionnaire.childrenID.splice(item.index + 1, 0, { type: clonedGroup.type(), id: clonedGroup.id() });
                    chapters(datacontext.groups.getChapters());
                }
                router.navigateTo(clonedGroup.getHref());
                calcStatistics();
            },
            activate = function (routeData, callback) {
                

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
                questionnaire().isSelected(false);

                if (routeData.has('questionnaire')) {
                    editQuestionnaire(routeData.questionnaire);
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
            editQuestionnaire = function () {
                questionnaire().isSelected(true);
                openDetails("show-questionnaire");
            },
            editQuestion = function (id) {
                var question = datacontext.questions.getLocalById(id);
                if (_.isNull(question) || question.isNullo) {
                    return;
                }
                question.isSelected(true);
                question.localPropagatedGroups(datacontext.groups.getPropagateableGroups());
                selectedQuestion(question);
                selectedQuestion.valueHasMutated();
                openDetails("show-question");
                $('#alias').focus();
            },
            editGroup = function (id) {
                var group = datacontext.groups.getLocalById(id);
                if (_.isNull(group) || group.isNullo) {
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
            isOutputVisible = ko.observable(false),
        toggleOutput = function () {
            isOutputVisible(!isOutputVisible());
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
            datacontext.questionnaire.childrenID.push({ type: group.type(), id: group.id() });
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
                                    showError(d);
                                errors.removeAll();
                                errors.push(d);
                                isOutputVisible(true);
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

                _.each(datacontext.groups.getAllLocal(), function (group) {
                    group.fillChildren();
                });
                //parent.fillChildren();
                datacontext.questions.cleanTriggers(child);
                router.navigateTo(parent.getHref());
            } else {
                var child = _.find(datacontext.questionnaire.childrenID(), { 'id': item.id() });
                datacontext.questionnaire.childrenID.remove(child);

                chapters(datacontext.groups.getChapters());
                router.navigateTo(config.hashes.details);
            }
            calcStatistics();
            isOutputVisible(false);
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
                                    showError(d);
                                errors.removeAll();
                                errors.push(d);
                                isOutputVisible(true);
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

            isOutputVisible(false);
        },
        saveGroup = function (group) {

            if (group.hasParent() && group.parent().isNew()) {
                config.logger(config.warnings.saveParentFirst);
                return;
            }

            var command = '';
            if (group.isNew()) {
                if (group.isClone()) {
                    command = config.commands.cloneGroup;
                } else {
                    command = config.commands.createGroup;
                }
            } else {
                command = config.commands.updateGroup;
            }

            group.canUpdate(false);

            datacontext.sendCommand(
                command,
                group,
                {
                    success: function () {
                        group.isNew(false);
                        group.dirtyFlag().reset();
                        calcStatistics();
                        isOutputVisible(false);
                        group.canUpdate(true);
                        group.commit();
                    },
                    error: function (d) {
                            showError(d);
                        errors.removeAll();
                        errors.push(d);
                        isOutputVisible(true);
                        group.canUpdate(true);
                    }
                });
        },
        saveQuestion = function (question) {

            if (question.hasParent() && question.parent().isNew()) {
                config.logger(config.warnings.saveParentFirst);
                return;
            }

            var command = '';
            if (question.isNew()) {
                if (question.isClone()) {
                    command = config.commands.cloneQuestion;
                } else {
                    command = config.commands.createQuestion;
                }
            } else {
                command = config.commands.updateQuestion;
            }

            question.canUpdate(false);

            datacontext.sendCommand(
                command,
                question,
                {
                    success: function () {
                        question.isNew(false);
                        question.dirtyFlag().reset();
                        calcStatistics();
                        isOutputVisible(false);
                        question.canUpdate(true);
                        question.commit();
                    },
                    error: function (d) {
                            showError(d);
                        errors.removeAll();
                        errors.push(d);
                        isOutputVisible(true);
                        question.canUpdate(true);
                    }
                });
        },
        saveQuestionnaire = function (questionnaire) {

            questionnaire.canUpdate(false);

            datacontext.sendCommand(
                config.commands.updateQuestionnaire,
                questionnaire,
                {
                    success: function () {
                        questionnaire.dirtyFlag().reset();
                        isOutputVisible(false);
                        questionnaire.canUpdate(true);
                    },
                    error: function (d) {
                            showError(d);
                        errors.removeAll();
                        errors.push(d);
                        isOutputVisible(true);
                        questionnaire.canUpdate(true);
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
        isMovementPossible = function (arg, event, ui) {

            var fromId = arg.sourceParent.id;
            var toId = arg.targetParent.id;
            var moveItemType = arg.item.type().replace('View', '').toLowerCase();
            var isDropedInChapter = (_.isNull(toId) || _.isUndefined(toId));
            var isDraggedFromChapter = (_.isNull(fromId) || _.isUndefined(fromId));

            if (arg.item.isNew()) {
                arg.cancelDrop = true;
                config.logger(config.warnings.cantMoveUnsavedItem);
                return;
            }

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

            if (!isDropedInChapter && target.gtype() !== "None" && moveItemType == "group") {
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
                        _.each(datacontext.groups.getAllLocal(), function (group) {
                            group.fillChildren();
                        });

                        chapters(datacontext.groups.getChapters());

                            showError(d);
                        errors.removeAll();
                        errors.push(d);
                        isOutputVisible(true);
                    }
                });
        },
        calcStatistics = function () {
            var questions = datacontext.questions.getAllLocal();
            var groups = datacontext.groups.getAllLocal();
            statistics.questions(questions.length);
            statistics.groups(groups.length);
            var counter = _.countBy(questions, function (q) { return q.isNew(); });
            statistics.unsavedQuestion(_.isUndefined(counter['true']) ? 0 : counter['true']);
            counter = _.countBy(groups, function (g) { return g.isNew(); });
            statistics.unsavedGroups(_.isUndefined(counter['true']) ? 0 : counter['true']);
        },
        toogleGroups = function () {
            $('.ui-expander-head:not(.ui-expander-head-collapsed)').click();
        },
        collapseItemIfNeeded = function (arg) {
            console.log(arg.item);
        },
        expandItemIfNeeded = function (arg) {
            console.log(arg);
        },
        init = function () {
            filter.subscribe(filterContent);
            ko.bindingHandlers.sortable.options.start = function (arg, ui) {
                if ($(ui.item).children('.ui-expander').length > 0) {
                    var button = $(ui.item).children('.ui-expander').children('.ui-expander-head');
                    if ($(button).hasClass('ui-expander-head-collapsed') == false) {
                        button.click();
                    }
                }
            };
        },
        showError = function(message) {
                errors.removeAll();
                errors.push(message);
                showOutput();
        };

        init();

        return {
            cloneQuestion: cloneQuestion,
            cloneGroup: cloneGroup,
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
            isOutputVisible: isOutputVisible,
            toggleOutput: toggleOutput,
            errors: errors,
            statistics: statistics,
            searchResult: searchResult,
            saveQuestionnaire: saveQuestionnaire,
            toogleGroups: toogleGroups
        };
    });