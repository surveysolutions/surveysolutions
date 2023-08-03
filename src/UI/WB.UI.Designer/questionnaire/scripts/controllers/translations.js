angular.module('designerApp')
    .controller('TranslationsCtrl',
        function ($rootScope, $scope, $state, $i18next, hotkeys, commandService, utilityService, confirmService, Upload, $uibModal, notificationService, moment, shareService) {
            'use strict';

            $scope.downloadBaseUrl = '../../translations';
            $scope.isReadOnlyForUser = false;

            var hideTranslationsPane = 'ctrl+shift+t';

            if (hotkeys.get(hideTranslationsPane) !== false) {
                hotkeys.del(hideTranslationsPane);
            }

            hotkeys.add(hideTranslationsPane, $i18next.t('HotkeysCloseTranslations'), function (event) {
                event.preventDefault();
                $scope.foldback();
            });

            $scope.translations = [];

            var dataBind = function (translation, translationDto) {
                translation.translationId = translationDto.translationId;
                translation.name = translationDto.name;
                translation.file = null;
                translation.isDefault = translationDto.isDefault;
                translation.content = {};
                translation.content.details = {};
                translation.downloadUrl = $scope.downloadBaseUrl + '/' + $scope.questionnaire.questionnaireId + '/xlsx/' + translationDto.translationId;
                translation.isOriginalTranslation = false;

                if (!_.isUndefined(translationDto.content) && !_.isNull(translationDto.content)) {
                    translation.content.size = translationDto.content.size;
                    translation.content.type = translationDto.content.contentType;

                    if (!_.isUndefined(translationDto.content.details) && !_.isNull(translationDto.content.details)) {
                        translation.content.details.height = translationDto.content.details.height;
                        translation.content.details.width = translationDto.content.details.width;
                    }
                }
            };

            $scope.onSave = function ($event, translation) {
                if (!translation.isOriginalTranslation) {
                    $scope.saveTranslation(translation);
                    $event.stopPropagation();
                }
                else {
                    shareService.udpateQuestionnaire(
                        $scope.questionnaire.questionnaireId,
                        $scope.questionnaire.title,
                        $scope.questionnaire.variable,
                        $scope.questionnaire.editedHideIfDisabled,
                        $scope.questionnaire.isPublic,
                        translation.name
                    ).then(function () {
                        translation.checkpoint.name = translation.name;
                        translation.form.$setPristine();
                    });

                    $event.stopPropagation();
                }
            };
            $scope.onCancel = function ($event, translation) {
                if (!translation.isOriginalTranslation) {
                    $scope.cancel(translation);
                    $event.stopPropagation();
                }
                else {
                    translation.name = translation.checkpoint.name;
                    translation.form.$setPristine();

                    $event.stopPropagation();
                }
            }

            $scope.loadTranslations = function () {
                if ($scope.questionnaire === null)
                    return;
                
                $scope.translations.splice(0, $scope.translations.length)

                var defaultTranslation = {
                    translationId: null,
                    name: !$scope.questionnaire.defaultLanguageName ? $i18next.t("Translation_Original") : $scope.questionnaire.defaultLanguageName,
                    file: null,
                    isDefault: !_.any($scope.questionnaire.translations, { isDefault: true }),
                    content: { details: {} },
                    isOriginalTranslation: true
                };
                defaultTranslation.checkpoint = { name: defaultTranslation.name };

                $scope.translations.push(defaultTranslation);

                $scope.isReadOnlyForUser = $scope.questionnaire.isReadOnlyForUser || false;

                if ($scope.questionnaire.translations === null)
                    return;

                _.each($scope.questionnaire.translations, function (translationDto) {
                    var translation = { checkpoint: {} };
                    if (!_.any($scope.translations, function (elem) { return elem.translationId === translationDto.translationId; })) {
                        dataBind(translation, translationDto);
                        dataBind(translation.checkpoint, translationDto);
                        $scope.translations.push(translation);
                    }
                });
            };


            $scope.isFolded = false;

            $scope.unfold = function () {
                $scope.isFolded = true;
            };

            $scope.createAndUploadFile = function (file) {
                if (_.isNull(file) || _.isUndefined(file)) {
                    return;
                }

                if ($scope.isReadOnlyForUser) {
                    notificationService.notice($i18next.t('NoPermissions'));
                    return;
                }

                
                var translation = { 
                };

                $scope.fileSelected(translation, file, function () {
                    commandService.updateTranslation($state.params.questionnaireId, translation).then(function () {
                        translation.downloadUrl = $scope.downloadBaseUrl + '/' + $scope.questionnaire.questionnaireId + '/xlsx/' + translation.translationId
                        translation.checkpoint = translation.checkpoint || {};

                        dataBind(translation.checkpoint, translation);
                        $scope.translations.push(translation);
                        setTimeout(function () { utilityService.focus("focusTranslation" + translation.translationId); }, 500);
                    });
                });
            };

            $scope.fileSelected = function (translation, file, callback) {
                if (_.isUndefined(file) || _.isNull(file)) {
                    return;
                }

                translation.file = file;

                translation.content = {};
                translation.content.size = file.size;
                translation.content.type = file.type;

                translation.meta = {};
                translation.meta.fileName = file.name;
                translation.meta.lastUpdated = moment();

                var maxNameLength = 32;

                var suspectedTranslations = translation.meta.fileName.match(/[^[\]]+(?=])/g);

                if (suspectedTranslations && suspectedTranslations.length > 0)
                    translation.name = suspectedTranslations[0];
                else
                    translation.name = translation.meta.fileName.replace(/\.[^/.]+$/, "");

                var fileNameLength = translation.name.length;
                translation.name = translation.name.substring(0, fileNameLength < maxNameLength ? fileNameLength : maxNameLength);
                translation.oldTranslationId = translation.translationId;
                translation.translationId = utilityService.guid();

                if (!_.isUndefined(translation.form)) {
                    translation.form.$setDirty();
                }

                if (!_.isUndefined(callback)) {
                    callback();
                }
            };

            $scope.saveTranslation = function (translation) {
                commandService.updateTranslation($state.params.questionnaireId, translation).then(function () {
                    dataBind(translation.checkpoint, translation);
                    translation.form.$setPristine();
                    $rootScope.$broadcast('translationChanged', { 
                        newTranslationId: translation.translationId, 
                        oldTranslationId: translation.oldTranslationId
                    }) 
                });
            };


            $scope.$on('verifing', function (scope, params) {
                for (var i = 0; i < $scope.translations.length; i++) {
                    var translations = $scope.translations[i];
                    if (translations.form.$dirty) {
                        $scope.saveTranslation(translations);
                    }
                }
            });

            $scope.cancel = function (translation) {
                dataBind(translation, translation.checkpoint || {});
                translation.form.$setPristine();
            };

            $scope.deleteTranslation = function (index) {
                var translation = $scope.translations[index];
                var translationName = translation.name || $i18next.t('SideBarTranslationNoName');
                var modalInstance = confirmService.open(utilityService.createQuestionForDeleteConfirmationPopup(translationName));

                modalInstance.result.then(function (confirmResult) {
                    if (confirmResult === 'ok') {
                        commandService.deleteTranslation($state.params.questionnaireId, translation.translationId).then(function () {
                            $scope.translations.splice(index, 1);
                            $rootScope.$broadcast('translationRemoved', { translationId: translation.translationId })
                        });
                    }
                });
            };

            $scope.getDefaultTemplate = function () {
            };

            $scope.setDefaultTranslation = function (translationIndex, isDefault) {
                var translation = $scope.translations[translationIndex];

                commandService.setDefaultTranslation($state.params.questionnaireId, isDefault ? translation.translationId : null).then(function () {
                    _.each($scope.translations, function (translation) {
                        translation.isDefault = translation.checkpoint.isDefault = false;
                    });
                    translation.isDefault = translation.checkpoint.isDefault = isDefault;
                });
            };

            $scope.foldback = function () {
                $scope.isFolded = false;
                $rootScope.$broadcast("closeTranslations", {});
            };

            $scope.$on('openTranslations', function (scope, params) {
                $scope.unfold();
                if (!_.isUndefined(params) && !_.isUndefined(params.focusOn)) {
                    setTimeout(function () { utilityService.focus("focusTranslation" + params.focusOn); }, 500);
                }
            });

            $scope.$on('closeTranslationsRequested', function () {
                $scope.foldback();
            });

            $rootScope.$on('questionnaireLoaded', function () {
                $scope.loadTranslations();
            });
        });
