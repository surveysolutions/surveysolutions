angular.module('designerApp')
    .controller('LeftMenuCtrl',
        function ($rootScope, $scope) {
            'use strict';

            $scope.isFoldedChapters = false;
            $scope.isFoldedMacros = false;

            var closeOpenPanelIfAny = function() {
                if (!($scope.isFoldedChapters || $scope.isFoldedMacros))
                    return;

                if ($scope.isFoldedChapters) {
                    $rootScope.$broadcast("closeChaptersListRequested", {});
                }

                if ($scope.isFoldedMacros) {
                    $rootScope.$broadcast("closeMacrosListRequested", {});
                }
            };

            $scope.unfoldChapters = function () {
                if ($scope.isFoldedChapters)
                    return;
                closeOpenPanelIfAny();
                $scope.isFoldedChapters = true;
                $rootScope.$broadcast("openChaptersList", {});
            };

            $scope.unfoldMacros = function () {
                if ($scope.isFoldedMacros)
                    return;

                closeOpenPanelIfAny();
                $scope.isFoldedMacros = true;
                $rootScope.$broadcast("openMacrosList", {});
            };

            $scope.$on('closeChaptersList', function () {
                $scope.isFoldedChapters = false;
            });

            $scope.$on('closeMacrosList', function () {
                $scope.isFoldedMacros = false;
            });

        });
