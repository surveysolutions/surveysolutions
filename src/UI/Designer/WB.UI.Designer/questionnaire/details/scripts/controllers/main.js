angular.module('designerApp')
    .controller('MainCtrl',
        function ($rootScope, $scope, $state, questionnaireService, commandService, verificationService, utilityService, hotkeys, $uibModal, notificationService, userService) {

            $(document).on('click', "a[href='javascript:void(0);']", function (e) { e.preventDefault(); }); // remove when we will stop support of IE 9 KP-6076

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

                    var printWindow = window.open("../../questionnaire/expressiongeneration/" + $state.params.questionnaireId);
                    event.preventDefault();
                }
            });

            hotkeys.add({
                combo: 'ctrl+p',
                allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                description: 'Print',
                callback: function (event) {
                    
                    var printWindow = window.open("../../pdf/printpreview/" + $state.params.questionnaireId);
                    try{
                        printWindow.focus();
                    } catch(e) {
                        notificationService.notice(e);//"Make sure popups are not blocked");
                    }
                    //printWindow.print();
                    event.preventDefault();
                }
            });
            
            hotkeys.add({
                combo: 'ctrl+b',
                allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                description: 'Compile',
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
                description: 'Focus questionnaire tree',
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
                description: 'Focus title field in editor',
                callback: function (event) {
                    event.preventDefault();
                    $($(".question-editor textarea").get(0)).focus();
                }
            });

            if (hotkeys.get(openChaptersPane) !== false) {
                hotkeys.del(openChaptersPane);
            }
            hotkeys.add(openChaptersPane, 'Open section', function (event) {
                event.preventDefault();
                $scope.$broadcast("openChaptersList", "");
            });

            hotkeys.add({
                combo: 'ctrl+h',
                allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                description: 'Find and replace',
                callback: function (event) {
                    event.preventDefault();
                    $scope.showFindReplaceDialog();
                }
            });
            var searchBoxOpened = false;
            $scope.showFindReplaceDialog = function() {
                if (!searchBoxOpened) {
                    var modalInstance = $uibModal.open({
                        templateUrl: 'views/find-replace.html',
                        backdrop: false,
                        windowClass: "findReplaceModal",
                        controller: 'findReplaceCtrl'
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

            $scope.verify = function() {
                $scope.verificationStatus.errors = null;
                $scope.verificationStatus.warnings = null;
                $rootScope.$broadcast("verifing", {});

                setTimeout(function() {
                    verificationService.verify($state.params.questionnaireId).success(function(result) {
                        $scope.verificationStatus.errors = result.errors;
                        $scope.verificationStatus.warnings = result.warnings;
                        $scope.verificationStatus.time = new Date();
                        $scope.verificationStatus.typeOfMessageToBeShown = ERROR;

                        if ($scope.verificationStatus.errors.length > 0)
                            $scope.showVerificationErrors();
                        else {
                            $scope.closeVerifications();
                        }
                    });
                }, 500);
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
                Multimedia: 'icon-photo'
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

            $rootScope.navigateTo = function (reference) {
                if (reference.type.toLowerCase() === "macro") {
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
                    });
                    
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

            $rootScope.addOrUpdateLocalVariable = function (variableId, variableName) {
                var current = _.find($scope.variableNames, { id: variableId });
                if (!_.isUndefined(current)) {
                    current.name = variableName;
                } else {
                    $scope.variableNames.push({ id: variableId, name: variableName });
                }
                
                $rootScope.$broadcast("variablesChanged", {});
            };
            
            $rootScope.removeLocalVariable = function (variableId) {
                $scope.variableNames.splice(_.indexOf($scope.variableNames, _.findWhere($scope.variableNames, { id: variableId })), 1);
               
                $rootScope.$broadcast("variablesChanged", {});
            };

            $rootScope.$on('groupDeleted', function (scope, removedItemId) {
                $scope.questionnaire.groupsCount--;
                $scope.removeItemWithIdFromErrors(removedItemId);
            });

            $rootScope.$on('questionDeleted', function (scope, removedItemId) {
                $scope.questionnaire.questionsCount--;
                $scope.removeItemWithIdFromErrors(removedItemId);

                $rootScope.removeLocalVariable(removedItemId);
            });

            $rootScope.$on('varibleDeleted', function (scope, removedItemId) {
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
            
            $scope.aceLoaded = function (editor) {
                // Editor part
                var renderer = editor.renderer;
                
                // Options
                editor.setOptions({
                    maxLines: Infinity,
                    fontSize: 16,
                    highlightActiveLine: false,
                    theme: "ace/theme/github-extended",
                    enableBasicAutocompletion: true,
                    enableLiveAutocompletion: true
                });
                renderer.setShowGutter(false);
                renderer.setPadding(12);

                $scope.aceEditorUpdateMode(editor);

                editor.$blockScrolling = Infinity;
                editor.commands.bindKey("tab", null);
                editor.commands.bindKey("shift+tab", null);

                $rootScope.$on('variablesChanged', function() {
                    $scope.aceEditorUpdateMode(editor);
                });

                editor.on('focus', function() {
                    $('.ace_focus').parents('.pseudo-form-control').addClass('focused');
                });

                editor.on('blur', function () {
                    $('.pseudo-form-control.focused').removeClass('focused');
                });
            };

            $scope.getVariablesNames = function () {
                return _.pluck($rootScope.variableNames, "name");;
            }

            $scope.aceEditorUpdateMode = function(editor) {
                if (editor) {
                    var CSharpExtendableMode = window.ace.require("ace/mode/csharp-extended").Mode;
                    editor.getSession().setMode(new CSharpExtendableMode($scope.getVariablesNames));

                    var variablesCompletor =
                    {
                        getCompletions: function (editor, session, pos, prefix, callback) {
                            var i = 0;
                            callback(null, $scope.getVariablesNames().sort().reverse().map(function (variable) {
                                    return { name: variable, value: variable, score: i++, meta: "variable" }
                                }));
                        }
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
                questionnaireService.getQuestionnaireById($state.params.questionnaireId).success(function (result) {
                    $scope.questionnaire = result;
                    if (!$state.params.chapterId && result.chapters.length > 0) {
                        var defaultChapter = _.first(result.chapters);
                        var itemId = defaultChapter.itemId;
                        $scope.currentChapter = defaultChapter;
                        $state.go('questionnaire.chapter.group', { chapterId: itemId, itemId: itemId });
                    }

                    $rootScope.$emit('questionnaireLoaded');
                });
            };

            userService.getCurrentUserName().success(function(result) {
                $scope.currentUserName = result;
            });

            getQuestionnaire();
        }
    );
