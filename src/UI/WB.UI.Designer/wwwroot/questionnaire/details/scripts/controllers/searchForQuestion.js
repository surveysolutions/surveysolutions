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
                        $scope.filters.splice(0, 0, allFolders);
                    });
            };

            var getIconClass = function(type, entity) {
                if (type === "question")
                    return $rootScope.answerTypeClass[entity.itemType];
                if (type ===  'static-text') 
                    return "icon-statictext";
                if (type === 'variable')
                    return "icon-variable";
                return null;
            }
            var search = function($scope) {
                $scope.$apply(function() {
                    var searchText = $scope.searchText.toLowerCase();
                    return $http({
                            method: 'GET',
                            url: baseUrl + '',
                            params: {
                                query: searchText,
                                folderId: $scope.selectedFilter.publicId,
                                privateOnly: false
                            }
                        })
                        .then(function(response) {
                            var results = response.data.entities;
                            _.forEach(results, function(entity) {
                                entity.hasFolder = (entity.folder || null) != null;
                                entity.type = $scope.getType(entity);
                                entity.icon = getIconClass(entity.type, entity);
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

            $scope.getType =  function(searchResult) {
                switch (searchResult.itemType) {
                    case 'Group': return "group"; 
                    case 'Roster': return "roster"; 
                    case 'StaticText': return 'static-text'; 
                    case 'Chapter': return 'group';
                    case 'Variable': return 'variable';
                    default: return 'question';
                }
            };

            $scope.getLink=  function(searchResult) {
                return '../../questionnaire/details/'+searchResult.questionnaireId +'/nosection/' + searchResult.type + '/' + searchResult.itemId;
            };

            $scope.pasteEntity = function(searchResult) {
                $uibModalInstance.close(searchResult);
            };

            $scope.loadFilters();
        });
