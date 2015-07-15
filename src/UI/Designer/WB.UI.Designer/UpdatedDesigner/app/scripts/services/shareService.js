(function() {
    angular.module('designerApp').factory('shareService', [
        '$http', 'utilityService', 'commandService',
        function($http, utils, commandService) {
            var shareService = {};

            shareService.findUserByEmail = function(email) {
                var baseUrl = '../../account/findbyemail';
                return $http.post(baseUrl, { email: email });
            };

            shareService.shareWith = function(email, questionnaireId, shareType) {
                return commandService.execute("AddSharedPersonToQuestionnaire", {
                    email: email,
                    questionnaireId: questionnaireId,
                    shareType: shareType
                });
            };

            shareService.revokeAccess = function(email, questionnaireId) {
                return commandService.execute("RemoveSharedPersonFromQuestionnaire", {
                    email: email,
                    questionnaireId: questionnaireId
                });
            };

            shareService.udpateQuestionnaire = function(questionnaireId, title, isPublic) {
                return commandService.execute("UpdateQuestionnaire", {
                    title: title,
                    questionnaireId: questionnaireId,
                    isPublic: isPublic
                });
            };

            return shareService;
        }
    ]);
})();