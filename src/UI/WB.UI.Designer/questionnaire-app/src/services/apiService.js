import { mande, defaults } from 'mande';

//defaults.headers.Authorization = 'Bearer token';

const api = mande('/questionnaire' /*, globalOptions*/);

export function get(url) {
    return api.get(url);
}

export function post(url, params) {
    const headers = {
        'Content-Type': 'application/json',
        Accept: 'application/json',
        'X-CSRF-TOKEN': getCsrfCookie()
    };

    return api.post(url, params, { headers: headers });
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
