import { i18n } from '../plugins/localization';
import { nextTick } from 'vue';
import { defer, isNull } from 'lodash';

export function format(format) {
    var args = Array.prototype.slice.call(arguments, 1);
    return format.replace(/{(\d+)}/g, function(match, number) {
        return typeof args[number] !== 'undefined' ? args[number] : match;
    });
}

export function guid() {
    function s4() {
        return Math.floor((1 + Math.random()) * 0x10000)
            .toString(16)
            .substring(1);
    }

    return s4() + s4() + s4() + s4() + s4() + s4() + s4() + s4();
}

export function focus(name) {
    $timeout(function() {
        $rootScope.$broadcast('focusOn', name);
    });
}

export function setFocusIn(elementId) {
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
        }, 300);
    }
}

export function focusout(name) {
    $timeout(function() {
        $rootScope.$broadcast('focusOut', name);
    });
}

export async function moveFocusAndAddOptionIfNeeded(
    targetDomElement,
    optionEditorClassName,
    optionVauleEditorClassName,
    options,
    addOptionCallBack,
    optionPropertyName
) {
    var target = targetDomElement;
    if (target.parents(optionEditorClassName).length <= 0) {
        return;
    }

    var optionScope = angular.element(target).scope()[optionPropertyName];
    var indexOfOption = options.indexOf(optionScope);
    if (indexOfOption < 0) return;

    if (indexOfOption === options.length - 1) {
        addOptionCallBack();
    }

    await nextTick();

    var questionOptionValueEditor = $(optionVauleEditorClassName);
    var optionValueInput = $(questionOptionValueEditor[indexOfOption + 1]);
    optionValueInput.focus();
    optionValueInput.select();
}

export function sanitize(input) {
    if (input) {
        var html = filterXSS(input, {
            whiteList: [], // empty, means filter out all tags
            stripIgnoreTag: true // filter out all HTML not in the whilelist
        });
        return html;
    }
    return input || '';
}

function filterXSS(input) {
    return input; // TODO need filterXSS lib
}

export function trimText(text) {
    return sanitize(text).substring(0, 50) + (text.length > 50 ? '...' : '');
}

export function createDeletePopup(message) {
    return {
        title: message,
        okButtonTitle: i18n.t('QuestionnaireEditor.Delete'),
        cancelButtonTitle: i18n.t('QuestionnaireEditor.Cancel')
    };
}

export function createQuestionForDeleteConfirmationPopup(title) {
    var trimmedTitle = trimText(title);
    var message = i18n.t('QuestionnaireEditor.DeleteConfirmQuestion', {
        trimmedTitle: trimmedTitle,
        interpolation: { escapeValue: false }
    });
    return {
        title: message,
        okButtonTitle: i18n.t('QuestionnaireEditor.Delete'),
        cancelButtonTitle: i18n.t('QuestionnaireEditor.Cancel')
    };
}

export function replaceOptionsConfirmationPopup(title) {
    var trimmedTitle = trimText(title);
    var message = i18n.t('QuestionnaireEditor.ReplaceOptionsConfirmation', {
        trimmedTitle: trimmedTitle,
        interpolation: { escapeValue: false }
    });
    return {
        title: message,
        okButtonTitle: i18n.t('QuestionnaireEditor.Yes'),
        cancelButtonTitle: i18n.t('QuestionnaireEditor.No')
    };
}

export function willBeTakenOnlyFirstOptionsConfirmationPopup(title, count) {
    var trimmedTitle = trimText(title);
    var message = i18n.t(
        'QuestionnaireEditor.OnlyFirstOptionsWillBeTakenConfirmation',
        {
            trimmedTitle: trimmedTitle,
            count: count,
            interpolation: { escapeValue: false }
        }
    );
    return {
        title: message,
        okButtonTitle: i18n.t('QuestionnaireEditor.Yes'),
        cancelButtonTitle: i18n.t('QuestionnaireEditor.No')
    };
}

export function createEmptyGroup(parent) {
    var newId = guid();
    var emptyGroup = {
        itemId: newId,
        title: i18n.t('QuestionnaireEditor.DefaultNewSubsection'),
        items: [],
        itemType: 'Group',
        hasCondition: false,
        getParentItem: function() {
            return parent;
        }
    };
    return emptyGroup;
}

export function createEmptyRoster(parent) {
    var newId = guid();
    var emptyRoster = {
        itemId: newId,
        title:
            i18n.t('QuestionnaireEditor.DefaultNewRoster') + ' - %rostertitle%',
        items: [],
        itemType: 'Group',
        hasCondition: false,
        isRoster: true,
        getParentItem: function() {
            return parent;
        }
    };
    return emptyRoster;
}

export function createEmptyQuestion(parent) {
    var newId = guid();
    var emptyQuestion = {
        itemId: newId,
        title: '',
        type: 'Text',
        itemType: 'Question',
        hasCondition: false,
        hasValidation: false,
        getParentItem: function() {
            return parent;
        }
    };
    return emptyQuestion;
}

export function createEmptyStaticText(parent) {
    var newId = guid();
    var emptyStaticText = {
        itemId: newId,
        text: i18n.t('QuestionnaireEditor.DefaultNewStaticText'),
        itemType: 'StaticText',
        hasCondition: false,
        hasValidation: false,
        getParentItem: function() {
            return parent;
        }
    };
    return emptyStaticText;
}

export function createEmptyVariable(parent) {
    var newId = guid();
    var emptyVariable = {
        itemId: newId,
        itemType: 'Variable',
        variableData: {},
        getParentItem: function() {
            return parent;
        }
    };
    return emptyVariable;
}

export function isTreeItemVisible(item) {
    var viewport = {
        top: $('.questionnaire-tree-holder > .scroller').offset().top,
        bottom: $(window).height()
    };
    var top = item.offset().top;
    var bottom = top + item.outerHeight();

    var isTopBorderVisible = top > viewport.top && top < viewport.bottom;
    var isBottomBorderVisible =
        bottom > viewport.top && bottom < viewport.bottom;
    var distanceToTopBorder = Math.abs(top - viewport.top);
    var distanceToBottomBorder = Math.abs(bottom - viewport.bottom);

    return {
        isVisible: isTopBorderVisible && isBottomBorderVisible,
        shouldScrollDown: distanceToTopBorder < distanceToBottomBorder,
        scrollPositionWhenScrollUp:
            viewport.top -
            $('.question-list').offset().top +
            distanceToBottomBorder
    };
}

export function scrollToValidationCondition(conditionIndex) {
    if (!isNull(conditionIndex)) {
        defer(function() {
            document
                .querySelector('.question-editor .form-holder')
                .scrollTo('#validationCondition' + conditionIndex, 500, {
                    easing: 'swing',
                    offset: -10
                });
        });
    }
}

export function scrollToElement(parent, id) {
    if (isNull(parent) || isNull(id)) return;

    defer(function() {
        document
            .querySelector(parent)
            .scrollTo(id, 500, { easing: 'swing', offset: -10 });
    });
}
