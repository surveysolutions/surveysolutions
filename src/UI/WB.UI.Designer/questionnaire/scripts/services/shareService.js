(function() {
    angular.module('designerApp').factory('shareService', [
        '$http', 'utilityService', 'commandService',
        function($http, utils, commandService) {
            var shareService = {};

            shareService.findUserByEmailOrLogin = function(emailOrLogin) {
                var baseUrl = '../../api/users/findbyemail';
                return $http.post(baseUrl, { query: emailOrLogin });
            };

            shareService.shareWith = function(emailOrLogin, questionnaireId, shareType) {
                return commandService.execute("AddSharedPersonToQuestionnaire", {
                    emailOrLogin: emailOrLogin,
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

            shareService.udpateQuestionnaire = function(questionnaireId, title, variable, hideifDisabled, isPublic) {
                return commandService.execute("UpdateQuestionnaire", {
                    title: title,
                    variable: variable,
                    questionnaireId: questionnaireId,
                    hideifDisabled: hideifDisabled, 
                    isPublic: isPublic
                });
            };

            return shareService;
        }
    ]);
})();
