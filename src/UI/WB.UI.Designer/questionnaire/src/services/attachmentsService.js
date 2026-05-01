import { upload, commandCall, processResponseErrorOrThrow } from './apiService';
import emitter from './emitter';
import { newGuid } from '../helpers/guid';
import { mande } from 'mande';
import { useBlockUIStore } from '../stores/blockUI';
import { useProgressStore } from '../stores/progress';

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

export async function uploadZipAttachments(questionnaireId, file) {
    const progressStore = useProgressStore();
    const blockUI = useBlockUIStore();

    progressStore.start();
    blockUI.start();

    const api = mande('/api/command/attachment/zip', { headers: { 'Content-Type': null } });

    const formData = new FormData();
    formData.append('file', file);
    formData.append('questionnaireId', questionnaireId);

    try {
        const response = await api.post(formData);
        blockUI.stop();
        progressStore.stop();
        return response;
    } catch (error) {
        blockUI.stop();
        progressStore.stop();
        processResponseErrorOrThrow(error);
    }
}
