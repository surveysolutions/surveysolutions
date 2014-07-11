(function() {
    'use strict';

    angular.module('designerApp')
        .controller('StaticTextCtrl', [
            '$scope', '$stateParams', 'questionnaireId', 'questionnaireService', 'commandService', '$log',
            function ($scope, $stateParams, questionaireId, questionnaireService, commandService, $log) {

                var dataBind = function (result) {
                    $scope.activeStaticText = $scope.activeStaticText || {};
                    $scope.activeStaticText.breadcrumbs = result.breadcrumbs;
                    $scope.activeStaticText.text = result.text;
                };

                $scope.loadStaticText = function() {
                    questionnaireService.getStaticTextDetailsById(questionaireId, $stateParams.itemId)
                        .success(function(result) {
                            $scope.initialStaticText = angular.copy(result);
                            dataBind(result);
                        });
                };

                $scope.saveStaticText = function() {
                    commandService.sendStaticTextCommand(questionaireId, $scope.activeStaticText).success(function (result) {
                        if (!result.IsSuccess) {
                            $log.error(result.Error);
                        }
                    });
                };
            
                $scope.moveToChapter = function(chapterId) {
                    questionnaireService.moveStaticText($scope.activeStaticText.itemId, 0, chapterId, questionaireId);
                    
                    var removeFrom = $scope.activeStaticText.getParentItem() || $scope;
                    removeFrom.items.splice(_.indexOf(removeFrom.items, $scope.activeStaticText), 1);
                    $scope.resetSelection();
                };

                $scope.resetStaticText = function() {
                    dataBind($scope.initialStaticText);
                };

                $scope.loadStaticText();

                $scope.$watch('activeStaticText', function () {
                    $scope.loadStaticText();
                });
            }
        ]);
}());