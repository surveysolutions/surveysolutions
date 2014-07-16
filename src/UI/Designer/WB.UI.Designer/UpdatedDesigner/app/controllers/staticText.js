(function() {
    'use strict';

    angular.module('designerApp')
        .controller('StaticTextCtrl', [
            '$scope', '$state', 'questionnaireId', 'questionnaireService', 'commandService', '$log',
            function ($scope, $state, questionaireId, questionnaireService, commandService, $log) {

                var dataBind = function (result) {
                    $scope.activeStaticText = $scope.activeStaticText || {};
                    $scope.activeStaticText.breadcrumbs = result.breadcrumbs;
                    $scope.activeStaticText.itemId = $state.params.itemId;
                    $scope.activeStaticText.text = result.text;
                };

                $scope.loadStaticText = function() {
                    questionnaireService.getStaticTextDetailsById(questionaireId, $state.params.itemId)
                        .success(function(result) {
                            $scope.initialStaticText = angular.copy(result);
                            dataBind(result);
                        });
                };

                $scope.saveStaticText = function() {
                    commandService.updateStaticText(questionaireId, $scope.activeStaticText).success(function (result) {
                        $scope.initialStaticText = angular.copy($scope.activeStaticText);
                    });
                };

                $scope.cancelStaticText = function () {
                    var temp = angular.copy($scope.initialStaticText);
                    dataBind(temp);
                };

                $scope.loadStaticText();    
            }
        ]);
}());