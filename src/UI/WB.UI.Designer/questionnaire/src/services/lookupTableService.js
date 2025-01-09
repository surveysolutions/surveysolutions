import { upload, commandCall } from './apiService';
import emitter from './emitter';
import { newGuid } from '../helpers/guid';

export async function addLookupTable(questionnaireId, lookupTable) {
    var command = {
        questionnaireId: questionnaireId,
        lookupTableId: lookupTable.itemId
    };
    return commandCall('AddLookupTable', command).then(response => {
        emitter.emit('lookupTableUpdated', {
            lookupTable: lookupTable
        });
    });
}

export async function updateLookupTable(questionnaireId, lookupTable) {
    var command = {
        questionnaireId: questionnaireId,
        lookupTableId: lookupTable.itemId,
        lookupTableName: lookupTable.name,
        lookupTableFileName: lookupTable.fileName
    };

    command.oldLookupTableId = lookupTable.itemId;
    command.lookupTableId = newGuid();

    const response = await upload(
        '/api/command/UpdateLookupTable',
        lookupTable.file,
        command
    );

    //lookupTable.meta.lastUpdated = new Date();

    emitter.emit('lookupTableUpdated', {
        lookupTable: lookupTable,
        newId: command.oldLookupTableId ? command.lookupTableId : null
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
            id: itemId
        });
    });
}
