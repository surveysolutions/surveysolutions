import { get, commandCall } from '../services/apiService';
import emitter from './emitter';

export function deleteVariable(questionnaireId, entityId) {
    var command = {
        questionnaireId: questionnaireId,
        entityId: entityId
    };

    return commandCall('DeleteVariable', command).then(result => {
        emitter.emit('variableDeleted', {
            itemId: entityId
        });
    });
}

export async function getVariable(questionnaireId, entityId) {
    const data = await get(
        '/api/questionnaire/editVariable/' + questionnaireId,
        {
            variableId: entityId
        }
    );
    return data;
}

export function updateVariable(questionnaireId, variable) {
    var command = {
        questionnaireId: questionnaireId,
        entityId: variable.id,
        variableData: {
            expression: variable.expression,
            name: variable.variable,
            type: variable.type,
            label: variable.label,
            doNotExport: variable.doNotExport
        }
    };

    var commandName = 'UpdateVariable';

    return commandCall(commandName, command).then(response => {
        emitter.emit('variableUpdated', {
            itemId: variable.id,
            name: variable.variable,
            label: variable.label,
            type: variable.type,
            doNotExport: variable.doNotExport
        });
    });
}
