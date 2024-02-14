import { commandCall } from './apiService';

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
