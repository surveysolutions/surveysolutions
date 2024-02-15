import { upload, commandCall } from './apiService';
import emitter from './emitter';

export async function updateCategories(questionnaireId, categories) {
    var command = {
        questionnaireId: questionnaireId,
        categoriesId: categories.categoriesId,
        oldCategoriesId: categories.oldCategoriesId,
        name: categories.name
    };

    const response = await upload(
        '/api/command/categories',
        categories.file,
        command
    );
    emitter.emit('categoriesUpdated', {
        categories: categories
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
