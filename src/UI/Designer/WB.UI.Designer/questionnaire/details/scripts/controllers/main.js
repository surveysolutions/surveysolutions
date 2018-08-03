angular.module('designerApp')
    .controller('MainCtrl',
        function ($rootScope, $scope, $state, $i18next, $sce, questionnaireService, commandService, verificationService, webTesterService, utilityService, hotkeys, $uibModal, notificationService, userService) {
            $scope.verificationStatus = {
                errors: null,
                warnings: null,
                visible: false,
                time: new Date()
            };

            $scope.questionnaire = {
                questionsCount: 0,
                groupsCount: 0,
                rostersCount: 0
            };

            var openCompilationPage = 'ctrl+shift+b';
            var focusTreePane = 'shift+alt+x';
            var focusEditorPane = 'shift+alt+e';
            var openChaptersPane = 'left';

            hotkeys.add({
                combo: openCompilationPage,
                allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                callback: function (event) {
                    window.open("../../questionnaire/expressiongeneration/" + $state.params.questionnaireId);
                    event.preventDefault();
                }
            });

            hotkeys.add({
                combo: 'ctrl+p',
                allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                description: $i18next.t('HotkeysPrint'),
                callback: function (event) {
                    window.open("../../pdf/printpreview/" + $state.params.questionnaireId, "_blank");
                    event.preventDefault();
                }
            });
            
            hotkeys.add({
                combo: 'ctrl+b',
                allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                description: $i18next.t('Compile'),
                callback: function (event) {
                    $scope.verify();
                    event.preventDefault();
                }
            });

            hotkeys.add({
                combo: 'esc',
                allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                callback: function () {
                    $scope.verificationStatus.visible = false;
                }
            });

            if (hotkeys.get(focusTreePane) !== false) {
                hotkeys.del(focusTreePane);
            }
            hotkeys.add({
                combo: focusTreePane,
                allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                description: $i18next.t('HotkeysFocusTree'),
                callback: function (event) {
                    event.preventDefault();
                    document.activeElement.blur();
                }
            });

            if (hotkeys.get(focusEditorPane) !== false) {
                hotkeys.del(focusEditorPane);
            }
            hotkeys.add({
                combo: focusEditorPane,
                allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                description: $i18next.t('HotkeysFocusTitle'),
                callback: function (event) {
                    event.preventDefault();
                    $($(".question-editor textarea").get(0)).focus();
                }
            });

            if (hotkeys.get(openChaptersPane) !== false) {
                hotkeys.del(openChaptersPane);
            }
            hotkeys.add(openChaptersPane, $i18next.t('OpenSection'), function (event) {
                event.preventDefault();
                $scope.$broadcast("openChaptersList", "");
            });

            hotkeys.add({
                combo: 'ctrl+h',
                allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                description: $i18next.t('FindReplaceTitle'),
                callback: function (event) {
                    event.preventDefault();
                    $scope.showFindReplaceDialog();
                }
            });

            $scope.isCommentsBlockVisible = false;
            $scope.toggleComments = function (item) {
                $scope.isCommentsBlockVisible = !$scope.isCommentsBlockVisible;
                if ($scope.isCommentsBlockVisible) {
                    $rootScope.$broadcast("commentsOpened", {});
                }
            };

            $rootScope.$on('openCommentEditorRequested', function (event, data) {
                if ($scope.isCommentsBlockVisible === true)
                    return;
                $scope.isCommentsBlockVisible = true;
                $rootScope.$broadcast("commentsOpened", {});
            });


            var searchBoxOpened = false;
            $scope.showFindReplaceDialog = function() {
                if (!searchBoxOpened) {
                    var modalInstance = $uibModal.open({
                        templateUrl: 'views/find-replace.html',
                        backdrop: false,
                        windowClass: "findReplaceModal dragAndDrop",
                        controller: 'findReplaceCtrl',
                        resolve: {
                            isReadOnlyForUser: $scope.questionnaire.isReadOnlyForUser || false
                        }
                    });
                    searchBoxOpened = true;
                    modalInstance.closed.then(function() {
                        searchBoxOpened = false;
                    });
                }
            };

            $scope.questionnaireId = $state.params.questionnaireId;
            var ERROR = "error";
            var WARNING = "warning";

            $scope.verify = function () {
                $scope.verificationStatus.errors = null;
                $scope.verificationStatus.warnings = null;
                $rootScope.$broadcast("verifing", {});

                setTimeout(function() {
                    verificationService.verify($state.params.questionnaireId).then(function(result) {
                        var data = result.data;
                        $scope.verificationStatus.errors = data.errors;
                        $scope.verificationStatus.warnings = data.warnings;
                        $scope.verificationStatus.time = new Date();
                        $scope.verificationStatus.typeOfMessageToBeShown = ERROR;

                        if ($scope.verificationStatus.errors.length > 0) {
                            $scope.showVerificationErrors();
                        }
                        else {
                            $scope.closeVerifications();
                        }
                    });
                }, 500);
            };

            $scope.webTest = function () {
                var webTesterWindow = window.open("about:blank", '_blank');

                webTesterService.run($state.params.questionnaireId).then(function (response) {
                    webTesterWindow.location.href = response.data;
                });
            };

            $scope.showVerificationErrors = function () {
                if ($scope.verificationStatus.errors.length === 0)
                    return;
                $scope.verificationStatus.typeOfMessageToBeShown = ERROR;
                $scope.verificationStatus.messagesToShow = $scope.verificationStatus.errors;
                $scope.verificationStatus.visible = true;
            }
            $scope.showVerificationWarnings = function () {
                if ($scope.verificationStatus.warnings.length === 0)
                    return;
                $scope.verificationStatus.typeOfMessageToBeShown = WARNING;
                $scope.verificationStatus.messagesToShow = $scope.verificationStatus.warnings;
                $scope.verificationStatus.visible = true;
            }

            $scope.closeVerifications = function() {
                $scope.verificationStatus.visible = false;
            };

            var referancesWithMessagesArray = [];
            $scope.getReferancesWithMessages = function (messagesToShow) {
                for (var indexReferencesWithErrors in messagesToShow.errors) {
                    var referencesWithErrors = messagesToShow.errors[indexReferencesWithErrors];
                    for (var indexReference in referencesWithErrors.references) {
                        var reference = referencesWithErrors.references[indexReference];
                        referancesWithMessagesArray.push({
                            reference: reference, 
                            compilationErrorMessages: referencesWithErrors.compilationErrorMessages,
                        });
                    }
                }
                return referancesWithMessagesArray;
            };

            $scope.toggleCheatSheet = function () {
                hotkeys.toggleCheatSheet();
            };

            $scope.answerTypeClass = {
                YesNo: 'icon-singleoption',
                DropDownList: 'icon-singleoption',
                MultyOption: 'icon-multyoption',
                Numeric: 'icon-numeric',
                DateTime: 'icon-datetime',
                GpsCoordinates: 'icon-gpscoordinates',
                AutoPropagate: 'icon-textedit',
                TextList: 'icon-textlist',
                QRBarcode: 'icon-qrbarcode',
                Text: 'icon-text',
                SingleOption: 'icon-singleoption',
                Multimedia: 'icon-photo',
                Area: 'icon-area',
                Audio: 'icon-audio'
            };

            $scope.items = [];

            $scope.item = null;

            $scope.questionnaire = null;

            $scope.chaptersTree = {
                accept: function (sourceNodeScope) {
                    return _.isEmpty(sourceNodeScope.item);
                },
                dragStart: function () {
                    $scope.chaptersTree.isDragging = true;
                },
                beforeDrop: function (event) {
                    $scope.chaptersTree.isDragging = false;
                },
                dropped: function (event) {
                    var rollback = function (item, targetIndex) {
                        $scope.questionnaire.chapters.splice(_.indexOf($scope.questionnaire.chapters, item), 1);
                        $scope.questionnaire.chapters.splice(targetIndex, 0, item);
                    };

                    if (event.dest.index !== event.source.index) {
                        var group = event.source.nodeScope.chapter;
                        questionnaireService.moveGroup(group.itemId, event.dest.index, null, $state.params.questionnaireId)
                            .error(function () {
                                rollback(group, event.source.index);
                            });
                    }
                }
            };

            $rootScope.navigateTo = function (reference, reload) {
                if (reference.type.toLowerCase() === "questionnaire") {
                    $scope.verificationStatus.visible = false;
                    $scope.showShareInfo();
                } else if (reference.type.toLowerCase() === "macro") {
                    $scope.verificationStatus.visible = false;
                    $rootScope.$broadcast("openMacrosList", { focusOn: reference.itemId });
                } else if (reference.type.toLowerCase() === "lookuptable") {
                    $scope.verificationStatus.visible = false;
                    $rootScope.$broadcast("openLookupTables", { focusOn: reference.itemId });
                } else if (reference.type.toLowerCase() === "attachment") {
                    $scope.verificationStatus.visible = false;
                    $rootScope.$broadcast("openAttachments", { focusOn: reference.itemId });
                } else if (reference.type.toLowerCase() === "translation") {
                    $scope.verificationStatus.visible = false;
                    $rootScope.$broadcast("openTranslations", { focusOn: reference.itemId });
                } else {
                    $state.go('questionnaire.chapter.' + reference.type.toLowerCase(), {
                        chapterId: reference.chapterId,
                        itemId: reference.itemId,
                        indexOfEntityInProperty: reference.indexOfEntityInProperty,
                        property: reference.property
                    },
                        {reload : reload});
                }
            };
            
            $rootScope.$on('$stateChangeSuccess', function (event, toState, toParams) {
                utilityService.scrollToValidationCondition(toParams.indexOfEntityInProperty);
            });

            $scope.removeItemWithIdFromErrors = function (itemId) {
                var errors = $scope.verificationStatus.errors;

                $scope.verificationStatus.errors = _.filter(errors, function (item) {
                    return item.ItemId != itemId;
                });
                _.each(errors, function (error) {
                    if (error.isGroupedMessage) {
                        error.references = _.filter(error.references, function (reference) {
                            return reference.itemId != itemId;
                        });
                    }
                });

                var errorsCount = 0;
                _.each(errors, function (error) {
                    if (error.isGroupedMessage) {
                        _.each(error.references, function (reference) {
                            errorsCount++;
                        });
                    } else {
                        errorsCount++;
                    }
                });

                errors = _.filter(errors, function (error) {
                    return !error.isGroupedMessage || error.references.length;
                });

                $scope.verificationStatus.errors = errors;
                $scope.verificationStatus.errorsCount = errorsCount;
            };

            $scope.currentChapter = null;
            $rootScope.variableNames = [];

            $rootScope.updateVariableNames = function (variables) {
                $rootScope.variableNames = variables;

                $rootScope.$broadcast("variablesChanged", {});
            };

            $rootScope.updateSelfVariable = function(type) {
                var selfVar = _.find($scope.variableNames, { name: "self" });
                selfVar.type = type;
            }

            $rootScope.addOrUpdateLocalVariable = function (variableId, variableName, type) {
                var current = _.find($scope.variableNames, { id: variableId });
                if (!_.isUndefined(current)) {
                    current.name = variableName;
                    current.type = type;
                } else {
                    $scope.variableNames.push({ id: variableId, name: variableName, type: type });
                }
                
                $rootScope.$broadcast("variablesChanged", {});
            };
            
            $rootScope.removeLocalVariable = function (variableId) {
                $scope.variableNames.splice(_.indexOf($scope.variableNames, _.findWhere($scope.variableNames, { id: variableId })), 1);
               
                $rootScope.$broadcast("variablesChanged", {});
            };

            function isNotLinkedOrLinkedToTextList(question) {
                var notLinked = question.linkedToType == null && question.linkedToEntity == null;
                var linkedToTl = question.linkedToType === "textlist" ||
                    (question.linkedToEntity || {}).type === "textlist";

                return notLinked || linkedToTl;
            }

            $rootScope.updateVariableTypes = function(question) {
                var type = $rootScope.getUnderlyingVariableType(question);
                $rootScope.updateSelfVariable(type);
                $rootScope.addOrUpdateLocalVariable(question.itemId, question.variable, type);
            }

            $rootScope.getUnderlyingVariableType = function(question) {
                switch (question.type) {
                    case "Text":
                    case "Multimedia":
                    case "QRBarcode":
                        return "string";
                    case "TextList":
                        return "TextList"; // real type is TextListAnswerRow[]
                    case "DateTime":
                        return "DateTime?";
                    case "GpsCoordinates":
                        return "GeoLocation";
                    case 'MultyOption':
                        if (question.yesNoView) {
                            return "YesNoAnswers"; // real type is YesNoAndAnswersMissings
                        }
                        if (isNotLinkedOrLinkedToTextList(question)) {
                            return "int[]";
                        }
                        return "RosterVector[]";
                    case "SingleOption":
                        if (isNotLinkedOrLinkedToTextList(question)) {
                            return "int?";
                        }

                        return "RosterVector";
                    case "Numeric":
                        if (question.isInteger) {
                            return "int?";
                        }

                        return "double?";
                }

                return null;
            }

            $rootScope.$on('groupDeleted', function (scope, removedItemId) {
                $scope.questionnaire.groupsCount--;
                $scope.removeItemWithIdFromErrors(removedItemId);
            });

            $rootScope.$on('questionDeleted', function (scope, removedItemId) {
                $scope.questionnaire.questionsCount--;
                $scope.removeItemWithIdFromErrors(removedItemId);

                $rootScope.removeLocalVariable(removedItemId);
            });

            $rootScope.$on('variableDeleted', function (scope, removedItemId) {
                $scope.removeItemWithIdFromErrors(removedItemId);

                $rootScope.removeLocalVariable(removedItemId);
            });

            $rootScope.$on('rosterDeleted', function (scope, removedItemId) {
                $scope.questionnaire.rostersCount--;
                $scope.removeItemWithIdFromErrors(removedItemId);
                $rootScope.removeLocalVariable(removedItemId);
            });

            $rootScope.$on('groupAdded', function () {
                $scope.questionnaire.groupsCount++;
            });

            $rootScope.$on('questionAdded', function () {
                $scope.questionnaire.questionsCount++;
            });

            $rootScope.$on('rosterAdded', function () {
                $scope.questionnaire.rostersCount++;
            });

            $rootScope.$on('chapterCloned', function () {
                getQuestionnaire();
            });

            $rootScope.$on('chapterDeleted', function () {
                getQuestionnaire();
            });

            $rootScope.$on('statictextAdded', function () {
            });

            $rootScope.$on('chapterPasted', function () {
                getQuestionnaire();
            });

            $rootScope.$on('itemPasted', function () {
                getQuestionnaire();
            });

            $scope.getPersonsSharedWith = function (questionnaire) {
                if (!questionnaire)
                    return [];

                return _.without(questionnaire.sharedPersons, _.findWhere(questionnaire.sharedPersons, { isOwner: true }));
            };

            $scope.showShareInfo = function () {
                $uibModal.open({
                    templateUrl: 'views/share.html',
                    controller: 'shareCtrl',
                    windowClass: 'share-window',
                    resolve:
                    {
                        questionnaire: function () {
                            return $scope.questionnaire;
                        }
                    }
                });
            };

            var setCommonAceOptions = function (editor) {
                // Editor part
                var renderer = editor.renderer;

                renderer.setShowGutter(false);
                renderer.setPadding(12);

                editor.$blockScrolling = Infinity;
                editor.commands.bindKey("tab", null);
                editor.commands.bindKey("shift+tab", null);

                editor.on('focus', function () {
                    $('.ace_focus').parents('.pseudo-form-control').addClass('focused');
                });

                editor.on('blur', function () {
                    $('.pseudo-form-control.focused').removeClass('focused');
                });
            }

            $scope.setupAceForSubstitutions = function (editor) {
                editor.setOptions({
                    maxLines: 30,
                    fontSize: 16,
                    highlightActiveLine: false,
                    indentedSoftWrap: false,
                    showLineNumbers: false,
                    theme: "ace/theme/github-extended"
                });

                var textExtendableMode = window.ace.require("ace/mode/text-extended").Mode;
                editor.getSession().setMode(new textExtendableMode());
                editor.getSession().setUseWrapMode(true);
                setCommonAceOptions(editor);
            };

            $scope.aceLoadedForOtionFilter = $scope.aceLoaded = function (editor) {
                $scope.aceLoaded(editor, "linkedOptionsFilter");
            }
            $scope.aceLoaded = function (editor, editorType) {
                editorType = editorType || 'regular';
                editor.setOptions({
                    maxLines: Infinity,
                    fontSize: 16,
                    highlightActiveLine: false,
                    theme: "ace/theme/github-extended",
                    enableBasicAutocompletion: true,
                    enableLiveAutocompletion: true
                });
                $scope.aceEditorUpdateMode(editor, editorType);
                
                $rootScope.$on('variablesChanged', function () {
                    $scope.aceEditorUpdateMode(editor, editorType);
                });

                setCommonAceOptions(editor);
            };

            $scope.getVariablesNames = function () {
                return _($rootScope.variableNames);
            }

            $scope.aceEditorUpdateMode = function (editor, editorType) {
                if (editor) {
                    var CSharpExtendableMode = window.ace.require("ace/mode/csharp-extended").Mode;
                    editor.getSession().setMode(new CSharpExtendableMode(function () {
                        return _.pluck($rootScope.variableNames, "name");
                    }));

                    var variablesCompletor =
                    {
                        getCompletions: function(editor, session, pos, prefix, callback) {
                            var i = 0;

                            var variables = $scope.getVariablesNames()
                                .sortBy('name')
                                .reverse()
                                .map(function(variable) {
                                    return {
                                        name: variable.name,
                                        value: variable.name,
                                        score: i++,
                                        meta: variable.type
                                    }
                                });

                            if (editorType === "linkedOptionsFilter") {
                                variables.push({
                                    name: "@current",
                                    value: "@current",
                                    score: i++,
                                    meta: "Roster"
                                });
                            }

                            callback(null, variables);
                        },

                        identifierRegexps : [/[@a-zA-Z_0-9\$\-\u00A2-\uFFFF]/]
                    };

                    var lang_tool = ace.require("ace/ext/language_tools");
                    lang_tool.setCompleters([variablesCompletor]);
                }
            }
           
            $rootScope.$on('$stateChangeSuccess',
                function (event, toState, toParams) {
                    var target = toState.name.replace('questionnaire.chapter.', '');
                    if (target === "question" || target === "group" || target === "roster" || target === "statictext") {
                        var itemId = "#" + toParams.itemId;
                        $scope.$broadcast("scrollToElement", itemId);
                    }
                });

            var getQuestionnaire = function () {
                questionnaireService.getQuestionnaireById($state.params.questionnaireId).then(function (result) {
                    $scope.questionnaire = result.data;
                    if (!$state.params.chapterId && result.data.chapters.length > 0) {
                        var defaultChapter = _.first(result.data.chapters);
                        var itemId = defaultChapter.itemId;
                        $scope.currentChapter = defaultChapter;
                        $state.go('questionnaire.chapter.group', { chapterId: itemId, itemId: itemId });
                    }

                    $rootScope.$emit('questionnaireLoaded');
                });
            };

            userService.getCurrentUserName().then(function(result) {
                $scope.currentUserName = result.data.userName;
                $scope.currentUserEmail = result.data.email;
            });

            getQuestionnaire();
        }
    );
