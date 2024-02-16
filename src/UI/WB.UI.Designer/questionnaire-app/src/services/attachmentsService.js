import { upload, commandCall } from './apiService';
import emitter from './emitter';
import { newGuid } from '../helpers/guid';

export async function updateAttachment(
    questionnaireId,
    attachment,
    isNew = false
) {
    var command = {
        questionnaireId: questionnaireId,
        attachmentId: attachment.attachmentId,
        attachmentName: attachment.name
    };

    //attachment update requires new Id even for only title change
    if (!isNew) {
        command.oldAttachmentId = attachment.attachmentId;
        command.attachmentId = newGuid();
    }

    const fileName = attachment.meta ? attachment.meta.fileName : null;

    const response = await upload(
        '/api/command/attachment',
        attachment.file,
        command,
        fileName
    );

    attachment.meta.lastUpdateDate = new Date();

    emitter.emit('attachmentUpdated', {
        attachment: attachment,
        newId: command.oldAttachmentId ? command.attachmentId : null
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
            id: attachmentId
        });
    });
}
