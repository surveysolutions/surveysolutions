import { get, post } from './apiService';

export function updateExportPdfStatus(questionnaireId, translationId) {
    return get(
        '/pdf/statusPdf/' +
            questionnaireId +
            '?timezoneOffsetMinutes=' +
            new Date().getTimezoneOffset() +
            '&translation=' +
            translationId
    );
}

export function retryExportPdf(questionnaireId, translationId) {
    return post('/pdf/retry/' + questionnaireId, {
        translation: translationId
    });
}
