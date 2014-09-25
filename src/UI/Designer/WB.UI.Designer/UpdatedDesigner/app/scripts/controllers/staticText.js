(function () {
    'use strict';

    angular.module('designerApp')
        .controller('StaticTextCtrl', [
            '$rootScope', '$scope', '$state', 'questionnaireService', 'commandService',
            function ($rootScope, $scope, $state, questionnaireService, commandService) {

                var dataBind = function (result) {
                    $scope.activeStaticText = $scope.activeStaticText || {};
                    $scope.activeStaticText.breadcrumbs = result.breadcrumbs;
                    $scope.activeStaticText.itemId = $state.params.itemId;
                    $scope.activeStaticText.text = result.text;

                    $scope.staticTextForm.$setPristine();
                };

                $scope.loadStaticText = function () {
                    questionnaireService.getStaticTextDetailsById($state.params.questionnaireId, $state.params.itemId)
                        .success(function (result) {
                            $scope.initialStaticText = angular.copy(result);
                            dataBind(result);
                        });
                };

                $scope.saveStaticText = function () {
                    commandService.updateStaticText($state.params.questionnaireId, $scope.activeStaticText).success(function () {
                        $scope.initialStaticText = angular.copy($scope.activeStaticText);
                        $rootScope.$emit('staticTextUpdated', {
                            itemId: $scope.activeStaticText.itemId,
                            text: $scope.activeStaticText.text
                        });
                        $scope.staticTextForm.$setPristine();
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