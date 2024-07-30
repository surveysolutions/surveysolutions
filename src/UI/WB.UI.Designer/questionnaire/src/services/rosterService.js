import { get, commandCall } from './apiService';
import emitter from './emitter';
import { getItemIndexByIdFromParentItemsList } from './utilityService';
import { newGuid } from '../helpers/guid';
import { i18n } from '../plugins/localization';

export async function getRoster(questionnaireId, entityId) {
    const data = await get('/api/questionnaire/editRoster/' + questionnaireId, {
        rosterId: entityId
    });
    return data;
}

export function updateRoster(questionnaireId, roster) {
    var command = {
        questionnaireId: questionnaireId,
        groupId: roster.itemId,
        title: roster.title,
        description: roster.description,
        condition: roster.enablementCondition,
        hideIfDisabled: roster.hideIfDisabled,
        variableName: roster.variableName,
        displayMode: roster.displayMode,
        isRoster: true
    };

    switch (roster.type) {
        case 'Fixed':
            command.rosterSizeSource = 'FixedTitles';
            command.fixedRosterTitles = roster.fixedRosterTitles;
            break;
        case 'Numeric':
            command.rosterSizeQuestionId = roster.rosterSizeNumericQuestionId;
            command.rosterTitleQuestionId = roster.rosterTitleQuestionId;
            break;
        case 'List':
            command.rosterSizeQuestionId = roster.rosterSizeListQuestionId;
            break;
        case 'Multi':
            command.rosterSizeQuestionId = roster.rosterSizeMultiQuestionId;
            break;
    }

    return commandCall('UpdateGroup', command).then(response => {
        emitter.emit('rosterUpdated', { roster: roster });
    });
}

export function deleteRoster(questionnaireId, entityId) {
    var command = {
        questionnaireId: questionnaireId,
        groupId: entityId
    };

    return commandCall('DeleteGroup', command).then(result => {
        emitter.emit('rosterDeleted', {
            id: entityId
        });
    });
}

export function getQuestionsEligibleForNumericRosterTitle(
    questionnaireId,
    rosterId,
    rosterSizeQuestionId
) {
    return get(
        '/api/questionnaire/getQuestionsEligibleForNumericRosterTitle/' +
            questionnaireId,
        {
            rosterId: rosterId,
            rosterSizeQuestionId: rosterSizeQuestionId
        }
    );
}

export function addRoster(questionnaireId, parent, afterNodeId) {
    let index = getItemIndexByIdFromParentItemsList(parent, afterNodeId);
    const roster = createEmptyRoster();

    var command = {
        questionnaireId: questionnaireId,
        groupId: roster.itemId,
        title: roster.title,
        condition: '',
        hideIfDisabled: false,
        isRoster: true,
        rosterSizeQuestionId: null,
        rosterSizeSource: 'FixedTitles',
        fixedRosterTitles: [
            { value: 1, title: 'First Title' },
            { value: 2, title: 'Second Title' }
        ],
        rosterTitleQuestionId: null,
        parentGroupId: parent.itemId,
        variableName: roster.variableName
    };
    if (index != null && index >= 0) {
        index = index + 1;
        command.index = index;
    }

    return commandCall('AddGroup', command).then(function(result) {
        emitter.emit('rosterAdded', {
            roster: roster,
            index: index,
            parent: parent
        });

        return roster;
    });
}

function createEmptyRoster() {
    var newId = newGuid();
    var emptyRoster = {
        itemId: newId,
        title:
            i18n.t('QuestionnaireEditor.DefaultNewRoster') + ' - %rostertitle%',
        items: [],
        itemType: 'Group',
        hasCondition: false,
        isRoster: true
    };
    return emptyRoster;
}
