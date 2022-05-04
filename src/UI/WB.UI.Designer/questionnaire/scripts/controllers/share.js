angular
    .module('designerApp')
    .controller('shareCtrl', function(
        $scope,
        $log,
        $i18next,
        $uibModalInstance,
        questionnaire,
        currentUser,
        shareService,
        moment
    ) {
        'use strict';

        $scope.currentUser = currentUser;
        $scope.questionnaire = questionnaire;

        $scope.isQuestionnaireOwner = function() {
            var isowner = false;

            questionnaire.sharedPersons.forEach(function(p) {
                if (p.email == currentUser.email && p.isOwner) isowner = true;
            });

            return isowner;
        };

        $scope.toLocalDateTime = function (utc) {
            return moment.utc(utc).local().format('YYYY-MM-DD HH:mm');
        };


        $scope.passConfirmationOpen = null;
        $scope.questionnaire.editedTitle = questionnaire.title;
        $scope.questionnaire.editedVariable = questionnaire.variable;
        $scope.questionnaire.editedHideIfDisabled = questionnaire.hideIfDisabled;
        $scope.questionnaire.isShareQuestionnaireAsAnonymous = questionnaire.isShareQuestionnaireAsAnonymous;
        $scope.questionnaire.anonymousQuestionnaireId = questionnaire.anonymousQuestionnaireId;
        $scope.questionnaire.anonymousQuestionnaireShareDate = $scope.toLocalDateTime(questionnaire.anonymousQuestionnaireShareDateUtc);

        $scope.shareTypeOptions = [
            { text: $i18next.t('SettingsShareEdit'), name: 'Edit' },
            { name: 'View', text: $i18next.t('SettingsShareView') }
        ];

        $scope.viewModel = {
            shareWith: '',
            shareForm: {},
            shareType: $scope.shareTypeOptions[0],
            doesUserExist: true
        };
        $scope.getShareType = function(type) {
            if (type === 'Edit' || type === 'View')
                return _.find($scope.shareTypeOptions, { name: type });
            else return _.find($scope.shareTypeOptions, { name: type.name });
        };

        $scope.cancel = function() {
            $uibModalInstance.close();
        };

        $scope.invite = function() {
            var request = shareService.findUserByEmailOrLogin(
                $scope.viewModel.shareWith
            );
            request.then(function(result) {
                var data = result.data;
                $scope.viewModel.doesUserExist = data.doesUserExist;

                if (data.doesUserExist) {
                    var shareRequest = shareService.shareWith(
                        $scope.viewModel.shareWith,
                        $scope.questionnaire.questionnaireId,
                        $scope.viewModel.shareType.name
                    );
                    shareRequest.then(function() {
                        if (
                            _.where($scope.questionnaire.sharedPersons, {
                                email: $scope.viewModel.shareWith
                            }).length === 0
                        ) {
                            $scope.questionnaire.sharedPersons.push({
                                email: data.email,
                                login: data.userName,
                                userId: data.id,
                                shareType: $scope.viewModel.shareType
                            });
                            $scope.sortSharedPersons();
                        }

                        $scope.viewModel.shareWith = '';
                        $scope.viewModel.doesUserExist = true;
                        $scope.viewModel.shareForm.$setPristine();
                    });
                }
            });
        };

        $scope.updateTitle = function() {
            var updateRequest = shareService.udpateQuestionnaire(
                $scope.questionnaire.questionnaireId,
                $scope.questionnaire.editedTitle,
                $scope.questionnaire.editedVariable,
                $scope.questionnaire.editedHideIfDisabled,
                $scope.questionnaire.isPublic,
                $scope.questionnaire.defaultLanguageName
            );
            updateRequest.then(function() {
                $scope.questionnaire.title = $scope.questionnaire.editedTitle;
                $scope.questionnaire.variable =
                    $scope.questionnaire.editedVariable;
                $scope.questionnaire.hideIfDisabled =
                    $scope.questionnaire.editedHideIfDisabled;
                $uibModalInstance.close();
                $scope.questionnaireForm.$setPristine();
            });
        };

        $scope.revokeAccess = function(personInfo) {
            var revokeRequest = shareService.revokeAccess(
                personInfo.userId,
                personInfo.email,
                $scope.questionnaire.questionnaireId
            );

            revokeRequest.then(function() {
                $scope.questionnaire.sharedPersons = _.without(
                    $scope.questionnaire.sharedPersons,
                    _.findWhere($scope.questionnaire.sharedPersons, {
                        email: personInfo.email
                    })
                );

                $scope.sortSharedPersons();
            });
        };

        $scope.passOwnership = function(personInfo) {
            $scope.passConfirmationOpen = personInfo.email;
        };

        $scope.passOwnershipConfirmation = function(newOwner) {
            var passOwnership = shareService.passOwnership(
                $scope.currentUser.email,
                newOwner.userId,
                newOwner.email,
                $scope.questionnaire.questionnaireId
            );

            passOwnership.then(function() {
                newOwner.isOwner = true;

                $scope.questionnaire.sharedPersons.forEach(function(person) {
                    if (person.email == $scope.currentUser.email) {
                        person.isOwner = false;
                    }

                    if (person.email == newOwner.email) {
                        person.isOwner = true;
                    }
                });

                $scope.passConfirmationOpen = null;
            });
        };

        $scope.passOwnershipCancel = function() {
            $scope.passConfirmationOpen = null;
        };

        $scope.togglePublicity = function() {
            var updateRequest = shareService.udpateQuestionnaire(
                $scope.questionnaire.questionnaireId,
                $scope.questionnaire.title,
                $scope.questionnaire.variable,
                $scope.questionnaire.editedHideIfDisabled,
                !$scope.questionnaire.isPublic,
                $scope.questionnaire.defaultLanguageName
            );

            updateRequest.then(function() {
                $scope.questionnaire.isPublic = !$scope.questionnaire.isPublic;
            });
        };

        $scope.updateAnonymousQuestionnaireSettings = function() {
            var updateRequest = shareService.updateAnonymousQuestionnaireSettings(
                $scope.questionnaire.questionnaireId,
                !$scope.questionnaire.isShareQuestionnaireAsAnonymous
            );

            updateRequest.then(function(result) {
                var data = result.data
                $scope.questionnaire.isShareQuestionnaireAsAnonymous = data.isActive;
                $scope.questionnaire.anonymousQuestionnaireId = data.isActive ? data.anonymousQuestionnaireId : null;
                $scope.questionnaire.anonymousQuestionnaireShareDate = $scope.toLocalDateTime(data.anonymousQuestionnaireShareDateUtc);
            });
        };
        
        $scope.regenerateAnonymousQuestionnaireLink = function() {
            var updateRequest = shareService.regenerateAnonymousQuestionnaireLink(
                $scope.questionnaire.questionnaireId
            );

            updateRequest.then(function(result) {
                var data = result.data
                $scope.questionnaire.isShareQuestionnaireAsAnonymous = data.isActive;
                $scope.questionnaire.anonymousQuestionnaireId = data.isActive ? data.anonymousQuestionnaireId : null;
                $scope.questionnaire.anonymousQuestionnaireShareDate = $scope.toLocalDateTime(data.anonymousQuestionnaireShareDateUtc);
            });
        };
        
        $scope.changeShareType = function(shareType) {
            $scope.viewModel.shareType = shareType;
        };

        $scope.sortSharedPersons = function () {
            var owner = _.findWhere($scope.questionnaire.sharedPersons, { isOwner: true });
            var sharedPersons = _.sortBy(_.without($scope.questionnaire.sharedPersons, owner), ['email']);

            $scope.questionnaire.sharedPersons = [owner].concat(sharedPersons);
        };

        $scope.getAnonymousQuestionnaireLink = function () {
            return window.location.origin + '/questionnaire/details/' + questionnaire.anonymousQuestionnaireId
        };

        $scope.copyAnonymousQuestionnaireLink = function () {
            var copyText = document.getElementById("anonymousQuestionnaireLink");
            copyText.select();
            copyText.setSelectionRange(0, 99999);
            navigator.clipboard.writeText(copyText.value);
        };

        $scope.sortSharedPersons();
});
