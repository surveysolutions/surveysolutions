import { get, commandCall } from '../services/apiService';
import emitter from './emitter';

export function deleteStaticText(questionnaireId, itemId) {
    var command = {
        questionnaireId: questionnaireId,
        entityId: itemId
    };

    return commandCall('DeleteStaticText', command).then(result => {
        emitter.emit('staticTextDeleted', {
            itemId: itemId
        });
    });
}

export function getStaticText(questionnaireId, staticTextId) {
    const data = get(
        '/api/questionnaire/editStaticText/' + questionnaireId,
        {
            staticTextId: staticTextId
        }
    );
    return data;
}

export function updateStaticText(questionnaireId, staticText) {
    var command = {
        questionnaireId: questionnaireId,
        entityId: staticText.id,
        text: staticText.text,
        attachmentName: staticText.attachmentName,
        enablementCondition: staticText.enablementCondition,
        hideIfDisabled: staticText.hideIfDisabled,
        validationConditions: staticText.validationConditions
    };

    var commandName = 'UpdateStaticText';

    return commandCall(commandName, command).then(response => {
        emitter.emit('staticTextUpdated', {
            itemId: staticText.id,
            text: staticText.text,
            attachmentName: staticText.attachmentName,

            enablementCondition: staticText.enablementCondition,
            validationConditions: staticText.validationConditions
        });
    });
}
