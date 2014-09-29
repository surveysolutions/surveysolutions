angular.module('designerApp')
    .controller('GroupCtrl',
        function ($rootScope, $scope, $stateParams, questionnaireService, commandService) {
            $scope.currentChapterId = $stateParams.chapterId;
            var dataBind = function (group) {
                $scope.activeGroup = group;

                $scope.activeGroup.isChapter = ($stateParams.itemId == $stateParams.chapterId);
                $scope.activeGroup.itemId = $stateParams.itemId;
                $scope.activeGroup.variableName = $stateParams.variableName;

                $scope.activeGroup.isFirstChapter = false;
                if ($scope.activeGroup.isChapter) {
                    if ($scope.questionnaire && $scope.questionnaire.chapters && $scope.questionnaire.chapters.length)
                        $scope.activeGroup.isFirstChapter = $stateParams.itemId == $scope.questionnaire.chapters[0].itemId;
                }

                $scope.groupForm.$setPristine();
            };

            $scope.loadGroup = function () {
                questionnaireService.getGroupDetailsById($stateParams.questionnaireId, $stateParams.itemId).success(function (result) {

                    dataBind(result.group);
                    $scope.activeGroup.breadcrumbs = result.breadcrumbs;
                    $scope.initialGroup = angular.copy($scope.activeGroup);
                }
                );
            };

            $scope.saveGroup = function () {
                if ($scope.groupForm.$valid) {
                    commandService.updateGroup($stateParams.questionnaireId, $scope.activeGroup).success(function (result) {
                        $scope.initialGroup = angular.copy($scope.activeGroup);
                        $rootScope.$emit('groupUpdated', {
                            itemId: $scope.activeGroup.itemId,
                            title: $scope.activeGroup.title
                        });
                        $scope.groupForm.$setPristine();
                    });
                }
            };

            $scope.cancelGroup = function () {
                var temp = angular.copy($scope.initialGroup);
                dataBind(temp);
            };

            $scope.deleteItem = function () {
                if ($scope.activeGroup.isChapter) {
                    $rootScope.$emit('deleteChapter', {
                        chapter: $scope.activeGroup
                    });
                } else {
                    $scope.deleteGroup($scope.activeGroup);
                }
            };

            if ($scope.questionnaire) {
                $scope.loadGroup();
            } else {
                $rootScope.$on('questionnaireLoaded', function () {
                    $scope.loadGroup();

                });
            }

        }
    );
