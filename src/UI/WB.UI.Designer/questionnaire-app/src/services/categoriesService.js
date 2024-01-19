import { upload, commandCall } from './apiService';

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

    return response;
}

export function deleteCategories(questionnaireId, categoriesId) {
    var command = {
        questionnaireId: questionnaireId,
        categoriesId: categoriesId
    };
    return commandCall('DeleteCategories', command);
}
