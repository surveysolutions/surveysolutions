angular.module('questionnaires', ['ui.bootstrap', 'mgo-angular-wizard'])
    .controller('importController', ['$scope', '$modal', '$log', function ($scope, $modal, $log) {

        $scope.open = function () {

            var modalInstance = $modal.open({
                templateUrl: 'myModalContent.html',
                controller: 'importDialogController',
                resolve: {
                }
            });

            modalInstance.result.then(function () {
                $log.info('Model closed');
            }, function () {
                $log.info('Modal dismissed at: ' + new Date());
                $log.info($scope.test);
            });
        };
    }])
    .controller('importDialogController', ['$scope', '$modalInstance', '$http', '$log', 'WizardHandler', function ($scope, $modalInstance, $http, $log, wizardHandler) {
        $scope.signInInProggress = false;

    $scope.test = 'test';

        $scope.credentials = {
            userName: '',
            password: ''
        };

        $scope.questionnaires = {
            total: 0,
            pageSize: 10,
            filter: '',
            selectedItemId: false,
            items: []
        }

        $scope.getQuestionnaries = function (page) {
            $log.info('getQuestionnaires');
            if (_.isUndefined(page)) {
                page = 1;
            }
            $http({
                method: 'GET',
                url: '/Headquarters/api/questionnaires',
                params: {
                    filter: $scope.questionnaires.filter,
                    page: page,
                    pageSize: $scope.questionnaires.pageSize
                }
            }).success(function (data) {
                $scope.questionnaires.items = data.Items;
                $scope.questionnaires.total = data.Total;
            });
        }

        $scope.login = function () {
            $scope.signInInProggress = true;
            $http({
                method: 'POST',
                url: '/Headquarters/api/Questionnaires/LoginToDesigner',
                params: {
                    userName: $scope.credentials.userName,
                    password: $scope.credentials.password
                }
            }).success(function () {
                $scope.signInInProggress = false;
                wizardHandler.wizard('importWizard').next();

                $scope.getQuestionnaries(1);

            

            }).error(function (data) {
                $scope.signInInProggress = false;
                $scope.errorMessage = data.Message;
                $log.error(data);
            });
        };

        $scope.selectItem = function (itemId) {
            $scope.questionnaires.selectedItemId = itemId;
            $log.info(itemId);
        }

        $scope.cancel = function () {
            $modalInstance.dismiss('cancel');
        };
    }]);