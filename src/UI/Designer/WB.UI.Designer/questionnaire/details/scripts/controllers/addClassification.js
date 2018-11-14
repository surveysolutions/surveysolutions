angular.module('designerApp')
    .controller('addClassificationCtrl',
        function ($rootScope, $scope, $http, $state, commandService, utilityService, isReadOnlyForUser, hasOptions, $i18next, $uibModalInstance) {
            var baseUrl = '../../api';

            $scope.isReadOnlyForUser = isReadOnlyForUser;
            var allClassificationGroups = { id: null, title: $i18next.t('AllClassifications') };
            $scope.groups = [];
            $scope.selectedGroup = allClassificationGroups;
            $scope.searchText = '';
            $scope.classifications1 = [];
            $scope.classifications2 = [];
            $scope.totalResults = 0;

            utilityService.setFocusIn('searchFor');

            $scope.loadClassificationGroups = function()
            {
                return $http({
                        method: 'GET',
                        url: baseUrl + '/classifications/groups',
                        params: {}
                    })
                    .then(function(response) {
                        $scope.groups = response.data;
                        $scope.groups.splice(0, 0, allClassificationGroups);
                    });
            };

            $scope.loadCategories = function(classification) {
                if (classification.categories.length > 0)
                    return new Promise(function(resolve, reject){
                        resolve();
                    });
                return $http({
                        method: 'GET',
                        url: baseUrl + '/classification/'+ classification.id + '/categories',
                        params: {}
                    })
                    .then(function(response) {
                        classification.categories = response.data;
                    });
            };

            var search = function($scope) {
                $scope.$apply(function() {
                    var searchText = $scope.searchText.toLowerCase();
                    return $http({
                            method: 'GET',
                            url: baseUrl + '/classifications/search',
                            params: {
                                query: searchText,
                                groupId: $scope.selectedGroup.id
                            }
                        })
                        .then(function(response) {
                            var classifications = response.data.classifications;
                            _.each(classifications, function(classification) {
                                classification.categories = [];
                                classification.categoriesAreOpen = false;
                            });
                            $scope.classifications1 = classifications.slice(0, classifications.length/2);
                            $scope.classifications2 = classifications.slice(classifications.length/2 + 1);
                            $scope.totalResults = response.data.total;
                        });
                });
            };

            var searchThrottled = _.debounce(search, 1000);

            $scope.$watch('searchText', function(){searchThrottled($scope);});

            $scope.changeClassificationGroup = function(group) {
                $scope.selectedGroup = group;
                searchThrottled($scope);
            };

            $scope.toggleCategories  = function(classification) {
                if (classification.categories.length === 0) {
                    $scope.loadCategories(classification)
                        .then(function() {
                            classification.categoriesAreOpen = true;
                        });
                } else {
                    classification.categoriesAreOpen = !classification.categoriesAreOpen;
                }
            };


            $scope.replaceOptions = function(classification) {
                if (classification.categories.length === 0) {
                    $scope.loadCategories(classification)
                        .then(function() {
                            $uibModalInstance.close(classification);
                        });
                } else {
                    $uibModalInstance.close(classification);
                }
            };
            $scope.loadClassificationGroups();
        });
