import { get, post, commandCall } from '../services/apiService';
import emitter from './emitter';
import { toLocalDateTime } from '../services/utilityService';

export function addSection(questionnaireId, newSection) {
    // return commandCall('AddGroup', {questionnaireId, newGroup}).then(response => {
    //     emitter.emit('groupAdded', {questionnaireId, newGroup});
    // });
}

export function addSubsection(questionnaireId, newSubsection) {
    // return commandCall('AddGroup', {questionnaireId, newGroup}).then(response => {
    //     emitter.emit('groupAdded', {questionnaireId, newGroup});
    // });
}

export async function getQuestionnaire(questionnaireId) {
    try {
        return await get('/api/questionnaire/get/' + questionnaireId);
    } catch (error) {
        if (error.response.status === 401 || error.response.status === 404) {
            window.location = '/';
        } else throw error;
    }
}

export function updateQuestionnaireSettings(questionnaireId, questionnaire) {
    var command = {
        title: questionnaire.title,
        variable: questionnaire.variable,
        questionnaireId: questionnaireId,
        hideifDisabled: questionnaire.hideIfDisabled,
        isPublic: questionnaire.isPublic,
        defaultLanguageName: questionnaire.defaultLanguageName
    };

    return commandCall('UpdateQuestionnaire', command).then(async response => {
        emitter.emit('questionnaireSettingsUpdated', {
            title: questionnaire.title,
            variable: questionnaire.variable,
            questionnaireId: questionnaireId,
            hideIfDisabled: questionnaire.hideIfDisabled,
            isPublic: questionnaire.isPublic,
            defaultLanguageName: questionnaire.defaultLanguageName
        });
    });
}

export async function regenerateAnonymousQuestionnaireLink(questionnaireId) {
    var regenerateUrl =
        '/questionnaire/regenerateAnonymousQuestionnaireLink/' +
        questionnaireId;

    return await post(regenerateUrl).then(response => {
        var data = response;
        emitter.emit('anonymousQuestionnaireSettingsUpdated', {
            isAnonymouslyShared: data.isActive,
            anonymousQuestionnaireId: data.isActive
                ? data.anonymousQuestionnaireId
                : null,
            anonymouslySharedAtUtc: data.generatedAtUtc
        });
        return data;
    });
}

export async function updateAnonymousQuestionnaireSettings(
    questionnaireId,
    isActive
) {
    var updateUrl =
        '/questionnaire/updateAnonymousQuestionnaireSettings/' +
        questionnaireId;

    return await post(updateUrl, { isActive: isActive }).then(response => {
        var data = response;
        emitter.emit('anonymousQuestionnaireSettingsUpdated', {
            isAnonymouslyShared: data.isActive,
            anonymousQuestionnaireId: data.isActive
                ? data.anonymousQuestionnaireId
                : null,
            anonymouslySharedAtUtc: data.anonymouslySharedAtUtc
        });
    });
}

export function shareWith(
    userEmail,
    userName,
    userId,
    questionnaireId,
    shareType
) {
    return commandCall('AddSharedPersonToQuestionnaire', {
        emailOrLogin: userEmail,
        questionnaireId: questionnaireId,
        shareType: shareType
    }).then(response => {
        emitter.emit('sharedPersonAdded', {
            questionnaireId: questionnaireId,
            shareType: shareType,
            email: userEmail,
            name: userName,
            id: userId
        });
    });
}

export function removeSharedPerson(questionnaireId, userId, email) {
    return commandCall('RemoveSharedPersonFromQuestionnaire', {
        personId: userId,
        email: email,
        questionnaireId: questionnaireId
    }).then(response => {
        emitter.emit('sharedPersonRemoved', {
            personId: userId,
            email: email,
            questionnaireId: questionnaireId
        });
    });
}

export function passOwnership(
    questionnaireId,
    ownerEmail,
    newOwnerId,
    newOwnerEmail
) {
    return commandCall('PassOwnershipFromQuestionnaire', {
        ownerEmail,
        newOwnerId,
        newOwnerEmail,
        questionnaireId: questionnaireId
    }).then(response => {
        emitter.emit('ownershipPassed', {
            ownerEmail,
            newOwnerId,
            newOwnerEmail,
            questionnaireId: questionnaireId
        });
    });
}

export function migrateToNewVersion(questionnaireId) {
    var command = {
        questionnaireId: questionnaireId
    };
    return commandCall('MigrateToNewVersion', command);
}

export function moveItem(
    questionnaireId,
    itemId,
    itemType,
    newParentId,
    index
) {
    if (itemType == 'Question')
        return moveQuestion(questionnaireId, itemId, index, newParentId);
    else if (itemType == 'StaticText')
        return moveStaticText(questionnaireId, itemId, index, newParentId);
    else if (itemType == 'Variable')
        return moveVariable(questionnaireId, itemId, index, newParentId);
    else if (itemType == 'Group')
        return moveGroup(questionnaireId, itemId, index, newParentId);
}

export function moveGroup(questionnaireId, groupId, index, destGroupId) {
    var command = {
        targetGroupId: destGroupId,
        targetIndex: index,
        groupId: groupId,
        questionnaireId: questionnaireId
    };

    return commandCall('MoveGroup', command).then(response => {
        emitter.emit('groupMoved', {
            itemId: groupId,
            newParentId: destGroupId,
            newIndex: index,
            questionnaireId: questionnaireId
        });
    });
}

export function moveQuestion(questionnaireId, questionId, index, destGroupId) {
    var command = {
        targetGroupId: destGroupId,
        targetIndex: index,
        questionId: questionId,
        questionnaireId: questionnaireId
    };

    return commandCall('MoveQuestion', command).then(response => {
        emitter.emit('questionMoved', {
            itemId: questionId,
            newParentId: destGroupId,
            newIndex: index,
            questionnaireId: questionnaireId
        });
    });
}

export function moveStaticText(questionnaireId, entityId, index, destGroupId) {
    var command = {
        targetEntityId: destGroupId,
        targetIndex: index,
        entityId: entityId,
        questionnaireId: questionnaireId
    };

    return commandCall('MoveStaticText', command).then(response => {
        emitter.emit('staticTextMoved', {
            itemId: entityId,
            newParentId: destGroupId,
            newIndex: index,
            questionnaireId: questionnaireId
        });
    });
}

export function moveVariable(questionnaireId, entityId, index, destGroupId) {
    var command = {
        targetEntityId: destGroupId,
        targetIndex: index,
        entityId: entityId,
        questionnaireId: questionnaireId
    };

    return commandCall('MoveVariable', command).then(response => {
        emitter.emit('variableMoved', {
            itemId: entityId,
            newParentId: destGroupId,
            newIndex: index,
            questionnaireId: questionnaireId
        });
    });
}
