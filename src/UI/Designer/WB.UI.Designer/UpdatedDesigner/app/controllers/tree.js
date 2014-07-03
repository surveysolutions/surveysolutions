

angular.module('designerApp')
    .controller('TreeCtrl', [
        '$scope', '$stateParams', 'questionnaireId', 'questionnaireService', 'commandService', 'verificationService', 'utilityService', 'hotkeys', '$state', '$modal', '$log',
        function($scope, $stateParams, questionnaireId, questionnaireService, commandService, verificationService, utilityService, hotkeys, $state, $modal, $log) {
            'use strict';

              var connectTree = function () {
                    var setParent = function (item, parent) {
                        item.getParentItem = function () {
                            return parent;
                        };
                        _.each(item.items, function (child) {
                            setParent(child, item);
                        });
                    };

                    _.each($scope.items, function (item) {
                        setParent(item, null);
                    });
                };  
           
            questionnaireService.getChapterById(questionnaireId, $stateParams.chapterId)
                .success(function (result) {
                    $scope.items = result.items;
                    $scope.currentChapter = result;
                    connectTree();

                    window.ContextMenuController.get().init();
                });
        }
    ]);