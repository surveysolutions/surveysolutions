(function() {
    'use strict';
    angular.module('designerApp')
        .factory('commentsService', [
            '$http', 'commandService', 'utilityService',
            function($http, commandService, string) {

                var urlBase = '../../api/comments';
                var commentsService = {};

                commentsService.getItemCommentsById = function(questionnaireId, itemId) {
                    var url = string.format('{0}/get/{1}?itemId={2}', urlBase, questionnaireId, itemId);
                    return $http.get(url);
                };

                return commentsService;
            }
        ]);

}());
