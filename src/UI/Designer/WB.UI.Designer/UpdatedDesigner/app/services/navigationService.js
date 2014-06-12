(function() {
    'use strict';
    angular.module('designerApp')
        .factory('navigationService', [
            '$location', 'utilityService', function($location, string) {
                var navigationService = {};

                navigationService.openItem = function(questionnaireId, currentChapterId, itemId) {
                    var url = string.format('/{0}/chapter/{1}/item/{2}', questionnaireId, currentChapterId, itemId);
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