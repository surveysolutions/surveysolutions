import { get, commandCall } from './apiService';
import emitter from './emitter';
import { getItemIndexByIdFromParentItemsList } from './utilityService';
import { newGuid } from '../helpers/guid';
import { i18n } from '../plugins/localization';

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
        emitter.emit('groupUpdated', { group: group });
    });
}

export function addGroup(questionnaireId, parent, afterNodeId) {
    let index = getItemIndexByIdFromParentItemsList(parent, afterNodeId);
    const group = createEmptyGroup(parent);

    var command = {
        questionnaireId: questionnaireId,
        groupId: group.itemId,
        title: group.title,
        condition: '',
        hideIfDisabled: false,
        isRoster: false,
        rosterSizeQuestionId: null,
        rosterSizeSource: 'Question',
        rosterFixedTitles: null,
        rosterTitleQuestionId: null,
        parentGroupId: parent.itemId,
        variableName: null
    };
    if (index != null && index >= 0) {
        index = index + 1;
        command.index = index;
    }

    return commandCall('AddGroup', command).then(result => {
        emitter.emit('groupAdded', {
            group: group,
            index: index,
            parent: parent
        });

        return group;
    });
}

function createEmptyGroup() {
    var newId = newGuid();
    var emptyGroup = {
        itemId: newId,
        title: i18n.t('QuestionnaireEditor.DefaultNewSubsection'),
        items: [],
        itemType: 'Group',
        hasCondition: false
    };
    return emptyGroup;
}
