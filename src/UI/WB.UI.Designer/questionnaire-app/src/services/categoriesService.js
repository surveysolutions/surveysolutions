import { upload, commandCall } from './apiService';
import emitter from './emitter';

export async function updateCategories(questionnaireId, categories) {
    var command = {
        questionnaireId: questionnaireId,
        categoriesId: categories.categoriesId,
        oldCategoriesId: categories.oldCategoriesId,
        name: categories.name
    };

    return upload('/api/command/categories', categories.file, command).then(
        response => {
            emitter.emit('categoriesUpdated', {
                categories: categories
            });
        }
    );
}

export function deleteCategories(questionnaireId, categoriesId) {
    var command = {
        questionnaireId: questionnaireId,
        categoriesId: categoriesId
    };
    return commandCall('DeleteCategories', command).then(response => {
        emitter.emit('categoriesDeleted', {
            categoriesId: categoriesId
        });
    });
}
