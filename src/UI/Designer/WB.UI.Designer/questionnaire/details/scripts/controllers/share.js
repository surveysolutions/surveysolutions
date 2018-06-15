angular.module('designerApp').controller('shareCtrl',
    function ($scope, $log, $i18next, $uibModalInstance, questionnaire, shareService) {
        "use strict";

        $scope.questionnaire = questionnaire;
        $scope.questionnaire.editedTitle = questionnaire.title;
        $scope.questionnaire.editedVariable = questionnaire.variable;

        $scope.shareTypeOptions = [{ text: $i18next.t('SettingsShareEdit'), name: "Edit" }, { name: "View", text: $i18next.t('SettingsShareView') }];
        
        $scope.viewModel = {
            shareWith: '',
            shareForm: {},
            shareType: $scope.shareTypeOptions[0],
            doesUserExist: true
        };
        $scope.getShareType = function(type) {
            if (type === 'Edit' || type === 'View') 
                return _.find($scope.shareTypeOptions, {name: type});
            else
                return _.find($scope.shareTypeOptions, {name: type.name});
        }

        $scope.cancel = function () {
            $uibModalInstance.close();
        };

        $scope.invite = function () {
            var request = shareService.findUserByEmailOrLogin($scope.viewModel.shareWith);
            request.then(function (result) {
                var data = result.data;
                $scope.viewModel.doesUserExist = data.doesUserExist;

                if (data.doesUserExist) {
                    var shareRequest = shareService.shareWith(
                        $scope.viewModel.shareWith, 
                        $scope.questionnaire.questionnaireId, 
                        $scope.viewModel.shareType.name);
                    shareRequest.then(function () {
                        if (_.where($scope.questionnaire.sharedPersons, { email: $scope.viewModel.shareWith }).length === 0) {
                            $scope.questionnaire.sharedPersons.push({
                                 email: data.email, 
                                 login: data.userName,
                                 userId: data.id,
                                 shareType: $scope.viewModel.shareType
                            });
                        }

                        $scope.viewModel.shareWith = '';
                        $scope.viewModel.doesUserExist = true;
                        $scope.viewModel.shareForm.$setPristine();
                    });
                }
            });
        };

        $scope.updateTitle = function () {
            var updateRequest = shareService.udpateQuestionnaire($scope.questionnaire.questionnaireId, $scope.questionnaire.editedTitle, $scope.questionnaire.editedVariable, $scope.questionnaire.isPublic);
            updateRequest.then(function () {
                $scope.questionnaire.title = $scope.questionnaire.editedTitle;
                $scope.questionnaire.variable = $scope.questionnaire.editedVariable;
                $uibModalInstance.close();
                $scope.questionnaireForm.$setPristine();
            });
        };

        $scope.revokeAccess = function (personInfo) {
            var revokeRequest = shareService.revokeAccess(personInfo.email, $scope.questionnaire.questionnaireId);

            revokeRequest.then(function () {
                $scope.questionnaire.sharedPersons = _.without($scope.questionnaire.sharedPersons,
                    _.findWhere($scope.questionnaire.sharedPersons, { email: personInfo.email }));
            });
        };

        $scope.togglePublicity = function () {
            var updateRequest = shareService.udpateQuestionnaire($scope.questionnaire.questionnaireId, $scope.questionnaire.title, !$scope.questionnaire.isPublic);
            updateRequest.then(function () {
                $scope.questionnaire.isPublic = !$scope.questionnaire.isPublic;
            });
        };
        $scope.changeShareType = function (shareType) {
            $scope.viewModel.shareType = shareType;
        };
    }
);
