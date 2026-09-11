import { defineStore } from 'pinia';
import { i18n } from '../plugins/localization';

const hotkey = (keys, i18nKey) => ({
    keys: Array.isArray(keys) ? keys : [keys],
    description: i18n.t(`QuestionnaireEditor.${i18nKey}`)
});

export const useHotkeysStore = defineStore('hotkeys', {
    state: () => ({
        hotkeys: [
            hotkey('?',          'HotkeysShowHideHelp'),
            hotkey('Ctrl + s',   'Save'),
            hotkey('Ctrl + f',   'HotkeysSearch'),
            hotkey('Ctrl + h',   'FindReplaceTitle'),
            hotkey('Ctrl + p',   'HotkeysPrint'),
            hotkey('Ctrl + b',   'Compile'),
            hotkey('Ctrl + i',   'HotkeysTest')
        ]
    }),
    getters: {
        getHotkeys: state => state.hotkeys
    },
    actions: {}
});