import { commandCall } from './apiService';
import emitter from './emitter';
import { newGuid } from '../helpers/guid';
import { useCookies } from 'vue3-cookies';
import { ref, computed } from 'vue';

const readyToPaste = ref(null);

export const canPaste = computed(() => {
    //console.log('canPaste: ' + readyToPaste.value);
    if (readyToPaste.value !== null) {
        return readyToPaste.value;
    }

    const cookies = useCookies();
    readyToPaste.value = cookies.cookies.isKey('itemToCopy');
    return readyToPaste.value;
});

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

    readyToPaste.value = true;
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

export async function pasteItemAfter(questionnaireId, afterNodeId) {
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
