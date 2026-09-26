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
let activeRouteName = null;
const recoveryTimeoutIds = new Map();

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

function confirmDynamicImportRecovery(routeName, requireReloadConfirmation, hasUnsavedChangesCallback) {
    const hasPendingChanges = hasUnsavedChanges(routeName)
        || hasUnsavedChangesCallback?.() === true;

    if (hasPendingChanges) {
        return window.confirm(
            i18n.t('QuestionnaireEditor.UnsavedChangesReload')
        );
    }

    if (!requireReloadConfirmation) {
        return true;
    }

    return window.confirm(
        i18n.t('QuestionnaireEditor.RefreshPageConfirm')
    );
}

export function scheduleDynamicImportRecovery(error, options = {}) {
    const {
        recoveryScope,
        routeName,
        requireReloadConfirmation = false,
        hasUnsavedChanges: hasUnsavedChangesCallback
    } = options;
    const scope = getDynamicImportRecoveryScope(recoveryScope);
    if (recoveryTimeoutIds.has(scope)) return;

    const retryCount = getRetryCount(scope);

    if (retryCount === null) {
        console.error('Dynamic import recovery skipped: session storage is unavailable.', error);
        return;
    }

    if (retryCount >= MAX_DYNAMIC_IMPORT_RETRIES) {
        clearRetryCount(scope);
        console.error('Dynamic import retry budget exhausted:', error);
        return;
    }

    if (!confirmDynamicImportRecovery(routeName, requireReloadConfirmation, hasUnsavedChangesCallback)) {
        return;
    }

    if (!setRetryCount(retryCount + 1, scope)) {
        console.error('Dynamic import recovery skipped: session storage is unavailable.', error);
        return;
    }

    const timeoutId = window.setTimeout(() => {
        recoveryTimeoutIds.delete(scope);
        window.location.reload();
    }, getDynamicImportRetryDelay(retryCount));
    recoveryTimeoutIds.set(scope, timeoutId);
}

export function clearDynamicImportRecovery(recoveryScope) {
    const timeoutId = recoveryTimeoutIds.get(recoveryScope);
    if (timeoutId !== undefined) {
        window.clearTimeout(timeoutId);
        recoveryTimeoutIds.delete(recoveryScope);
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
