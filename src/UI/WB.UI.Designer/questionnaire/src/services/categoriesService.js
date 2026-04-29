import { upload, commandCall } from './apiService';
import emitter from './emitter';
import { newGuid } from '../helpers/guid';

export async function updateCategories(
    questionnaireId,
    categories,
    isNew = false
) {
    var command = {
        questionnaireId: questionnaireId,
        categoriesId: categories.categoriesId,
        name: categories.name
    };

    if (!isNew && categories.file !== null && categories.file !== undefined) {
        command.oldCategoriesId = categories.categoriesId;
        command.categoriesId = newGuid();
    }
    const response = await upload(
        '/api/command/categories',
        categories.file,
        command
    );
    emitter.emit('categoriesUpdated', {
        categories: categories,
        newId: command.oldCategoriesId ? command.categoriesId : null
    });

    return response;
}

export function deleteCategories(questionnaireId, categoriesId) {
    var command = {
        questionnaireId: questionnaireId,
        categoriesId: categoriesId
    };
    return commandCall('DeleteCategories', command).then(response => {
        emitter.emit('categoriesDeleted', {
            id: categoriesId
        });
    });
}

export async function copyCategories(targetQuestionnaireId, sourceQuestionnaireId, sourceCategoriesId, name) {
    const newCategoriesId = newGuid();
    const command = {
        questionnaireId: targetQuestionnaireId,
        sourceQuestionnaireId: sourceQuestionnaireId,
        sourceCategoriesId: sourceCategoriesId,
        newCategoriesId: newCategoriesId,
        name: name
    };
    await commandCall('CopyCategories', command);
    emitter.emit('categoriesUpdated', {
        categories: { categoriesId: newCategoriesId, name: name },
        newId: null
    });
    return newCategoriesId;
}
