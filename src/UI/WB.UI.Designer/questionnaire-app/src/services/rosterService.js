import { get, commandCall } from './apiService';
import emitter from './emitter';

export async function getRoster(questionnaireId, entityId) {
    const data = await get('/api/questionnaire/editRoster/' + questionnaireId, {
        rosterId: entityId
    });
    return data;
}
