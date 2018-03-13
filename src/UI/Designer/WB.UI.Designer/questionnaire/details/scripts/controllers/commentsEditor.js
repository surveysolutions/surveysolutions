angular.module('designerApp')
    .controller('CommentsEditorCtrl', 
        function ($rootScope, $scope, $state, commentsService, $i18next, commandService, utilityService, $log, confirmService,  hotkeys) {
            'use strict';

            $scope.loadComments = function() {
                commentsService.getItemCommentsById($state.params.questionnaireId, $state.params.itemId)
                    .then(function(result) {
                        var data = result.data;
                        console.log(data);
                        $scope.comments = data;
                        $scope.activeComment = {
                            comment: ''
                        };
                    });
            };
            $scope.currentChapterId = $state.params.chapterId;
            $scope.loadComments();
        });
