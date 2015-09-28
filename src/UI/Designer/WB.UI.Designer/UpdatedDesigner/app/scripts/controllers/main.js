angular.module('designerApp')
    .controller('MainCtrl',
        function($rootScope, $scope, $state, questionnaireService, commandService, verificationService, utilityService, hotkeys, $modal) {

            $scope.verificationStatus = {
                errorsCount: null,
                errors: [],
                visible: false,
                time: new Date()
            };

            $scope.questionnaire = {
                questionsCount: 0,
                groupsCount: 0,
                rostersCount: 0
            };
            var focusTreePane = 'shift+alt+x';
            var focusEditorPane = 'shift+alt+e';
            var openChaptersPane = 'left';

            hotkeys.add({
                combo: 'esc',
                allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                callback: function() {
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
                    callback: function(event) {
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

            $scope.questionnaireId = $state.params.questionnaireId;

            $scope.verify = function () {
                $scope.verificationStatus.errors = [];
                verificationService.verify($state.params.questionnaireId).success(function (result) {
                    $scope.verificationStatus.errors = result.errors;
                    $scope.verificationStatus.errorsCount = result.errorsCount;
                    $scope.verificationStatus.time = new Date();
                    $scope.verificationStatus.visible = result.errorsCount > 0;
                });
            };

            $scope.toggleCheatSheet = function() {
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
                dropped: function (event) {
                    $scope.chaptersTree.isDragging = false;
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

            $scope.navigateTo = function (reference) {
                $state.go('questionnaire.chapter.' + reference.type.toLowerCase(), {
                    chapterId: reference.chapterId,
                    itemId: reference.itemId
                });
            };

            $scope.removeItemWithIdFromErrors = function (itemId) {
                var errors = $scope.verificationStatus.errors;

                $scope.verificationStatus.errors = _.filter(errors, function (item) {
                    return item.ItemId != itemId;
                });
                _.each(errors, function(error) {
                    if (error.isGroupOfErrors) {
                        error.references = _.filter(error.references, function (reference) {
                            
                            return reference.itemId != itemId;
                        });
                    }
                })

                var errorsCount = 0;
                _.each(errors, function (error) {
                    if (error.isGroupOfErrors) {
                        _.each(error.references, function(reference) {
                            errorsCount++;
                        });
                    } else {
                        errorsCount++;
                    }
                });
                    
                errors = _.filter(errors, function(error) {
                    return !error.isGroupOfErrors || error.references.length;
                });

                $scope.verificationStatus.errors = errors;
                $scope.verificationStatus.errorsCount = errorsCount;
            }

            $scope.currentChapter = null;

            $rootScope.$on('groupDeleted', function (scope, removedItemId) {
                $scope.questionnaire.groupsCount--;
                $scope.removeItemWithIdFromErrors(removedItemId);
            });

            $rootScope.$on('questionDeleted', function (scope, removedItemId) {
                $scope.questionnaire.questionsCount--;
                $scope.removeItemWithIdFromErrors(removedItemId);
            });

            $rootScope.$on('rosterDeleted', function (scope, removedItemId) {
                $scope.questionnaire.rostersCount--;
                $scope.removeItemWithIdFromErrors(removedItemId);
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

            $scope.getPersonsSharedWith = function(questionnaire) {
                if (!questionnaire)
                    return [];

                return _.without(questionnaire.sharedPersons, _.findWhere(questionnaire.sharedPersons, { isOwner: true }));
            };

            $scope.showShareInfo = function () {
                $modal.open({
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

            $scope.aceLoaded = function(editor){
                // Editor part
                var renderer = editor.renderer;

                // Options
                editor.setOptions({
                    maxLines: Infinity,
                    mode: "ace/mode/csharp",
                    fontSize: 16,
                    highlightActiveLine: false,
                    theme: "ace/theme/github"
                });
                renderer.setShowGutter(false);
                renderer.setPadding(12);
            };

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

            getQuestionnaire();
        }
    );
