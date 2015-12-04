angular.module('designerApp')
    .controller('LeftMenuCtrl',
        function ($rootScope, $scope) {
            'use strict';

            $scope.isFoldedChapters = false;
            $scope.isFoldedMacros = false;
            $scope.isFoldedLookupTables = false;

            var closeOpenPanelIfAny = function() {
                if (!($scope.isFoldedChapters || $scope.isFoldedMacros || $scope.isFoldedLookupTables))
                    return;

                if ($scope.isFoldedChapters) {
                    $rootScope.$broadcast("closeChaptersListRequested", {});
                }

                if ($scope.isFoldedMacros) {
                    $rootScope.$broadcast("closeMacrosListRequested", {});
                }
                if ($scope.isFoldedLookupTables) {
                    $rootScope.$broadcast("closeLookupTablesRequested", {});
                }
            };

            var closeAllPanel = function () {
                $scope.isFoldedMacros = false;
                $scope.isFoldedChapters = false;
                $scope.isFoldedLookupTables = false;
            }

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

            $scope.unfoldLookupTables = function () {
                if ($scope.isFoldedLookupTables)
                    return;

                closeOpenPanelIfAny();
                $scope.isFoldedLookupTables = true;
                $rootScope.$broadcast("openLookupTables", {});
            };

            $scope.$on('closeChaptersList', function () {
                closeAllPanel();
            });

            $scope.$on('closeMacrosList', function () {
                closeAllPanel();
            });

            $scope.$on('closeLookupTables', function () {
                closeAllPanel();
            });
        });