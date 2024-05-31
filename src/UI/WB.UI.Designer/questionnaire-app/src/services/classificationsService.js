import { get, commandCall } from './apiService';

const baseUrl = '/api/';

export function replaceOptionsWithClassification(
    questionnaireId,
    questionId,
    classificationId
) {
    var command = {
        questionnaireId: questionnaireId,
        questionId: questionId,
        classificationId: classificationId
    };
    return commandCall('ReplaceOptionsWithClassification', command);
}

export function loadClassificationGroups() {
    return get(baseUrl + 'classifications/groups');
}

export function loadCategories(classificationId) {
    return get(
        baseUrl +
            'classifications/classification/' +
            classificationId +
            '/categories'
    );
}

export function search(groupId, searchText, privateOnly) {
    return get(baseUrl + 'classifications/classifications/search', {
        query: searchText,
        groupId: groupId,
        privateOnly: privateOnly || false
    });
}
