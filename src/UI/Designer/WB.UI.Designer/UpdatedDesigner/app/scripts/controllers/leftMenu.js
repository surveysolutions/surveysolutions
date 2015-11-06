angular.module('designerApp')
    .controller('LeftMenuCtrl',
        function ($rootScope, $scope) {
            'use strict';

            $scope.isFoldedChapters = false;
            $scope.isFoldedMacroses = false;

            var closeOpenPanelIfAny = function () {
                if (!($scope.isFoldedChapters || $scope.isFoldedMacroses))
                    return;

                if ($scope.isFoldedChapters) {
                    $rootScope.$broadcast("closeChaptersListRequested", {});
                }

                if ($scope.isFoldedMacroses) {
                    $rootScope.$broadcast("closeMacrosesListRequested", {});
                }
            }

            $scope.unfoldChapters = function () {
                if ($scope.isFoldedChapters)
                    return;
                closeOpenPanelIfAny();
                $scope.isFoldedChapters = true;
                $rootScope.$broadcast("openChaptersList", {});
            };

            $scope.unfoldMacroses = function () {
                if ($scope.isFoldedMacroses)
                    return;

                closeOpenPanelIfAny();
                $scope.isFoldedMacroses = true;
                $rootScope.$broadcast("openMacrosesList", {});
            };

            $scope.$on('closeChaptersList', function () {
                $scope.isFoldedChapters = false;
            });

            $scope.$on('closeMacrosesList', function () {
                $scope.isFoldedMacroses = false;
            });

        });
