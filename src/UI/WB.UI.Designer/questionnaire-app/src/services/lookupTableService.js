import { upload, commandCall } from './apiService';
import emitter from './emitter';

export async function addLookupTable(questionnaireId, lookupTable) {
    var command = {
        questionnaireId: questionnaireId,
        lookupTableId: lookupTable.itemId
    };
    return commandCall('AddLookupTable', command).then(response => {
        emitter.emit('lookupTableAdded', {
            lookupTable: lookupTable
        });
    });
}

export async function updateLookupTable(questionnaireId, lookupTable) {
    var command = {
        questionnaireId: questionnaireId,
        lookupTableId: lookupTable.itemId,
        lookupTableName: lookupTable.name,
        lookupTableFileName: lookupTable.fileName,
        oldLookupTableId: lookupTable.oldItemId
    };

    const response = await upload(
        '/api/command/UpdateLookupTable',
        lookupTable.file,
        command
    );

    emitter.emit('lookupTableUpdated', {
        lookupTable: lookupTable
    });

    return response;
}

export function deleteLookupTable(questionnaireId, itemId) {
    var command = {
        questionnaireId: questionnaireId,
        lookupTableId: itemId
    };

    return commandCall('DeleteLookupTable', command).then(response => {
        emitter.emit('lookupTableDeleted', {
            lookupTableId: itemId
        });
    });
}
