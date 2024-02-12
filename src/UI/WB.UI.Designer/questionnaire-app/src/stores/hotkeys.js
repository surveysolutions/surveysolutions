import { defineStore } from 'pinia';
import { i18n } from '../plugins/localization';

export const useHotkeysStore = defineStore('hotkeys', {
    state: () => ({
        hotkeys: [
            {
                keys: ['Ctrl + Shift + ?'],
                description: i18n.t('QuestionnaireEditor.HotkeysShowHideHelp')
            },
            {
                keys: ['Ctrl + Shift + s'],
                description: i18n.t('QuestionnaireEditor.Save')
            },
            {
                keys: ['Ctrl + Shift + f'],
                description: i18n.t('QuestionnaireEditor.HotkeysSearch')
            },
            {
                keys: ['Ctrl + Shift + h'],
                description: i18n.t('QuestionnaireEditor.FindReplaceTitle')
            },
            {
                keys: ['Ctrl + Shift + p'],
                description: i18n.t('QuestionnaireEditor.HotkeysPrint')
            },
            {
                keys: ['Ctrl + b'],
                description: i18n.t('QuestionnaireEditor.Compile')
            },
            {
                keys: ['Shift + Alt + x'],
                description: i18n.t('QuestionnaireEditor.HotkeysFocusTree')
            },
            {
                keys: ['Shift + Alt + e'],
                description: i18n.t('QuestionnaireEditor.HotkeysFocusTitle')
            },
            {
                keys: ['arrowleft'],
                description: i18n.t('QuestionnaireEditor.HotkeysOpenSection')
            },
            {
                keys: ['arrowright'],
                description: i18n.t('QuestionnaireEditor.HotkeysHideSections')
            },
            {
                keys: ['Ctrl + r'],
                description: i18n.t('QuestionnaireEditor.HotkeysCloseScenarios')
            },
            {
                keys: ['Ctrl + m'],
                description: i18n.t('QuestionnaireEditor.HotkeysCloseMacros')
            },
            {
                keys: ['Ctrl + l'],
                description: i18n.t('QuestionnaireEditor.HotkeysCloseLookup')
            },
            {
                keys: ['Ctrl + Shift + a'],
                description: i18n.t(
                    'QuestionnaireEditor.HotkeysHideAttachments'
                )
            },
            {
                keys: ['Ctrl + Shift + t'],
                description: i18n.t(
                    'QuestionnaireEditor.HotkeysCloseTranslations'
                )
            },
            {
                keys: ['Ctrl + Shift + c'],
                description: i18n.t(
                    'QuestionnaireEditor.HotkeysCloseCategories'
                )
            },
            {
                keys: ['Ctrl + i'],
                description: i18n.t('QuestionnaireEditor.HotkeysCloseMetadata')
            },
            {
                keys: ['Ctrl + Alt + c'],
                description: i18n.t('QuestionnaireEditor.HotkeysCloseComments')
            },
            {
                keys: ['arrowdown'],
                description: i18n.t(
                    'QuestionnaireEditor.HotkeysNavigateToSibling'
                )
            },
            {
                keys: ['arrowup'],
                description: i18n.t(
                    'QuestionnaireEditor.HotkeysNavigateToPrevSibling'
                )
            },
            {
                keys: ['enter'],
                description: i18n.t('QuestionnaireEditor.HotkeysOpenItem')
            }
        ]
    }),
    getters: {
        getHotkeys: state => state.hotkeys
    },
    actions: {}
});
