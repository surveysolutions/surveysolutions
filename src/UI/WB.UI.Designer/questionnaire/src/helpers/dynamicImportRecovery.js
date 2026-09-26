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
let activeRouteName = null;
let recoveryTimeoutId = null;

export function isDynamicImportError(error) {
    const message = (error instanceof Error ? error.message : String(error)).toLowerCase();
    return dynamicImportErrorPatterns.some(pattern => message.includes(pattern));
}

function getDynamicImportRetryDelay(retryCount) {
    return 300 * 2 ** retryCount + Math.floor(Math.random() * 200);
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
    switch (routeName ?? activeRouteName) {
        case 'roster':
            return useRosterStore().getIsDirty;
        case 'group':
            return useGroupStore().getIsDirty;
        case 'question':
            return useQuestionStore().getIsDirty;
        case 'statictext':
            return useStaticTextStore().getIsDirty;
        case 'variable':
            return useVariableStore().getIsDirty;
        default:
            return false;
    }
}

export function setActiveDynamicImportRouteName(routeName) {
    activeRouteName = routeName ?? null;
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

    recoveryTimeoutId = window.setTimeout(() => {
        recoveryTimeoutId = null;
        window.location.reload();
    }, getDynamicImportRetryDelay(retryCount));
}

export function clearDynamicImportRecovery() {
    if (recoveryTimeoutId !== null) {
        window.clearTimeout(recoveryTimeoutId);
        recoveryTimeoutId = null;
    }

    clearRetryCount();
    recoveryScheduled = false;
}

export function wrapDynamicImport(loader) {
    return () =>
        loader().catch(error => {
            if (isDynamicImportError(error)) {
                scheduleDynamicImportRecovery(error);
            }

            throw error;
        });
}
