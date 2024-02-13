import { get, commandCall } from './apiService';
import emitter from './emitter';
import { getItemIndexByIdFromParentItemsList } from './utilityService';
import { newGuid } from '../helpers/guid';

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

    return commandCall('UpdateVariable', command).then(response => {
        emitter.emit('variableUpdated', variable);
    });
}

export function addVariable(questionnaireId, parent, afterNodeId) {
    let index = getItemIndexByIdFromParentItemsList(parent, afterNodeId);
    const variable = createEmptyVariable();

    var command = {
        questionnaireId: questionnaireId,
        entityId: variable.itemId,
        parentId: parent.itemId,
        variableData: {}
    };
    if (index != null && index >= 0) {
        index = index + 1;
        command.index = index;
    }

    return commandCall('AddVariable', command).then(result => {
        emitter.emit('variableAdded', {
            variable: variable,
            index: index,
            parent: parent
        });

        return variable;
    });
}

function createEmptyVariable() {
    var newId = newGuid();
    var emptyVariable = {
        itemId: newId,
        itemType: 'Variable',
        variableData: {},
        items: []
    };
    return emptyVariable;
}
