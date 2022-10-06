angular.module('designerApp')
    .controller('CategoriesCtrl',
        function ($rootScope, $scope, $state, $i18next, hotkeys, commandService, utilityService, confirmService, Upload, $uibModal, notificationService, moment) {
            'use strict';

            $scope.downloadBaseUrl = '../../categories';
            $scope.isReadOnlyForUser = false;
            $scope.shouldUserSeeReloadPromt = false;
            $scope.openEditor = null
            
            $scope.bcChannel = null

            // https://developer.mozilla.org/en-US/docs/Web/API/Broadcast_Channel_API
            // Automatically reload window on popup close. If supported by browser
            if('BroadcastChannel' in window){
                $scope.bcChannel = new BroadcastChannel("editcategory")
                $scope.bcChannel.onmessage = function(ev) {
                    console.log(ev.data)
                    if(ev.data === 'close#' + $scope.openEditor) {
                        window.location.reload();
                    }
                }
            }

            var hideCategoriesPane = 'ctrl+shift+c';

            if (hotkeys.get(hideCategoriesPane) !== false) {
                hotkeys.del(hideCategoriesPane);
            }

            hotkeys.add(hideCategoriesPane, $i18next.t('HotkeysCloseCategories'), function (event) {
                event.preventDefault();
                $scope.foldback();
            });

            $scope.categoriesList = [];

            var dataBind = function (categories, categoriesDto) {
                categories.categoriesId = categoriesDto.categoriesId;
                categories.name = categoriesDto.name;
                categories.file = null;
                categories.content = {};
                categories.content.details = {};

                if (!_.isUndefined(categoriesDto.content) && !_.isNull(categoriesDto.content)) {
                    categories.content.size = categoriesDto.content.size;
                    categories.content.type = categoriesDto.content.contentType;

                    if (!_.isUndefined(categoriesDto.content.details) && !_.isNull(categoriesDto.content.details)) {
                        categories.content.details.height = categoriesDto.content.details.height;
                        categories.content.details.width = categoriesDto.content.details.width;
                    }
                }
            };

            $scope.loadCategories = function () {
                if ($scope.questionnaire === null)
                    return;

                $scope.isReadOnlyForUser = $scope.questionnaire.isReadOnlyForUser || false;

                if ($scope.questionnaire.categories === null)
                    return;

                $scope.categoriesList = []
                _.each($scope.questionnaire.categories, function (categoriesDto) {
                    var categories = { checkpoint: {} };
                    if (!_.any($scope.categoriesList, function (elem) { return elem.categoriesId === categoriesDto.categoriesId; })) {
                        dataBind(categories, categoriesDto);
                        dataBind(categories.checkpoint, categoriesDto);
                        $scope.categoriesList.push(categories);
                    }
                });

                $scope.shouldUserSeeReloadPromt = false;
                // $scope.$apply()
            };


            $scope.isFolded = false;

            $scope.unfold = function () {
                $scope.isFolded = true;
            };

            $scope.addNewCategory = function() {
                if ($scope.isReadOnlyForUser) {
                    notificationService.notice($i18next.t('NoPermissions'));
                    return;
                }

                var categories = { categoriesId: utilityService.guid() };
                
                commandService.updateCategories($state.params.questionnaireId, categories)
                    .then(function (response) {
                        if (response.status !== 200) return;

                        categories.checkpoint = categories.checkpoint || {};

                        dataBind(categories.checkpoint, categories);
                        $scope.categoriesList.push(categories);
                        updateQuestionnaireCategories();

                        setTimeout(function() {
                                utilityService.focus("focusCategories" + categories.categoriesId);
                            },
                            500);
                    });
            }

            $scope.createAndUploadFile = function (file) {
                if (_.isNull(file) || _.isUndefined(file)) {
                    return;
                }

                if ($scope.isReadOnlyForUser) {
                    notificationService.notice($i18next.t('NoPermissions'));
                    return;
                }

                var categories = { categoriesId: utilityService.guid() };

                $scope.fileSelected(categories, file, function () {
                    commandService.updateCategories($state.params.questionnaireId, categories)
                        .then(function (response) {
                            if (response.status !== 200) return;

                            categories.checkpoint = categories.checkpoint || {};

                            dataBind(categories.checkpoint, categories);
                            $scope.categoriesList.push(categories);
                            updateQuestionnaireCategories();

                            setTimeout(function() {
                                    utilityService.focus("focusCategories" + categories.categoriesId);
                                },
                                500);
                        });
                });
            };

            $scope.fileSelected = function(categories, file, callback) {
                if (_.isUndefined(file) || _.isNull(file)) {
                    return;
                }

                categories.file = file;

                categories.content = {};
                categories.content.size = file.size;
                categories.content.type = file.type;

                categories.meta = {};
                categories.meta.fileName = file.name;
                categories.meta.lastUpdated = moment();

                var maxNameLength = 32;

                var suspectedCategories = categories.meta.fileName.match(/[^[\]]+(?=])/g);

                if (suspectedCategories && suspectedCategories.length > 0)
                    categories.name = suspectedCategories[0];
                else
                    categories.name = categories.meta.fileName.replace(/\.[^/.]+$/, "");

                var fileNameLength = categories.name.length;
                categories.name = categories.name.substring(0, fileNameLength < maxNameLength ? fileNameLength : maxNameLength);
                categories.oldCategoriesId = categories.categoriesId;
                categories.categoriesId = utilityService.guid();

                if (!_.isUndefined(categories.form)) {
                    categories.form.$setDirty();
                }

                if (!_.isUndefined(callback)) {
                    callback();
                }
            };

            $scope.saveCategories = function (categories) {
                commandService.updateCategories($state.params.questionnaireId, categories).then(function () {
                    dataBind(categories.checkpoint, categories);
                    categories.form.$setPristine();

                    updateQuestionnaireCategories(categories);
                });
            };


            $scope.$on('verifing', function (scope, params) {
                for (var i = 0; i < $scope.categoriesList.length; i++) {
                    var categories = $scope.categoriesList[i];
                    if (categories.form.$dirty) {
                        $scope.saveCategories(categories);
                    }
                }
            });

            $scope.cancel = function (categories) {
                dataBind(categories, categories.checkpoint || {});
                categories.form.$setPristine();
            };

            $scope.deleteCategories = function(index) {
                var categories = $scope.categoriesList[index];
                var categoriesName = categories.name || $i18next.t('SideBarCategoriesNoName');
                
                var trimmedCategoriesName = utilityService.trimText(categoriesName);
                var message = $i18next.t('DeleteConfirmCategories', { trimmedTitle: trimmedCategoriesName });

                var modalInstance =
                    confirmService.open(utilityService.createDeletePopup(message));

                modalInstance.result.then(function(confirmResult) {
                    if (confirmResult === 'ok') {
                        commandService.deleteCategories($state.params.questionnaireId, categories.categoriesId).then(
                            function() {
                                $scope.categoriesList.splice(index, 1);

                                updateQuestionnaireCategories();
                            });
                    }
                });
            };

            $scope.editCategories = function (questionnaireId, categoriesId)
            {
                $scope.shouldUserSeeReloadPromt = true;
                $scope.openEditor = categoriesId

                window.open("../../questionnaire/editcategories/" + questionnaireId + "?categoriesid=" + categoriesId,
                    "", "scrollbars=yes, center=yes, modal=yes, width=960, height=745, top=" + (screen.height - 745) / 4 
                    + ", left= " + (screen.width - 960) / 2, true);
            }

            $scope.foldback = function () {
                $scope.isFolded = false;
                $rootScope.$broadcast("closeCategories", {});
            };

            $scope.$on('openCategories', function (scope, params) {
                $scope.unfold();
                if (!_.isUndefined(params) && !_.isUndefined(params.focusOn)) {
                    setTimeout(function () { utilityService.focus("focusCategories" + params.focusOn); }, 500);
                }
            });

            $scope.$on('closeCategoriesRequested', function () {
                $scope.foldback();
            });

            $rootScope.$on('questionnaireLoaded', function () {
                $scope.loadCategories();
            });

            var updateQuestionnaireCategories = function (categories) {
                if ($scope.categoriesList === null)
                    return;

                $scope.questionnaire.categories = _.map($scope.categoriesList,
                    function(categoriesDto) {
                        return { categoriesId: categoriesDto.categoriesId, name: categoriesDto.name };
                    });

                $rootScope.$broadcast("updateCategories", categories || {});
            };
        });
