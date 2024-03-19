import { commandCall } from './apiService';
import emitter from './emitter';

export function addMacro(questionnaireId, macroId) {
    var command = {
        questionnaireId: questionnaireId,
        macroId: macroId
    };

    return commandCall('AddMacro', command).then(response => {
        emitter.emit('macroAdded', {
            name: '',
            content: '',
            description: '',
            isDescriptionVisible: false,
            itemId: macroId
        });
    });
}

export function updateMacro(questionnaireId, macro) {
    var command = {
        questionnaireId: questionnaireId,
        macroId: macro.itemId,
        name: macro.name,
        content: macro.content,
        description: macro.description
    };

    return commandCall('UpdateMacro', command).then(response => {
        emitter.emit('macroUpdated', {
            name: macro.name,
            content: macro.content,
            description: macro.description,
            //isDescriptionVisible: false,
            itemId: macro.itemId
        });
    });
}

export function deleteMacro(questionnaireId, macroId) {
    var command = {
        questionnaireId: questionnaireId,
        macroId: macroId
    };

    return commandCall('DeleteMacro', command).then(response => {
        emitter.emit('macroDeleted', {
            questionnaireId: questionnaireId,
            itemId: macroId
        });
    });
}
