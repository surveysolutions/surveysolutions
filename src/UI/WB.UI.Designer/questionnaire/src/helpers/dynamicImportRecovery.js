import { useRosterStore } from '../stores/roster';
import { useGroupStore } from '../stores/group';
import { useQuestionStore } from '../stores/question';
import { useStaticTextStore } from '../stores/staticText';
import { useVariableStore } from '../stores/variable';
import { i18n } from '../plugins/localization';

const DYNAMIC_IMPORT_RETRY_KEY_PREFIX = 'dynamic-import-retry-count:';
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
let scheduledRecoveryScope = null;

export function isDynamicImportError(error) {
    const message = (error instanceof Error ? error.message : String(error)).toLowerCase();
    return dynamicImportErrorPatterns.some(pattern => message.includes(pattern));
}

function getDynamicImportRetryDelay(retryCount) {
    return 300 * 2 ** retryCount + Math.floor(Math.random() * 200);
}

function getDynamicImportRecoveryScope(recoveryScope) {
    return recoveryScope ?? 'route:default';
}

export function getRouteDynamicImportRecoveryScope(routeName) {
    return `route:${routeName ?? 'default'}`;
}

function getRetryStorageKey(recoveryScope) {
    return `${DYNAMIC_IMPORT_RETRY_KEY_PREFIX}${getDynamicImportRecoveryScope(recoveryScope)}`;
}

function getRetryCount(recoveryScope) {
    try {
        const value = Number(window.sessionStorage.getItem(getRetryStorageKey(recoveryScope)) ?? 0);
        return Number.isFinite(value) ? value : 0;
    } catch {
        return null;
    }
}

function setRetryCount(count, recoveryScope) {
    try {
        window.sessionStorage.setItem(getRetryStorageKey(recoveryScope), String(count));
        return true;
    } catch {
        return false;
    }
}

function clearRetryCount(recoveryScope) {
    try {
        window.sessionStorage.removeItem(getRetryStorageKey(recoveryScope));
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

function confirmDynamicImportRecovery(routeName, requireReloadConfirmation) {
    if (!requireReloadConfirmation && !hasUnsavedChanges(routeName)) {
        return true;
    }

    return window.confirm(
        i18n.t('QuestionnaireEditor.UnsavedChangesReload')
    );
}

export function scheduleDynamicImportRecovery(error, options = {}) {
    if (recoveryScheduled) return;

    const { recoveryScope, routeName, requireReloadConfirmation = false } = options;
    const scope = getDynamicImportRecoveryScope(recoveryScope);
    const retryCount = getRetryCount(scope);

    if (retryCount === null) {
        console.error('Dynamic import recovery skipped: session storage is unavailable.', error);
        return;
    }

    if (retryCount >= MAX_DYNAMIC_IMPORT_RETRIES) {
        console.error('Dynamic import retry budget exhausted:', error);
        return;
    }

    recoveryScheduled = true;
    scheduledRecoveryScope = scope;

    if (!confirmDynamicImportRecovery(routeName, requireReloadConfirmation)) {
        recoveryScheduled = false;
        scheduledRecoveryScope = null;
        return;
    }

    if (!setRetryCount(retryCount + 1, scope)) {
        recoveryScheduled = false;
        scheduledRecoveryScope = null;
        console.error('Dynamic import recovery skipped: session storage is unavailable.', error);
        return;
    }

    recoveryTimeoutId = window.setTimeout(() => {
        recoveryTimeoutId = null;
        window.location.reload();
    }, getDynamicImportRetryDelay(retryCount));
}

export function clearDynamicImportRecovery(recoveryScope) {
    if (recoveryTimeoutId !== null && scheduledRecoveryScope === recoveryScope) {
        window.clearTimeout(recoveryTimeoutId);
        recoveryTimeoutId = null;
        recoveryScheduled = false;
        scheduledRecoveryScope = null;
    }

    clearRetryCount(recoveryScope);
}

export function wrapDynamicImport(loader, options = {}) {
    return () =>
        loader()
            .then(module => {
                clearDynamicImportRecovery(options.recoveryScope);
                return module;
            })
            .catch(error => {
                if (isDynamicImportError(error)) {
                    scheduleDynamicImportRecovery(error, options);
                }

                throw error;
            });
}
