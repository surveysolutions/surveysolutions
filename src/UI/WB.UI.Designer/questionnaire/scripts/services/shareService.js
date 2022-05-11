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

            shareService.revokeAccess = function (userId, email, questionnaireId) {
                return commandService.execute("RemoveSharedPersonFromQuestionnaire", {
                    personId: userId,
                    email: email,
                    questionnaireId: questionnaireId
                });
            };

            shareService.passOwnership = function (ownerEmail, newOwnerId, newOwnerEmail, questionnaireId) {
                return commandService.execute("PassOwnershipFromQuestionnaire", {
                    ownerEmail, newOwnerId, newOwnerEmail,
                    questionnaireId: questionnaireId
                });
            };

            shareService.udpateQuestionnaire = function(questionnaireId, title, variable, hideifDisabled, isPublic, defaultLanguageName) {
                return commandService.execute("UpdateQuestionnaire", {
                    title: title,
                    variable: variable,
                    questionnaireId: questionnaireId,
                    hideifDisabled: hideifDisabled, 
                    isPublic: isPublic,
                    defaultLanguageName : defaultLanguageName
                });
            };

            shareService.updateAnonymousQuestionnaireSettings = function(questionnaireId, isActive) {
                var baseUrl = '../../questionnaire/updateAnonymousQuestionnaireSettings/' + questionnaireId;
                return $http.post(baseUrl, { isActive: isActive }, { headers: { 'X-CSRF-TOKEN': getCsrfCookie()} });
            };

            shareService.regenerateAnonymousQuestionnaireLink = function(questionnaireId) {
                var baseUrl = '../../questionnaire/regenerateAnonymousQuestionnaireLink/' + questionnaireId;
                return $http.post(baseUrl, null, { headers: { 'X-CSRF-TOKEN': getCsrfCookie()} });
            };

            return shareService;
        }
    ]);
})();
