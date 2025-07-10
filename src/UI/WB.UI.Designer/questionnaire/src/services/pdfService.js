import { getSilently, post } from './apiService';

export function updateExportPdfStatus(questionnaireId, translationId) {
    return getSilently(
        '/pdf/statusPdf/' +
            questionnaireId +
            '?timezoneOffsetMinutes=' +
            new Date().getTimezoneOffset() +
            '&translation=' +
            encodeURIComponent(translationId)
    );
}

export function retryExportPdf(questionnaireId, translationId) {
    return post('/pdf/retry/' + questionnaireId, {
        translation: translationId
    });
}
