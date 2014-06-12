(function() {
    'use strict';

    angular.module('designerApp')
        .controller('VerificationCtrl', [
            '$scope', '$routeParams', 'verificationService',
            function($scope, $routeParams, verificationService) {
                $scope.verificationStatus = {
                    errorsCount: 0,
                    errors: []
                };

                $scope.verify = function() {
                    verificationService.verify($routeParams.questionnaireId).success(function(result) {
                        $scope.verificationStatus.errors = result.errors;
                        $scope.verificationStatus.errorsCount = result.errors.length;

                        if ($scope.verificationStatus.errorsCount > 0) {
                            $('#verification-modal').modal({
                                backdrop: false,
                                show: true
                            });
                        }
                    });
                };
            }
        ]);
})();