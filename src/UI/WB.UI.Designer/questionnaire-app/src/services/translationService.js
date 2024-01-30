import { upload, commandCall } from './apiService';
import emitter from './emitter';

export async function updateTranslation(questionnaireId, translation) {
    var command = {
        questionnaireId: questionnaireId,
        translationId: translation.translationId,
        oldTranslationId: translation.oldTranslationId,
        name: translation.name
    };

    const response = await upload(
        '/api/command/translation',
        translation.file,
        command
    );

    emitter.emit('translationUpdated', {
        translation: translation
    });

    return response;
}

export function deleteTranslation(questionnaireId, translationId) {
    var command = {
        questionnaireId: questionnaireId,
        translationId: translationId
    };

    return commandCall('DeleteTranslation', command).then(response => {
        emitter.emit('translationDeleted', {
            translationId: translationId
        });
    });
}

export function setDefaultTranslation(questionnaireId, translationId) {
    var command = {
        questionnaireId: questionnaireId,
        translationId: translationId
    };
    return commandCall('SetDefaultTranslation', command).then(response => {
        emitter.emit('settedDefaultTranslation', {
            translationId: translationId
        });
    });
}
