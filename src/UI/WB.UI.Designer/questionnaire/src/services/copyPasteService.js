import { commandCall } from './apiService';
import emitter from './emitter';
import { newGuid } from '../helpers/guid';
import { ref, computed } from 'vue';
import { getCookie, setCookie, removeCookie, hasCookie } from '../helpers/cookies';
const readyToPaste = ref(null);

function createSourceQuestionnaireRevision(questionnaireId, questionnaireRevision) {
    if (!questionnaireId) return null;

    return questionnaireRevision
        ? { questionnaireId, revision: questionnaireRevision }
        : { questionnaireId };
}

export const canPaste = computed(() => {
    //console.log('canPaste: ' + readyToPaste.value);
    if (readyToPaste.value !== null) {
        return readyToPaste.value;
    }

    readyToPaste.value = hasCookie('itemToCopy');
    return readyToPaste.value;
});

export function copyItem(questionnaire, item) {
    var itemIdToCopy = item.itemId;

    var itemToCopyType = (item.itemType === 'Group' && item.isRoster === true) ? 'Roster' : item.itemType;

    var itemToCopy = {
        questionnaireId: questionnaire.questionnaireId,
        questionnaireRevision: createSourceQuestionnaireRevision(
            questionnaire.questionnaireId,
            questionnaire.questionnaireRevision
        ),
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
        newId,
        itemToCopy.questionnaireRevision
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
        sourceQuestionnaireRevision: itemToCopy.questionnaireRevision,
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
    newId,
    sourceQuestionnaireRevision = null
) {
    var command = {
        sourceQuestionnaireId: sourceQuestionnaireId,
        sourceQuestionnaireRevision: sourceQuestionnaireRevision,
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
