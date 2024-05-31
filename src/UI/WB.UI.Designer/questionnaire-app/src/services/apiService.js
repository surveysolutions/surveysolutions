import { mande } from 'mande';
import { useBlockUIStore } from '../stores/blockUI';
import { useProgressStore } from '../stores/progress';
import { isNull, isUndefined } from 'lodash';
import { notice, error } from './notificationService';
import { i18n } from '../plugins/localization';

const api = mande('/' /*, globalOptions*/);

export function getSilently(url, queryParams) {
    return getImpl(url, queryParams, true);
}

export function get(url, queryParams) {
    return getImpl(url, queryParams, false);
}

function getImpl(url, queryParams, silent = false) {
    const progressStore = useProgressStore();
    const blockUI = useBlockUIStore();

    if (!silent) {
        blockUI.start();
        progressStore.start();
    }
    const headers = getHeaders(false);

    if (queryParams) {
        return api
            .get(url, {
                headers: headers,
                query: queryParams
            })
            .then(response => {
                if (!silent) {
                    blockUI.stop();
                    progressStore.stop();
                }
                return response;
            })
            .catch(error => {
                if (!silent) {
                    blockUI.stop();
                    progressStore.stop();
                }
                processResponseErrorOrThrow(error);
            });
    }

    return api
        .get(url, { headers: headers })
        .then(response => {
            if (!silent) {
                blockUI.stop();
                progressStore.stop();
            }
            return response;
        })
        .catch(error => {
            if (!silent) {
                blockUI.stop();
                progressStore.stop();
            }
            processResponseErrorOrThrow(error);
        });
}

export function post(url, params) {
    const progressStore = useProgressStore();
    const blockUI = useBlockUIStore();

    const headers = getHeaders();
    blockUI.start();
    progressStore.start();
    return api
        .post(url, params, { headers: headers })
        .then(response => {
            blockUI.stop();
            progressStore.stop();
            return response;
        })
        .catch(error => {
            blockUI.stop();
            progressStore.stop();

            processResponseErrorOrThrow(error);
        });
}

export function patch(url, params) {
    const progressStore = useProgressStore();
    const blockUI = useBlockUIStore();

    const headers = getHeaders();

    blockUI.start();
    progressStore.start();

    return api
        .patch(url, params, { headers: headers })
        .then(response => {
            blockUI.stop();
            progressStore.stop();
            return response;
        })
        .catch(error => {
            blockUI.stop();
            progressStore.stop();

            processResponseErrorOrThrow(error);
        });
}

export function put(url, params) {
    const progressStore = useProgressStore();
    const blockUI = useBlockUIStore();

    const headers = getHeaders();

    blockUI.start();
    progressStore.start();

    return api
        .put(url, params, { headers: headers })
        .then(response => {
            blockUI.stop();
            progressStore.stop();
            return response;
        })
        .catch(error => {
            blockUI.stop();
            progressStore.stop();

            processResponseErrorOrThrow(error);
        });
}

export function del(url) {
    const progressStore = useProgressStore();
    const blockUI = useBlockUIStore();

    const headers = getHeaders();
    blockUI.start();
    progressStore.start();
    return api
        .delete(url, { headers: headers })
        .then(response => {
            blockUI.stop();
            progressStore.stop();
            return response;
        })
        .catch(error => {
            blockUI.stop();
            progressStore.stop();

            processResponseErrorOrThrow(error);
        });
}

export function commandCall(commandType, command) {
    return post('/api/command', {
        type: commandType,
        command: JSON.stringify(command)
    });
}

export function upload(url, file, command, fileName) {
    const progressStore = useProgressStore();
    const blockUI = useBlockUIStore();

    progressStore.start();
    blockUI.start();

    const api = mande(url, { headers: { 'Content-Type': null } });

    const formData = new FormData();
    formData.append('file', isNull(file) || isUndefined(file) ? '' : file);
    formData.append(
        'command',
        isNull(command) ? null : JSON.stringify(command)
    );

    if (fileName) {
        formData.append('fileName', fileName);
    }

    return api
        .post(formData)
        .then(response => {
            blockUI.stop();
            progressStore.stop();

            return response;
        })
        .catch(error => {
            blockUI.stop();
            progressStore.stop();

            processResponseErrorOrThrow(error);
        });
}

function getHeaders(includeCSRF = true) {
    var headers = {
        'Content-Type': 'application/json',
        Accept: 'application/json' //, text/plain, */*
    };

    if (includeCSRF) {
        headers['X-CSRF-TOKEN'] = getCsrfCookie();
    }

    return headers;
}

function getCsrfCookie() {
    var name = 'CSRF-TOKEN-D=';
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return '';
}

function processResponseErrorOrThrow(errorResp) {
    if (!errorResp.response) {
        error(i18n.t('QuestionnaireEditor.RequestFailedUnexpectedly'));
        throw errorResp;
    }

    if (
        errorResp.response.status === 406 ||
        errorResp.response.status === 403 ||
        errorResp.response.status === 400
    ) {
        if (errorResp.body.message) {
            notice(errorResp.body.message);
        } else {
            notice(errorResp.body.Message);
        }
    } else if (errorResp.response.status === 404) {
        notice(i18n.t('QuestionnaireEditor.EntryWasNotFound'));
    } else {
        error(i18n.t('QuestionnaireEditor.RequestFailedUnexpectedly'));
    }

    throw errorResp;
}
