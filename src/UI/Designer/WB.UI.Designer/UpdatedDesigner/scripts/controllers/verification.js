'use strict';

angular.module('designerApp')
    .controller('VerificationCtrl', function ($scope, $routeParams, verificationService) {
        $scope.verificationStatus = {
            errorsCount: 8,
            errors: []
        };

        $scope.verify = function () {
            verificationService.verify($routeParams.questionnaireId).success(function (result) {
                $scope.verificationStatus.errors = result.errors;
                $scope.verificationStatus.errorsCount = result.errors.length;
            });
        };
    });