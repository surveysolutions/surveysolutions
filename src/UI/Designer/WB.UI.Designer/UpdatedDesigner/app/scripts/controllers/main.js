angular.module('designerApp')
    .controller('MainCtrl',
        function ($rootScope, $scope, $state, questionnaireService, commandService, verificationService, utilityService, hotkeys, $modal) {

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

            hotkeys.add({
                combo: 'esc',
                allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                callback: function () {
                    $scope.verificationStatus.visible = false;
                }
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

            $scope.currentChapter = null;

            $scope.addQuestion = function (parent) {
                var newId = utilityService.guid();
                var emptyQuestion = {
                    "itemId": newId,
                    "title": 'New Question',
                    "type": 'Text',
                    itemType: 'Question',
                    getParentItem: function () { return parent; }
                };

                commandService.addQuestion($state.params.questionnaireId, parent.itemId, newId).success(function (result) {
                    if (result.IsSuccess) {
                        parent.items.push(emptyQuestion);
                        $state.go('questionnaire.chapter.question', { chapterId: $state.params.chapterId, itemId: newId });
                        $rootScope.$emit('questionAdded');
                    }
                });
            };

          

            $scope.addGroup = function (parent) {
                var newId = utilityService.guid();
                var emptyGroup = {
                    "itemId": newId,
                    "title": "New group",
                    "items": [],
                    itemType: 'Group',
                    getParentItem: function () { return parent; }
                };
                commandService.addGroup($state.params.questionnaireId, emptyGroup, parent.itemId).success(function () {
                    parent.items.push(emptyGroup);
                    $rootScope.$emit('groupAdded');
                    $state.go('questionnaire.chapter.group', { chapterId: $state.params.chapterId, itemId: newId });
                });
            };

            $scope.addRoster = function (parent) {
                var newId = utilityService.guid();
                var emptyRoster = {
                    "itemId": newId,
                    "title": "New roster",
                    "items": [],
                    itemType: 'Group',
                    isRoster: true,
                    getParentItem: function () { return parent; }
                };

                commandService.addRoster($state.params.questionnaireId, emptyRoster, parent.itemId).success(function () {
                    parent.items.push(emptyRoster);
                    $rootScope.$emit('rosterAdded');
                    $state.go('questionnaire.chapter.roster', { chapterId: $state.params.chapterId, itemId: newId });
                });
            };

            $scope.addStaticText = function (parent) {
                var newId = utilityService.guid();
                var emptyStaticText = {
                    "itemId": newId,
                    "text": "New static text",
                    itemType: 'StaticText',
                    getParentItem: function () { return parent; }
                };

                commandService.addStaticText($state.params.questionnaireId, emptyStaticText, parent.itemId).success(function () {
                    parent.items.push(emptyStaticText);
                    $state.go('questionnaire.chapter.statictext', { chapterId: $state.params.chapterId, itemId: newId });
                });
            };

            $rootScope.$on('groupDeleted', function () {
                $scope.questionnaire.groupsCount--;
            });

            $rootScope.$on('questionDeleted', function () {
                $scope.questionnaire.questionsCount--;
            });

            $rootScope.$on('rosterDeleted', function () {
                $scope.questionnaire.rostersCount--;
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
