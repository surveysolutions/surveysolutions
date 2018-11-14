angular.module('designerApp')
    .factory('utilityService',
        function ($rootScope, $timeout, $i18next) {
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

            utilityService.sanitize = function(input) {
                if (input) {
                    var html = filterXSS(input, {
                        whiteList: [],        // empty, means filter out all tags
                        stripIgnoreTag: true      // filter out all HTML not in the whilelist
                    });
                    return html;
                }
                return input || '';
            };

            utilityService.createQuestionForDeleteConfirmationPopup = function (title) {
                var trimmedTitle = utilityService.sanitize(title).substring(0, 50) + (title.length > 50 ? "..." : "");
                var message = $i18next.t('DeleteConfirmQuestion',  {trimmedTitle: trimmedTitle});
                return {
                    title: message,
                    okButtonTitle: $i18next.t("Delete"),
                    cancelButtonTitle: $i18next.t("Cancel")
                };
            };

            utilityService.replaceOptionsConfirmationPopup = function (title) {
                var trimmedTitle = utilityService.sanitize(title).substring(0, 50) + (title.length > 50 ? "..." : "");
                var message = $i18next.t('ReplaceOptionsConfirmation',  {trimmedTitle: trimmedTitle});
                return {
                    title: message,
                    okButtonTitle: $i18next.t("Yes"),
                    cancelButtonTitle: $i18next.t("No")
                };
            };

            utilityService.createEmptyGroup = function (parent) {
                var newId = utilityService.guid();
                var emptyGroup = {
                    "itemId": newId,
                    "title": $i18next.t('DefaultNewSubsection'),
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
                    "title": $i18next.t('DefaultNewRoster'),
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
                    "text": $i18next.t('DefaultNewStaticText'),
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

            utilityService.scrollToElement = function(parent, id) {
                if (_.isNull(parent) || _.isNull(id))
                    return;
                
                _.defer(function () {
                    $(parent).scrollTo(id, 500, { easing: 'swing', offset: -10 });
                });
            }

            return utilityService;
        }
    );
