import { mande } from 'mande';
import { useBlockUIStore } from '../stores/blockUI';
import { useProgressStore } from '../stores/progress';
import { isNull, isUndefined } from 'lodash';
import { notice, error } from './notificationService';
import { i18n } from '../plugins/localization';

const api = mande('/' /*, globalOptions*/);

export function get(url, queryParams) {
    const progressStore = useProgressStore();
    const blockUI = useBlockUIStore();

    blockUI.start();
    progressStore.start();

    const headers = getHeaders(false);

    if (queryParams) {
        return api
            .get(url, {
                headers: headers,
                query: queryParams
            })
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

    return api
        .get(url, { headers: headers })
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

export function upload(url, file, command) {
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

function processResponseErrorOrThrow(error) {
    if (
        error.response.status === 406 ||
        error.response.status === 403 ||
        error.response.status === 400
    ) {
        if (error.body.message) {
            notice(error.body.message);
        } else {
            notice(error.body.Message);
        }
    } else if (error.response.status === 404) {
        notice(i18n.t('QuestionnaireEditor.EntryWasNotFound'));
    } else {
        error(i18n.t('QuestionnaireEditor.RequestFailedUnexpectedly'));
        throw error;
    }
}
