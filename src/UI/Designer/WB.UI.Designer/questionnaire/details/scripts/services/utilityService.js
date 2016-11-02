﻿(function () {
    angular.module('designerApp')
        .factory('utilityService', [
            '$rootScope', '$timeout',
            function ($rootScope, $timeout) {
                var utilityService = {};

                utilityService.guid = function () {
                    function s4() {
                        return Math.floor((1 + Math.random()) * 0x10000)
                            .toString(16)
                            .substring(1);
                    }

                    return s4() + s4() + s4() + s4() +
                        s4() + s4() + s4() + s4();
                };

                utilityService.format = function (format) {
                    var args = Array.prototype.slice.call(arguments, 1);
                    return format.replace(/{(\d+)}/g, function (match, number) {
                        return typeof args[number] !== 'undefined'
                            ? args[number]
                            : match;
                    });
                };

                utilityService.focus = function (name) {
                    $timeout(function () {
                        $rootScope.$broadcast('focusOn', name);
                    });
                };

                utilityService.setFocusIn = function (elementId) {
                    if (elementId) {
                        $timeout(function() {
                                var element = angular.element('#' + elementId);

                                if (!_.isNull(element) && !_.isUndefined(element)) {

                                    if (element.attr('ui-ace')) {
                                        var edit = ace.edit(element.attr('id'));
                                        edit.focus();
                                        edit.navigateFileEnd();
                                    } else {
                                        element.focus();
                                    }
                                }
                            },
                            300);
                    }
                }
                
                utilityService.focusout = function (name) {
                    $timeout(function () {
                        $rootScope.$broadcast('focusOut', name);
                    });
                };
                
                utilityService.moveFocusAndAddOptionIfNeeded = function (targetDomElement, optionEditorClassName, optionVauleEditorClassName, options, addOptionCallBack, optionPropertyName) {

                    var target = $(targetDomElement);
                    if (target.parents(optionEditorClassName).length <= 0) {
                        return;
                    }

                    var optionScope = angular.element(target).scope()[optionPropertyName];
                    var indexOfOption = options.indexOf(optionScope);
                    if (indexOfOption < 0)
                        return;

                    if (indexOfOption === options.length - 1)
                        addOptionCallBack();

                    $timeout(function () {
                        var questionOptionValueEditor = $(optionVauleEditorClassName);
                        var optionValueInput = $(questionOptionValueEditor[indexOfOption + 1]);
                        optionValueInput.focus();
                        optionValueInput.select();
                    });
                };

                utilityService.createQuestionForDeleteConfirmationPopup = function (title) {
                    var trimmedTitle = title.substring(0, 25) + (title.length > 25 ? "..." : "");
                    var message = 'Are you sure you want to delete "' + trimmedTitle + '"?';
                    return {
                        title: message,
                        okButtonTitle: "DELETE",
                        cancelButtonTitle: "BACK TO DESIGNER"
                    };
                };

                utilityService.createEmptyGroup = function (parent) {
                    var newId = utilityService.guid();
                    var emptyGroup = {
                        "itemId": newId,
                        "title": "New sub-section",
                        "items": [],
                        itemType: 'Group',
                        hasCondition:false,
                        getParentItem: function () { return parent; }
                    };
                    return emptyGroup;
                };

                utilityService.createEmptyRoster = function (parent) {
                    var newId = utilityService.guid();
                    var emptyRoster = {
                        "itemId": newId,
                        "title": "New roster",
                        "items": [],
                        itemType: 'Group',
                        hasCondition: false,
                        isRoster: true,
                        getParentItem: function () { return parent; }
                    };
                    return emptyRoster;
                };

                utilityService.createEmptyQuestion = function (parent) {
                    var newId = utilityService.guid();
                    var emptyQuestion = {
                        "itemId": newId,
                        "title": '',
                        "type": 'Text',
                        itemType: 'Question',
                        hasCondition: false,
                        hasValidation: false,
                        getParentItem: function () { return parent; }
                    };
                    return emptyQuestion;
                };

                utilityService.createEmptyStaticText = function (parent) {
                    var newId = utilityService.guid();
                    var emptyStaticText = {
                        "itemId": newId,
                        "text": "New static text",
                        itemType: 'StaticText',
                        hasCondition: false,
                        hasValidation: false,
                        getParentItem: function () { return parent; }
                    };
                    return emptyStaticText;
                };

                utilityService.createEmptyVariable = function (parent) {
                    var newId = utilityService.guid();
                    var emptyVariable = {
                        itemId: newId,
                        itemType: 'Variable',
                        variableData: {},
                        getParentItem: function () { return parent; }
                    };
                    return emptyVariable;
                };

                utilityService.isTreeItemVisible = function (item) {
                    var viewport = {
                        top: $(".questionnaire-tree-holder > .scroller").offset().top,
                        bottom: $(window).height()
                    };
                    var top = item.offset().top;
                    var bottom = top + item.outerHeight();

                    var isTopBorderVisible = top > viewport.top && top < viewport.bottom;
                    var isBottomBorderVisible = bottom > viewport.top && bottom < viewport.bottom;
                    var distanceToTopBorder = Math.abs(top - viewport.top);
                    var distanceToBottomBorder = Math.abs(bottom - viewport.bottom);

                    return {
                        isVisible: isTopBorderVisible && isBottomBorderVisible,
                        shouldScrollDown: distanceToTopBorder < distanceToBottomBorder,
                        scrollPositionWhenScrollUp: viewport.top - $(".question-list").offset().top + distanceToBottomBorder
                    };
                };

                utilityService.scrollToValidationCondition = function(conditionIndex) {
                    if (!_.isNull(conditionIndex)) {
                        _.defer(function () {
                            $(".question-editor .form-holder").scrollTo("#validationCondition" + conditionIndex, 500, {
                                easing: 'swing',
                                offset: -10
                            });
                        });
                    }
                }

                return utilityService;
            }
        ]);
})();