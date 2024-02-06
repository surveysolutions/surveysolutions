import { upload, commandCall } from './apiService';
import emitter from './emitter';

export async function updateAttachment(questionnaireId, attachment) {
    var command = {
        questionnaireId: questionnaireId,
        attachmentId: attachment.attachmentId,
        attachmentName: attachment.name,
        oldAttachmentId: attachment.oldAttachmentId
    };

    const fileName = attachment.meta ? attachment.meta.fileName : null;

    const response = await upload(
        '/api/command/attachment',
        attachment.file,
        command,
        fileName
    );

    emitter.emit('attachmentUpdated', {
        attachment: attachment
    });

    return response;
}

export function deleteAttachment(questionnaireId, attachmentId) {
    var command = {
        questionnaireId: questionnaireId,
        attachmentId: attachmentId
    };

    return commandCall('DeleteAttachment', command).then(response => {
        emitter.emit('attachmentDeleted', {
            attachmentId: attachmentId
        });
    });
}
