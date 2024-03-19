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

export async function runScenario(questionnaireId, scenarioId) {
    var webTesterWindow = window.open('about:blank', '_blank');

    var webTestUrl = await get('/api/questionnaire/webTest/' + questionnaireId);

    if (!isUndefined(scenarioId)) {
        webTestUrl += '?scenarioId=' + scenarioId;
    }

    webTesterWindow.location.href = webTestUrl;
}

export async function getScenarioSteps(questionnaireId, scenarioId) {
    const data = await get(
        '/api/questionnaire/' + questionnaireId + '/scenarios/' + scenarioId
    );
    return JSON.stringify(data, null, 4);
}
