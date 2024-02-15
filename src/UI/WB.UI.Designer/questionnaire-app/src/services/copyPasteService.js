import { commandCall } from './apiService';
import emitter from './emitter';
import { newGuid } from '../helpers/guid';
import { useCookies } from 'vue3-cookies';

let readyToPaste = null;

export function copyItem(questionnaireId, item) {
    const cookies = useCookies();

    var itemIdToCopy = item.itemId;

    var itemToCopy = {
        questionnaireId: questionnaireId,
        itemId: itemIdToCopy,
        itemType: item.itemType
    };

    cookies.cookies.remove('itemToCopy');
    cookies.cookies.set('itemToCopy', itemToCopy, { expires: 7 });

    readyToPaste = true;
}

export function canPaste() {
    if (readyToPaste != null) return readyToPaste;
    const cookies = useCookies();
    readyToPaste = cookies.cookies.isKey('itemToCopy');
    return readyToPaste;
}

export function pasteItemInto(questionnaireId, parentId) {
    const cookies = useCookies();

    var itemToCopy = cookies.cookies.get('itemToCopy');
    if (!itemToCopy) return;

    const newId = newGuid();

    return pasteItemIntoDetailed(
        questionnaireId,
        parentId,
        itemToCopy.questionnaireId,
        itemToCopy.itemId,
        newId
    );
}

export function pasteItemAfter(questionnaireId, afterNodeId) {
    const cookies = useCookies();

    var itemToCopy = cookies.cookies.get('itemToCopy');
    if (!itemToCopy) return;

    const newId = newGuid();

    var command = {
        sourceQuestionnaireId: itemToCopy.questionnaireId,
        sourceItemId: itemToCopy.itemId,
        entityId: newId,
        questionnaireId: questionnaireId,
        itemToPasteAfterId: afterNodeId
    };

    return commandCall('PasteAfter', command).then(() =>
        emitter.emit('itemPasted', {
            questionnaireId: questionnaireId,
            afterNodeId: afterNodeId
        })
    );
}

export function pasteItemIntoDetailed(
    questionnaireId,
    parentId,
    sourceQuestionnaireId,
    sourceItemId,
    newId
) {
    var command = {
        sourceQuestionnaireId: sourceQuestionnaireId,
        sourceItemId: sourceItemId,
        parentId: parentId,
        entityId: newId,
        questionnaireId: questionnaireId
    };

    return commandCall('PasteInto', command).then(() =>
        emitter.emit('itemPasted', {
            questionnaireId: questionnaireId,
            parentId: parentId
        })
    );
}
