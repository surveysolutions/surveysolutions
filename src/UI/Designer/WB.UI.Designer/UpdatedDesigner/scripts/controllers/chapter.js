'use strict';

angular.module('designerApp')
    .controller('ChapterCtrl', [
        '$scope', '$routeParams', 'questionnaireService', 'commandService',
        function($scope, $routeParams, questionnaireService, commandService) {

            $scope.loadGroup = function() {
                questionnaireService.getGroupDetailsById($routeParams.questionnaireId, $scope.activeChapter.itemId).success(function(result) {
                        var group = result.group;
                        $scope.activeChapter.description = group.description;
                        $scope.activeChapter.enablementCondition = group.enablementCondition;
                        $scope.activeChapter.breadcrumbs = result.breadcrumbs;
                    }
                );
            }

            $scope.loadGroup();

            $scope.$watch('activeChapter', function(newVal) {
                $scope.loadGroup();
            });

            $scope.saveChapter = function() {
                $("#edit-chapter-save-button").popover('destroy');
                commandService.updateGroup($routeParams.questionnaireId, $scope.activeChapter).success(function(result) {
                    console.log(result);
                    if (!result.IsSuccess) {
                        $("#edit-chapter-save-button").popover({
                            content: result.Error,
                            placement: top,
                            animation: true
                        }).popover('show');
                    }
                });
            };
        }
    ]);