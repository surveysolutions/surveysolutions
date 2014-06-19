(function() {
    'use strict';
    angular.module('designerApp')
        .factory('navigationService', [
            '$location', 'utilityService', 
            function($location, string) {
                var navigationService = {};

                navigationService.openQuestion = function(questionnaireId, currentChapterId, itemId) {
                    var url = string.format('/{0}/chapter/{1}/question/{2}', questionnaireId, currentChapterId, itemId);
                    $location.path(url);
                };

                navigationService.openRoster = function(questionnaireId, currentChapterId, itemId) {
                    var url = string.format('/{0}/chapter/{1}/roster/{2}', questionnaireId, currentChapterId, itemId);
                    $location.path(url);
                };

                navigationService.openGroup = function(questionnaireId, currentChapterId, itemId) {
                    var url = string.format('/{0}/chapter/{1}/group/{2}', questionnaireId, currentChapterId, itemId);
                    $location.path(url);
                };

                navigationService.openChapter = function(questionnaireId, chapterId) {
                    var url = string.format('/{0}/chapter/{1}', questionnaireId, chapterId);
                    $location.path(url);
                };

                navigationService.openQuestionnaire = function(questionnaireId) {
                    var url = string.format('/{0}', questionnaireId);
                    $location.path(url);
                };

                navigationService.editChapter = function(questionnaireId, chapterId) {
                    var url = string.format('/{0}/editchapter/{1}', questionnaireId, chapterId);
                    $location.path(url);
                };

                return navigationService;
            }
        ]);
}());