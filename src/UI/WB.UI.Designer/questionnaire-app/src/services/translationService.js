import { upload, commandCall } from './apiService';
import emitter from './emitter';
import { newGuid } from '../helpers/guid';

export function updateTranslation(questionnaireId, translation, isNew = false) {
    var command = {
        questionnaireId: questionnaireId,
        translationId: translation.translationId,
        name: translation.name
    };

    if (!isNew && translation.file !== null && translation.file !== undefined) {
        command.oldTranslationId = translation.translationId;
        command.translationId = newGuid();
    }

    return upload('/api/command/translation', translation.file, command).then(
        response => {
            emitter.emit('translationUpdated', {
                translation: translation,
                newId: command.oldTranslationId ? command.translationId : null
            });
            return response;
        }
    );
}

export function deleteTranslation(questionnaireId, translationId) {
    var command = {
        questionnaireId: questionnaireId,
        translationId: translationId
    };

    return commandCall('DeleteTranslation', command).then(response => {
        emitter.emit('translationDeleted', {
            id: translationId
        });
    });
}

export function setDefaultTranslation(questionnaireId, translationId) {
    var command = {
        questionnaireId: questionnaireId,
        translationId: translationId
    };
    return commandCall('SetDefaultTranslation', command).then(response => {
        emitter.emit('defaultTranslationSet', {
            translationId: translationId
        });
    });
}
