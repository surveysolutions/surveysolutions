import { onMounted, onUnmounted } from 'vue';
import { useRosterStore } from './roster';
import { useGroupStore } from './group';
import { useQuestionStore } from './question';
import { useStaticTextStore } from './staticText';
import { useVariableStore } from './variable';
import { useQuestionnaireStore } from './questionnaire';
import { i18n } from '../plugins/localization';
import { useRoute } from 'vue-router';

export const useUnsavedChanges = () => {
    const route = useRoute();
    const rosterStore = useRosterStore();
    const groupStore = useGroupStore();
    const questionStore = useQuestionStore();
    const staticTextStore = useStaticTextStore();
    const variableStore = useVariableStore();
    const questionnaireStore = useQuestionnaireStore();

    const getUnsavedChanges = routeName => {
        if (routeName == 'roster') {
            return rosterStore.getIsDirty;
        }
        if (routeName == 'group') {
            return groupStore.getIsDirty;
        }
        if (routeName == 'question') {
            return questionStore.getIsDirty;
        }
        if (routeName == 'statictext') {
            return staticTextStore.getIsDirty;
        }
        if (routeName == 'variable') {
            return variableStore.getIsDirty;
        }
        return false;
    };

    const confirmLeave = () => {
        return window.confirm(
            i18n.t('QuestionnaireEditor.UnsavedChangesLeave')
        );
    };

    const beforeUnloadListener = event => {
        const isReadOnlyForUser = questionnaireStore.info.isReadOnlyForUser;
        if (isReadOnlyForUser) return;

        const isDirty = getUnsavedChanges(route.name);
        if (isDirty && !confirmLeave()) {
            event.preventDefault();
            event.returnValue = ''; //for chrome
        }
        return null;
    };

    onMounted(() => {
        window.addEventListener('beforeunload', beforeUnloadListener);
    });

    onUnmounted(() => {
        window.removeEventListener('beforeunload', beforeUnloadListener);
    });

    return { getUnsavedChanges, confirmLeave };
};
