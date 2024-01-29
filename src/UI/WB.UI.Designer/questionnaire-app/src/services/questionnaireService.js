import { get, post, commandCall } from '../services/apiService';
import emitter from './emitter';
import { toLocalDateTime } from '../services/utilityService';

export function updateQuestionnaire(questionnaireId, questionnaire) {
    var command = {
        title: questionnaire.title,
        variable: questionnaire.variable,
        questionnaireId: questionnaireId,
        hideifDisabled: questionnaire.hideIfDisabled,
        isPublic: questionnaire.isPublic,
        defaultLanguageName: questionnaire.defaultLanguageName
    };

    return commandCall('UpdateQuestionnaire', command).then(async response => {
        emitter.emit('questionnaireUpdated', {
            title: questionnaire.title,
            variable: questionnaire.variable,
            questionnaireId: questionnaireId,
            hideifDisabled: questionnaire.hideIfDisabled,
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
            anonymousQuestionnaireShareDate: toLocalDateTime(
                data.anonymouslySharedAtUtc
            )
        });
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
            anonymousQuestionnaireShareDate: toLocalDateTime(
                data.anonymouslySharedAtUtc
            )
        });
    });
}

export function shareWith(emailOrLogin, questionnaireId, shareType) {
    return commandCall('AddSharedPersonToQuestionnaire', {
        emailOrLogin: emailOrLogin,
        questionnaireId: questionnaireId,
        shareType: shareType
    }).then(response => {
        emitter.emit('sharedPersonAdded', {
            emailOrLogin: emailOrLogin,
            questionnaireId: questionnaireId,
            shareType: shareType
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
