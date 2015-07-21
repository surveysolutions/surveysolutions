angular.module('designerApp')
    .controller('StaticTextCtrl',
        function ($rootScope, $scope, $state, questionnaireService, commandService, hotkeys) {
            "use strict";

            var saveStaticText = 'ctrl+s';

            if (hotkeys.get(saveStaticText) === false) {
                hotkeys.del(saveStaticText);
            }
            if ($scope.questionnaire != null && !$scope.questionnaire.isReadOnlyForUser)
            {
                hotkeys.bindTo($scope)
                    .add({
                        combo: saveStaticText,
                        description: 'Save changes',
                        allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                        callback: function(event) {
                            $scope.saveStaticText();
                            $scope.staticTextForm.$setPristine();
                            event.preventDefault();
                        }
                    });
            }
            var dataBind = function (result) {
                $scope.activeStaticText = $scope.activeStaticText || {};
                $scope.activeStaticText.breadcrumbs = result.breadcrumbs;
                $scope.activeStaticText.itemId = $state.params.itemId;
                $scope.activeStaticText.text = result.text;
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
                });
            };

            $scope.cancelStaticText = function () {
                var temp = angular.copy($scope.initialStaticText);
                dataBind(temp);
            };

            $scope.loadStaticText();
        }
    );
