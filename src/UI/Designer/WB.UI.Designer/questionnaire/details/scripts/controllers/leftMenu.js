angular.module('designerApp')
    .controller('LeftMenuCtrl',
        function ($rootScope, $scope) {
            'use strict';

            $scope.isUnfoldedChapters = false;
            $scope.isUnfoldedMacros = false;
            $scope.isUnfoldedLookupTables = false;
            $scope.isUnfoldedAttachments = false;
            $scope.isUnfoldedTranslations = false;


            var closeOpenPanelIfAny = function() {
                if (!($scope.isUnfoldedChapters || $scope.isUnfoldedMacros || $scope.isUnfoldedLookupTables || $scope.isUnfoldedAttachments || $scope.isUnfoldedTranslations))
                    return;

                if ($scope.isUnfoldedChapters) {
                    $rootScope.$broadcast("closeChaptersListRequested", {});
                }
                if ($scope.isUnfoldedMacros) {
                    $rootScope.$broadcast("closeMacrosListRequested", {});
                }
                if ($scope.isUnfoldedLookupTables) {
                    $rootScope.$broadcast("closeLookupTablesRequested", {});
                }
                if ($scope.isUnfoldedAttachments) {
                    $rootScope.$broadcast("closeAttachmentsRequested", {});
                }
                if ($scope.isUnfoldedTranslations) {
                    $rootScope.$broadcast("closeTranslationsRequested", {});
                }

            };

            var closeAllPanel = function () {
                $scope.isUnfoldedMacros = false;
                $scope.isUnfoldedChapters = false;
                $scope.isUnfoldedLookupTables = false;
                $scope.isUnfoldedAttachments = false;
                $scope.isUnfoldedTranslations = false;
            }

            $scope.unfoldChapters = function () {
                if ($scope.isUnfoldedChapters)
                    return;
                closeOpenPanelIfAny();
                $scope.isUnfoldedChapters = true;
                $rootScope.$broadcast("openChaptersList", {});
            };

            $scope.unfoldMacros = function () {
                if ($scope.isUnfoldedMacros)
                    return;

                closeOpenPanelIfAny();
                $scope.isUnfoldedMacros = true;
                $rootScope.$broadcast("openMacrosList", {});
            };

            $scope.unfoldLookupTables = function () {
                if ($scope.isUnfoldedLookupTables)
                    return;

                closeOpenPanelIfAny();
                $scope.isUnfoldedLookupTables = true;
                $rootScope.$broadcast("openLookupTables", {});
            };

            $scope.unfoldAttachments = function () {
                if ($scope.isUnfoldedAttachments)
                    return;

                closeOpenPanelIfAny();
                $scope.isUnfoldedAttachments = true;
                $rootScope.$broadcast("openAttachments", {});
            };

            $scope.unfoldTranslations = function () {
                if ($scope.isUnfoldedTranslations)
                    return;

                closeOpenPanelIfAny();
                $scope.isUnfoldedTranslations = true;
                $rootScope.$broadcast("openTranslations", {});
            };

            $scope.$on('openChaptersList', function () {
                $scope.isUnfoldedChapters = true;
            });

            $scope.$on('openLookupTables', function () {
                $scope.isUnfoldedLookupTables = true;
            });

            $scope.$on('openAttachments', function () {
                $scope.isUnfoldedAttachments = true;
            });

            $scope.$on('openTranslations', function () {
                $scope.isUnfoldedTranslations = true;
            });

            $scope.$on('openMacrosList', function () {
                $scope.isUnfoldedMacros = true;
            });

            $scope.$on('closeChaptersList', function () {
                closeAllPanel();
            });

            $scope.$on('closeMacrosList', function () {
                closeAllPanel();
            });

            $scope.$on('closeLookupTables', function () {
                closeAllPanel();
            });
            
            $scope.$on('closeAttachments', function () {
                closeAllPanel();
            });

            $scope.$on('closeTranslations', function () {
                closeAllPanel();
            });

            $scope.$on('verifing', function () {
                closeOpenPanelIfAny();
            });
        });