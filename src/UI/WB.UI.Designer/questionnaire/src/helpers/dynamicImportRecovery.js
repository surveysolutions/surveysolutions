import { useRosterStore } from '../stores/roster';
import { useGroupStore } from '../stores/group';
import { useQuestionStore } from '../stores/question';
import { useStaticTextStore } from '../stores/staticText';
import { useVariableStore } from '../stores/variable';
import { i18n } from '../plugins/localization';

const DYNAMIC_IMPORT_RETRY_KEY = 'dynamic-import-retry-count';
const MAX_DYNAMIC_IMPORT_RETRIES = 2;
const dynamicImportErrorPatterns = [
    'failed to fetch dynamically imported module',
    'error loading dynamically imported module',
    'importing a module script failed',
    'unable to preload css for'
];
let recoveryScheduled = false;

export function isDynamicImportError(error) {
    const message = (error instanceof Error ? error.message : String(error)).toLowerCase();
    return dynamicImportErrorPatterns.some(pattern => message.includes(pattern));
}

function delayDynamicImportRetry(retryCount) {
    const delay = 300 * 2 ** retryCount + Math.floor(Math.random() * 200);
    return new Promise(resolve => window.setTimeout(resolve, delay));
}

function getRetryCount() {
    try {
        const value = Number(window.sessionStorage.getItem(DYNAMIC_IMPORT_RETRY_KEY) ?? 0);
        return Number.isFinite(value) ? value : 0;
    } catch {
        return null;
    }
}

function setRetryCount(count) {
    try {
        window.sessionStorage.setItem(DYNAMIC_IMPORT_RETRY_KEY, String(count));
        return true;
    } catch {
        return false;
    }
}

function clearRetryCount() {
    try {
        window.sessionStorage.removeItem(DYNAMIC_IMPORT_RETRY_KEY);
    } catch {
        return;
    }
}

function hasUnsavedChanges(routeName) {
    const dirtyStates = {
        roster: useRosterStore().getIsDirty,
        group: useGroupStore().getIsDirty,
        question: useQuestionStore().getIsDirty,
        statictext: useStaticTextStore().getIsDirty,
        variable: useVariableStore().getIsDirty
    };

    if (routeName == null) {
        return Object.values(dirtyStates).some(Boolean);
    }

    switch (routeName) {
        case 'roster':
            return dirtyStates.roster;
        case 'group':
            return dirtyStates.group;
        case 'question':
            return dirtyStates.question;
        case 'statictext':
            return dirtyStates.statictext;
        case 'variable':
            return dirtyStates.variable;
        default:
            return false;
    }
}

export function scheduleDynamicImportRecovery(error, routeName) {
    if (recoveryScheduled) return;

    const retryCount = getRetryCount();

    if (retryCount === null) {
        console.error('Dynamic import recovery skipped: session storage is unavailable.', error);
        return;
    }

    if (retryCount >= MAX_DYNAMIC_IMPORT_RETRIES) {
        console.error('Dynamic import retry budget exhausted:', error);
        return;
    }

    recoveryScheduled = true;

    if (hasUnsavedChanges(routeName)) {
        const confirmed = window.confirm(
            i18n.t('QuestionnaireEditor.UnsavedChangesLeave')
        );
        if (!confirmed) {
            recoveryScheduled = false;
            return;
        }
    }

    if (!setRetryCount(retryCount + 1)) {
        recoveryScheduled = false;
        console.error('Dynamic import recovery skipped: session storage is unavailable.', error);
        return;
    }

    delayDynamicImportRetry(retryCount).then(() => window.location.reload());
}

export function clearDynamicImportRecovery() {
    clearRetryCount();
    recoveryScheduled = false;
}

export function wrapDynamicImport(loader, getRouteName = () => null) {
    return () =>
        loader().catch(error => {
            if (isDynamicImportError(error)) {
                scheduleDynamicImportRecovery(error, getRouteName());
            }

            throw error;
        });
}
