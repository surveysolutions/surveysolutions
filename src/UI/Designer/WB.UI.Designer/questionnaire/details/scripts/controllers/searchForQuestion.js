angular.module('designerApp')
    .controller('searchForQuestionCtrl',
        function ($rootScope, $scope, $http, $state, commandService, utilityService, isReadOnlyForUser, $i18next, $uibModalInstance) {
            var baseUrl = '../../api/search';

            $scope.isReadOnlyForUser = isReadOnlyForUser;
            var allFolders = { id: null, title: $i18next.t('AllFolders') };
            
            $scope.filters = [];
            $scope.selectedFilter = allFolders;
            $scope.searchText = '';
            $scope.searchResult1 = [];
            $scope.searchResult2 = [];
            $scope.totalResults = 0;

            utilityService.setFocusIn('searchFor');

            $scope.loadFilters = function()
            {
                return $http({
                        method: 'GET',
                        url: baseUrl + '/filters',
                        params: {}
                    })
                    .then(function(response) {
                        $scope.filters = response.data;
                        $scope.groups.splice(0, 0, allFolders);
                    });
            };

            var search = function($scope) {
                $scope.$apply(function() {
                    var searchText = $scope.searchText.toLowerCase();
                    return $http({
                            method: 'GET',
                            url: baseUrl + '',
                            params: {
                                query: searchText,
                                folderId: $scope.selectedFilter.id,
                                privateOnly: false
                            }
                        })
                        .then(function(response) {
                            var results = response.data.entities;
                            _.forEach(results, function(entity) {
                                entity.hasFolder = (entity.folder || null) != null;
                            });
                            var half = Math.ceil(results.length / 2);
                            $scope.searchResult1 = results.slice(0, half);
                            $scope.searchResult2 = results.slice(half);
                            $scope.totalResults = response.data.total;
                        });
                });
            };

            var searchThrottled = _.debounce(search, 1000);

            $scope.$watch('searchText', function(){searchThrottled($scope);});

            $scope.changeFilter = function(filter) {
                $scope.selectedFilter = filter;
                searchThrottled($scope);
            };

            $scope.getLink=  function(searchResult) {
                var urlItemType = '';
                switch (searchResult.itemType) {
                    case 'Question': urlItemType = 'question'; break;
                    case 'Group': urlItemType =(searchResult.isRoster ? "roster" : "group"); break;
                    case 'StaticText': urlItemType = 'static-text'; break;
                    case 'Chapter':  urlItemType ='group';break;
                    case 'Variable': urlItemType ='variable';break;
                }
                return '../../questionnaire/details/'+searchResult.questionnaireId +'/nosection/' + urlItemType + '/' + searchResult.itemId;
            };

            $scope.pasteEntity = function(searchResult) {
                $uibModalInstance.close(searchResult);
            };

            $scope.loadFilters();
        });
