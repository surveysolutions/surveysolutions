import { get, commandCall } from '../services/apiService';
import emitter from './emitter';
import { getItemIndexByIdFromParentItemsList } from './utilityService';
import { newGuid } from '../helpers/guid';
import { i18n } from '../plugins/localization';

export function deleteStaticText(questionnaireId, entityId) {
    var command = {
        questionnaireId: questionnaireId,
        entityId: entityId
    };

    return commandCall('DeleteStaticText', command).then(result => {
        emitter.emit('staticTextDeleted', {
            id: entityId
        });
    });
}

export async function getStaticText(questionnaireId, entityId) {
    const data = await get(
        '/api/questionnaire/editStaticText/' + questionnaireId,
        { staticTextId: entityId }
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

    return commandCall('UpdateStaticText', command).then(async response => {
        emitter.emit('staticTextUpdated', staticText);
    });
}

export function addStaticText(questionnaireId, parent, afterNodeId) {
    let index = getItemIndexByIdFromParentItemsList(parent, afterNodeId);

    const staticText = createEmptyStaticText();
    const command = {
        questionnaireId: questionnaireId,
        parentId: parent.itemId,
        entityId: staticText.itemId,
        text: staticText.text
    };

    if (index != null && index >= 0) {
        index = index + 1;
        command.index = index;
    }

    return commandCall('AddStaticText', command).then(result => {
        emitter.emit('staticTextAdded', {
            staticText: staticText,
            index: index,
            parent: parent
        });

        return staticText;
    });
}

function createEmptyStaticText() {
    var newId = newGuid();
    var staticText = {
        itemId: newId,
        text: i18n.t('QuestionnaireEditor.DefaultNewStaticText'),
        itemType: 'StaticText',
        hasCondition: false,
        hasValidation: false,
        items: []
    };
    return staticText;
}
