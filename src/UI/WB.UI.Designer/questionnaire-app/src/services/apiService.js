import { mande, defaults } from 'mande';
import { useBlockUIStore } from '../stores/blockUI';
//defaults.headers.Authorization = 'Bearer token';

const api = mande('/' /*, globalOptions*/);

export function get(url, queryParams) {
    if (queryParams) {
        return api.get(url, {
            query: queryParams
        });
    }

    return api.get(url);
}

export function post(url, params) {
    const headers = getHeaders();
    return api.post(url, params, { headers: headers });
}

export function patch(url, params) {
    const headers = getHeaders();
    return api.patch(url, params, { headers: headers });
}

export function del(url, params) {
    const headers = getHeaders();
    return api.delete(url, params, { headers: headers });
}

function getHeaders() {
    return {
        'Content-Type': 'application/json',
        Accept: 'application/json',
        'X-CSRF-TOKEN': getCsrfCookie()
    };
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
