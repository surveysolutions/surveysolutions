angular.module('designerApp')
    .controller('CommentsCtrl', 
        function ($rootScope, $scope, $state, $i18next, commandService, utilityService, $log, confirmService, questionnaireService, hotkeys) {
            'use strict';

            var hideCommentsPane = 'ctrl+alt+c';

            if (hotkeys.get(hideCommentsPane) !== false) {
                hotkeys.del(hideCommentsPane);
            }

            hotkeys.add(hideCommentsPane, $i18next.t('HotkeysCloseLookup'), function (event) {
                event.preventDefault();
                $scope.foldback();
            });

            $scope.commentThreads = [];

            $scope.isFolded = false;

            $scope.unfold = function () {
                $scope.isFolded = true;
            };

            $scope.foldback = function () {
                $scope.isFolded = false;
                $rootScope.$broadcast("closeComments", {});
            };

            $scope.$on('openComments', function (scope, params) {
                $scope.unfold();
                if (!_.isUndefined(params) && !_.isUndefined(params.focusOn)) {
                    setTimeout(function () { utilityService.focus("focusComment" + params.focusOn); }, 500);
                }
            });

            $scope.$on('closeCommentsRequested', function () {
                $scope.foldback();
            });

            $rootScope.$on('questionnaireLoaded', function () {
                //$scope.loadLookupTables();
            });
        });
