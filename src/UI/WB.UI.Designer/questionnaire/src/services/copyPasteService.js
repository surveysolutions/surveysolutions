import { commandCall } from './apiService';
import emitter from './emitter';
import { newGuid } from '../helpers/guid';
import { ref, computed } from 'vue';

function getCookie(name) {
    const match = document.cookie.match(new RegExp('(?:^|; )' + name + '=([^;]*)'));
    return match ? JSON.parse(decodeURIComponent(match[1])) : null;
}

function setCookie(name, value, days) {
    const expires = new Date(Date.now() + days * 864e5).toUTCString();
    document.cookie = `${name}=${encodeURIComponent(JSON.stringify(value))}; expires=${expires}; path=/`;
}

function removeCookie(name) {
    document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/`;
}

function hasCookie(name) {
    return document.cookie.split(';').some(c => c.trim().startsWith(name + '='));
}

const readyToPaste = ref(null);

export const canPaste = computed(() => {
    //console.log('canPaste: ' + readyToPaste.value);
    if (readyToPaste.value !== null) {
        return readyToPaste.value;
    }

    readyToPaste.value = hasCookie('itemToCopy');
    return readyToPaste.value;
});

export function copyItem(questionnaireId, item) {
    var itemIdToCopy = item.itemId;

    var itemToCopyType = (item.itemType === 'Group' && item.isRoster === true) ? 'Roster' : item.itemType;

    var itemToCopy = {
        questionnaireId: questionnaireId,
        itemId: itemIdToCopy,
        itemType: itemToCopyType
    };

    removeCookie('itemToCopy');
    setCookie('itemToCopy', itemToCopy, 7);

    readyToPaste.value = true;
}

export async function pasteItemInto(questionnaireId, parentId) {
    var itemToCopy = getCookie('itemToCopy');
    if (!itemToCopy) return;

    const newId = newGuid();

    await pasteItemIntoDetailed(
        questionnaireId,
        parentId,
        itemToCopy.questionnaireId,
        itemToCopy.itemId,
        newId
    );

    return {
        id: newId,
        itemType: itemToCopy.itemType
    };
}

export async function pasteItemAfter(questionnaireId, afterNodeId) {
    var itemToCopy = getCookie('itemToCopy');
    if (!itemToCopy) return;

    const newId = newGuid();

    var command = {
        sourceQuestionnaireId: itemToCopy.questionnaireId,
        sourceItemId: itemToCopy.itemId,
        entityId: newId,
        questionnaireId: questionnaireId,
        itemToPasteAfterId: afterNodeId
    };

    await commandCall('PasteAfter', command);

    emitter.emit('itemPasted', {
        questionnaireId: questionnaireId,
        afterNodeId: afterNodeId
    });

    return {
        id: newId,
        itemType: itemToCopy.itemType
    };
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
