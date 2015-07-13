(function() {
    'use strict';
    angular.module('designerApp').controller('shareCtrl',
    [
        '$scope', '$log', '$modalInstance', 'questionnaire', 'shareService',
        function($scope, $log, $modalInstance, questionnaire, shareService) {
            $scope.questionnaire = questionnaire;
            $scope.questionnaire.editedTitle = questionnaire.title;

            $scope.viewModel = {
                shareWith: '',
                shareForm: {},
                shareType: 'Edit',
                doesUserExist: true
            };

            $scope.shareTypeOptions = [{ name: "Edit" }, { name: "View" }];

            $scope.cancel = function () {
                //$scope.questionnaire.title = $scope.initialTitle;
                $modalInstance.dismiss();
            };

            $scope.invite = function() {
                var request = shareService.findUserByEmail($scope.viewModel.shareWith);
                request.success(function(data) {
                    $scope.viewModel.doesUserExist = data.doesUserExist;

                    if (data.doesUserExist) {
                        var shareRequest = shareService.shareWith($scope.viewModel.shareWith, $scope.questionnaire.questionnaireId, $scope.viewModel.shareType);
                        shareRequest.success(function() {
                            if (_.where($scope.questionnaire.sharedPersons, { email: $scope.viewModel.shareWith }).length === 0) {
                                $scope.questionnaire.sharedPersons.push({ email: $scope.viewModel.shareWith, sharedType: $scope.viewModel.shareType });
                            }

                            $scope.viewModel.shareWith = '';
                            $scope.viewModel.doesUserExist = true;
                        });
                    }
                });
            };

            $scope.updateTitle = function() {
                var updateRequest = shareService.udpateQuestionnaire($scope.questionnaire.questionnaireId, $scope.questionnaire.editedTitle, $scope.questionnaire.isPublic);
                updateRequest.success(function () {
                    $scope.questionnaire.title = $scope.questionnaire.editedTitle;
                    $modalInstance.dismiss();
                });
            };

            $scope.revokeAccess = function(personInfo) {
                var revokeRequest = shareService.revokeAccess(personInfo.email, $scope.questionnaire.questionnaireId);

                revokeRequest.success(function() {
                    $scope.questionnaire.sharedPersons = _.without($scope.questionnaire.sharedPersons,
                        _.findWhere($scope.questionnaire.sharedPersons, { email: personInfo.email }));
                });
            };

            $scope.togglePublicity = function() {
                var updateRequest = shareService.udpateQuestionnaire($scope.questionnaire.questionnaireId, $scope.questionnaire.title, !$scope.questionnaire.isPublic);
                updateRequest.success(function() {
                    $scope.questionnaire.isPublic = !$scope.questionnaire.isPublic;
                });
            };
            $scope.changeShareType = function (shareType) {
                $scope.viewModel.shareType = shareType.name;
            }
        }
    ]);
})();