(function(app) {
    'use strict';
    app.controller('shareCtrl',
    [
        '$scope', '$log', '$modalInstance', 'questionnaire', 'shareService',
        function ($scope, $log, $modalInstance, questionnaire, shareService) {
            $scope.questionnaire = questionnaire;
            
            $scope.viewModel = {
                shareWith: '',
                shareForm: {}
            };

            $scope.cancel = function () {
                $modalInstance.dismiss();
            };

            $scope.invite = function() {
                var request = shareService.findUserByEmail($scope.viewModel.shareWith);
                request.success(function (data) {
                    $scope.viewModel.shareForm.shareWithInput.$setValidity('', data.isUserExist);

                    if (data.isUserExist) {
                        var shareRequest = shareService.shareWith($scope.viewModel.shareWith, $scope.questionnaire.questionnaireId);
                        shareRequest.success(function () {
                            if (_.where($scope.questionnaire.sharedPersons, { email: $scope.viewModel.shareWith }).length == 0) {
                                $scope.questionnaire.sharedPersons.push({ email: $scope.viewModel.shareWith });
                            }

                            $scope.viewModel.shareWith = '';
                        });
                    }
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
        }
    ]);
})(app);