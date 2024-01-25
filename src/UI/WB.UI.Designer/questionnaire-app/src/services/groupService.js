import { get, commandCall } from './apiService';
import emitter from './emitter';

export function deleteGroup(questionnaireId, entityId) {
    var command = {
        questionnaireId: questionnaireId,
        groupId: entityId
    };

    return commandCall('DeleteGroup', command).then(result => {
        emitter.emit('groupDeleted', {
            itemId: entityId
        });
    });
}

export async function getGroup(questionnaireId, entityId) {
    const data = await get('/api/questionnaire/editGroup/' + questionnaireId, {
        groupId: entityId
    });
    return data;
}

export function updateGroup(questionnaireId, group) {
    var command = {
        questionnaireId: questionnaireId,
        groupId: group.id,
        title: group.title,
        condition: group.enablementCondition,
        hideIfDisabled: group.hideIfDisabled,
        isRoster: false,
        rosterSizeQuestionId: null,
        rosterSizeSource: 'Question',
        rosterFixedTitles: null,
        rosterTitleQuestionId: null,
        variableName: group.variableName
    };
    
    return commandCall('UpdateGroup', command).then(async response => {
        emitter.emit('groupUpdated', {
            id: group.id,
            title: group.title,
            enablementCondition: group.enablementCondition,
            hideIfDisabled: group.hideIfDisabled,
            isRoster: false,
            rosterSizeQuestionId: null,
            rosterSizeSource: 'Question',
            rosterFixedTitles: null,
            rosterTitleQuestionId: null,
            variableName: group.variableName
        });
    });
}
