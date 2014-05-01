'use strict';

angular.module('designerApp')
    .controller('ChapterCtrl', [
        '$scope', '$routeParams', 'questionnaireService', 'commandService',
        function($scope, $routeParams, questionnaireService, commandService) {

            $scope.loadGroup = function() {
                questionnaireService.getGroupDetailsById($routeParams.questionnaireId, $scope.activeChapter.itemId).success(function (result) {
                        var group = result.group;
                        $scope.activeChapter.description = group.description;
                        $scope.activeChapter.enablementCondition = group.enablementCondition;
                    }
                );
            }

            console.log($scope.activeChapter);
            $scope.loadGroup();

            $scope.$watch('activeChapter', function(newVal) {
                $scope.loadGroup();
            });

            $scope.saveChapter = function() {
                console.log($scope.activeChapter);
                commandService.updateGroup($routeParams.questionnaireId, $scope.activeChapter);
            };
        }
    ]);