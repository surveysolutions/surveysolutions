(function() {
    'use strict';
    angular.module('designerApp').factory('commentsService', 
        function($http, commandService, utilityService, blockUI, $q) {
        var urlBase = '../../questionnaire/{0}';
        var commentsService = {};

        function post(url, data) {
            return sendRequest('POST', url, data);
        }
        function patch(url, data) {
            return sendRequest('PATCH', url, data);
        }
        var deleteRequest = function(url, data) {
            return sendRequest('DELETE', url, data);
        };
        function sendRequest(method, url, data) {
            blockUI.start();
            return $http({
                method: method,
                url: url,
                data: data,
                headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' }
            }).then(function (response) {
                blockUI.stop();
                return response;
            }, function (response) {
                blockUI.stop();
                return $q.reject(response);
            });
        }
            
        commentsService.getCommentThreads = function(questionnaireId) {
            var url = utilityService.format('{0}/commentThreads', utilityService.format(urlBase, questionnaireId));
            return $http.get(url);
        };

        commentsService.getItemCommentsById = function(questionnaireId, itemId) {
            var url = utilityService.format('{0}/entity/{1}/comments', utilityService.format(urlBase, questionnaireId), itemId);
            return $http.get(url);
        };

        commentsService.resolveComment = function(questionnaireId, commentId) {
            var url = utilityService.format('{0}/comment/resolve/{1}', utilityService.format(urlBase, questionnaireId), commentId);
            return patch(url);
        };

        commentsService.postComment = function(questionnaireId, itemId, comment) {
            var url = utilityService.format('{0}/entity/addComment', utilityService.format(urlBase, questionnaireId));
            return post(url, comment);
        };
            
        commentsService.deleteComment = function(questionnaireId, itemId) {
            var url = utilityService.format('{0}/comment/{1}', utilityService.format(urlBase, questionnaireId), itemId);
            return deleteRequest(url);
        };

        return commentsService;
    });
}());
