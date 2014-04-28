'use strict';

angular.module('designerApp')
    .controller('MainCtrl', function ($scope, $routeParams, $location, $route, questionnaireService, commandService, verificationService, utilityService) {

        $scope.verificationStatus = {
            errorsCount: 0,
            errors: []
        };

        $scope.verify = function() {
            verificationService.verify($routeParams.questionnaireId).success(function(result) {
                $scope.verificationStatus.errors = result.errors;
                $scope.verificationStatus.errorsCount = result.errors.length;

                if ($scope.verificationStatus.errorsCount > 0) {
                    $('#verification-modal').modal({
                        backdrop: false,
                        show: true
                    });
                }
            });
        };
        $scope.chapters = [];

        $scope.items = [];

        $scope.item = null;

        $scope.questionnaire = null;

        $scope.currentChapter = null;

        $scope.currentChapterId = null;

        $scope.setItem = function (item) {
            var url = utilityService.format('/{0}/chapter/{1}/item/{2}', $routeParams.questionnaireId, $scope.currentChapterId, item.Id);
            $location.path(url);
            $scope.currentItemId = item.Id;
        };

        $scope.changeChapter = function(chapter) {
            var url = utilityService.format('/{0}/chapter/{1}', $routeParams.questionnaireId, chapter.ChapterId);
            $location.path(url);
            $scope.currentChapterId = chapter.ChapterId;
            $scope.loadChapterDetails($routeParams.questionnaireId, $scope.currentChapterId);
        };

        $scope.loadChapterDetails = function(questionnaireId, chapterId) {
            questionnaireService.getChapterById(questionnaireId, chapterId)
                .success(function(result) {
                    $scope.items = result.Items;
                    $scope.currentChapter = result;
                    window.ContextMenuController.get().init();
                });
        };

        $scope.isQuestion = function(item) {
            return item.hasOwnProperty('Type');
        };

        $scope.addQuestion = function(item) {
            var newId = utilityService.guid();
            var self = item;
            commandService.addQuestion($routeParams.questionnaireId, item, newId).success(function() {
                    self.Items.push(
                        {
                            "Id": newId,
                            "Title": "New Question",
                            "Variable": "",
                            "Type": 7,
                            "LinkedVariables": [],
                            "BrokenLinkedVariables": null
                        }
                    );
                }
            );
        };

        $scope.addGroup = function(item) {
            var newId = utilityService.guid();
            var newGroup = {
                "Id": newId,
                "Title": "New group",
                "QuestionsCount": 0,
                "GroupsCount": 0,
                "RostersCount": 0,
                "Items": []
            };

            commandService.addGroup($routeParams.questionnaireId, newGroup, item.Id).success(function() {
                item.Items.push(newGroup);
            });
        };

        $scope.collapse = function(item) {
            item.collapsed = true;
        };

        $scope.expand = function(item) {
            item.collapsed = false;
        };

        $scope.submit = function() {
            console.log('submit');
        };

        questionnaireService.getQuestionnaireById($routeParams.questionnaireId)
            .success(function(result) {
                if (result == 'null') {
                    alert('Questionnaire not found');
                } else {
                    $scope.questionnaire = result;

                    if ($routeParams.chapterId) {
                        $scope.currentChapterId = $routeParams.chapterId;
                        $scope.loadChapterDetails($routeParams.questionnaireId, $scope.currentChapterId);
                    } else {
                        $scope.currentChapter = result.Chapters[0];
                        $scope.currentChapterId = $scope.currentChapter.ChapterId;
                        $scope.loadChapterDetails($routeParams.questionnaireId, $scope.currentChapter.ChapterId);
                    }
                    if ($routeParams.itemId) {
                        $scope.currentItemId = $routeParams.itemId;
                    }
                }
            });

        //do not reload views, change url only
        var lastRoute = $route.current;
        $scope.$on('$locationChangeSuccess', function() {
            $route.current = lastRoute;
        }); 
    });