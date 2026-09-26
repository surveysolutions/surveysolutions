import { put, del, get } from './apiService';
import emitter from './emitter';
import { isUndefined } from 'lodash';

export async function updateScenario(questionnaireId, scenario) {
    const response = await put(
        '/api/questionnaire/' + questionnaireId + '/scenarios/' + scenario.id,
        { title: scenario.title }
    );

    emitter.emit('scenarioUpdated', {
        scenario: scenario
    });

    return response;
}

export async function deleteScenario(questionnaireId, scenarioId) {
    const response = await del(
        '/api/questionnaire/' + questionnaireId + '/scenarios/' + scenarioId
    );

    emitter.emit('scenarioDeleted', {
        scenarioId: scenarioId
    });

    return response;
}

export function isPopupBlockedError(error) {
    return error === 'popup_blocked'
        || error?.message === 'popup_blocked'
        || error?.code === 'popup_blocked'
        || error?.name === 'popup_blocked';
}

export async function runScenario(questionnaireId, scenarioId) {
    var webTesterWindow = window.open('about:blank', '_blank', 'noopener,noreferrer');

    if (!webTesterWindow) {
        const err = new Error('popup_blocked');
        err.code = 'popup_blocked';
        throw err;
    }

    webTesterWindow.opener = null;

    try {
        // Designer returns the full redirect URL with ?code=<one-time-code> already embedded.
        // JWT never appears in the browser — code is exchanged server-to-server by WebTester.
        var webTestUrl = await get('/api/questionnaire/webTest/' + questionnaireId);

        if (!isUndefined(scenarioId)) {
            // The URL already contains ?code=..., so scenarioId must be appended with &, not ?.
            const separator = webTestUrl.includes('?') ? '&' : '?';
            webTestUrl += separator + 'scenarioId=' + scenarioId;
        }

        webTesterWindow.location.href = webTestUrl;
    } catch (error) {
        webTesterWindow.close();
        throw error;
    }
}

export async function getScenarioSteps(questionnaireId, scenarioId) {
    const data = await get(
        '/api/questionnaire/' + questionnaireId + '/scenarios/' + scenarioId
    );
    return JSON.stringify(data, null, 4);
}
