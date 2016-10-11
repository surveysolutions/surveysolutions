angular.module('designerApp')
    .controller('StaticTextCtrl',
        function ($rootScope, $scope, $state, utilityService, questionnaireService, commandService, hotkeys) {
            "use strict";

            $scope.currentChapterId = $state.params.chapterId;

            var saveStaticText = 'ctrl+s';

            if (hotkeys.get(saveStaticText) !== false) {
                hotkeys.del(saveStaticText);
            }

            var markFormAsChanged = function () {
                if ($scope.staticTextForm) {
                    $scope.staticTextForm.$setDirty();
                }
            }
            
            hotkeys.bindTo($scope)
                .add({
                    combo: saveStaticText,
                    description: 'Save changes',
                    allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                    callback: function(event) {
                        if ($scope.questionnaire !== null && !$scope.questionnaire.isReadOnlyForUser) {
                            $scope.saveStaticText();
                            $scope.staticTextForm.$setPristine();
                            event.preventDefault();
                        }
                    }
                });
            
            var dataBind = function (result) {
                $scope.activeStaticText = $scope.activeStaticText || {};
                $scope.activeStaticText.breadcrumbs = result.breadcrumbs;
                $scope.activeStaticText.itemId = $state.params.itemId;
                $scope.activeStaticText.text = result.text;
                $scope.activeStaticText.attachmentName = result.attachmentName;

                $scope.activeStaticText.enablementCondition = result.enablementCondition;
                $scope.activeStaticText.hideIfDisabled = result.hideIfDisabled;
                $scope.activeStaticText.validationConditions = result.validationConditions;

                if (!_.isNull($scope.staticTextForm) && !_.isUndefined($scope.staticTextForm)) {
                    $scope.staticTextForm.$setPristine();
                }
            };

            $scope.loadStaticText = function () {
                questionnaireService.getStaticTextDetailsById($state.params.questionnaireId, $state.params.itemId)
                    .success(function (result) {
                        $scope.initialStaticText = angular.copy(result);
                        dataBind(result);
                        utilityService.scrollToValidationCondition($state.params.validationIndex);

                        var focusId = null;
                        switch ($state.params.property) {
                            case 'Title':
                                focusId = 'edit-static-text';
                                break;
                            case 'EnablingCondition':
                                focusId = 'edit-question-enablement-condition';
                                break;
                            case 'ValidationExpression':
                                focusId = 'validation-expression-' + $state.params.validationIndex;
                                break;
                            case 'ValidationMessage':
                                focusId = 'validation-message-' + $state.params.validationIndex;
                                break;
                            default:
                                break;
                        }

                        utilityService.setFocusIn(focusId);
                    });
            };

            var hasEnablementConditions = function (staticText) {
                return staticText.enablementCondition !== null &&
                    /\S/.test(staticText.enablementCondition);
            };

            var hasValidations = function (staticText) {
                return staticText.validationConditions.length > 0;
            };

            $scope.saveStaticText = function (callback) {
                commandService.updateStaticText($state.params.questionnaireId, $scope.activeStaticText).success(function () {
                    $scope.initialStaticText = angular.copy($scope.activeStaticText);
                    $rootScope.$emit('staticTextUpdated', {
                        itemId: $scope.activeStaticText.itemId,
                        text: $scope.activeStaticText.text,

                        hasCondition: hasEnablementConditions($scope.activeStaticText),
                        hasValidation: hasValidations($scope.activeStaticText),
                        hideIfDisabled: $scope.activeStaticText.hideIfDisabled
                    });
                    if (_.isFunction(callback)) {
                        callback();
                    }
                });
            };

            $scope.removeValidationCondition = function (index) {
                $scope.activeStaticText.validationConditions.splice(index, 1);
                markFormAsChanged();
            }

            $scope.addValidationCondition = function () {
                $scope.activeStaticText.validationConditions.push({
                    expression: '',
                    message: ''
                });
                markFormAsChanged();
                _.defer(function () {
                    $(".static-text-editor .form-holder").scrollTo({ top: '+=200px', left: "+=0" }, 250);
                });
            }

            $scope.$on('verifing', function (scope, params) {
                if ($scope.staticTextForm.$dirty)
                    $scope.saveStaticText(function() {
                        $scope.staticTextForm.$setPristine();
                    });
            });

            $scope.cancelStaticText = function () {
                var temp = angular.copy($scope.initialStaticText);
                dataBind(temp);
            };

            $scope.loadStaticText();
        }
    );
