import { commandCall } from './apiService';
import emitter from './emitter';

export function addCriticalityCondition(
    questionnaireId,
    criticalityConditionId
) {
    var command = {
        questionnaireId: questionnaireId,
        id: criticalityConditionId
    };

    return commandCall('AddCriticalRule', command).then(response => {
        emitter.emit('criticalityConditionAdded', {
            message: '',
            expression: '',
            description: '',
            isDescriptionVisible: false,
            id: criticalityConditionId
        });
    });
}

export function updateCriticalityCondition(
    questionnaireId,
    criticalityCondition
) {
    var command = {
        questionnaireId: questionnaireId,
        id: criticalityCondition.id,
        message: criticalityCondition.message,
        expression: criticalityCondition.expression,
        description: criticalityCondition.description
    };

    return commandCall('UpdateCriticalRule', command).then(response => {
        emitter.emit('criticalityConditionUpdated', {
            message: criticalityCondition.message,
            expression: criticalityCondition.expression,
            description: criticalityCondition.description,
            id: criticalityCondition.id
        });
    });
}

export function deleteCriticalityCondition(
    questionnaireId,
    criticalityConditionId
) {
    var command = {
        questionnaireId: questionnaireId,
        id: criticalityConditionId
    };

    return commandCall('DeleteCriticalRule', command).then(response => {
        emitter.emit('criticalityConditionDeleted', {
            questionnaireId: questionnaireId,
            id: criticalityConditionId
        });
    });
}
