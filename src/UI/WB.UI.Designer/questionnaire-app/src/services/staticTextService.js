import { get, commandCall } from '../services/apiService';
import emitter from './emitter';

export function deleteStaticText(questionnaireId, entityId) {
    var command = {
        questionnaireId: questionnaireId,
        entityId: entityId
    };

    return commandCall('DeleteStaticText', command).then(result => {
        emitter.emit('staticTextDeleted', {
            itemId: entityId
        });
    });
}

export async function getStaticText(questionnaireId, entityId) {
    const data = await get(
        '/api/questionnaire/editStaticText/' + questionnaireId,
        {
            staticTextId: entityId
        }
    );
    return data;
}

export function addStaticText(commandName, command) {
    return commandCall(commandName, command).then(response => {
        emitter.emit('staticTextAdded', {
            //itemId: command.entityId
        });
    });
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

    return commandCall('UpdateStaticText', command).then(async response => {
        emitter.emit('staticTextUpdated', {
            itemId: staticText.id,
            text: staticText.text,
            attachmentName: staticText.attachmentName,

            enablementCondition: staticText.enablementCondition,
            validationConditions: staticText.validationConditions
        });
    });
}
