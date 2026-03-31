import { mande } from 'mande';

const api = mande('/');

class OptionsApi {
    async getCategoryOptions(questionnaireRev, categoriesId) {
        return await api.get(`questionnaire/GetCategoryOptions/${questionnaireRev}`, {
            query: { categoriesId }
        });
    }

    async getOptions(questionnaireRev, questionId, cascading) {
        return await api.get(`questionnaire/GetOptions/${questionnaireRev}`, {
            query: { questionnaireRev, questionId, cascading }
        });
    }

    async applyOptions(
        categories,
        questionnaireRev,
        entityId,
        isCascading,
        isCategory
    ) {
        return await api.post(
            `questionnaire/applyoptions/${questionnaireRev}`,
            { categories },
            { query: { questionnaireRev, entityId, isCascading, isCategory } }
        );
    }

    getExportOptionsAsTabUri(
        questionnaireRev,
        entityId,
        isCategory,
        isCascading
    ) {
        const params = qs({
            type: 'tsv',
            entityId,
            isCategory,
            isCascading
        });
        return `/questionnaire/ExportOptions/${questionnaireRev}?${params}`;
    }

    getExportOptionsAsExlsUri(
        questionnaireRev,
        entityId,
        isCategory,
        isCascading
    ) {
        const params = qs({
            type: 'xlsx',
            entityId,
            isCategory,
            isCascading
        });
        return `/questionnaire/ExportOptions/${questionnaireRev}?${params}`;
    }

    async resetOptions() {
        await api.post('questionnaire/ResetOptions');
    }

    async uploadCategory(file) {
        const formData = new FormData();
        formData.append('csvFile', file);
        const uploadApi = mande('/questionnaire/EditCategories', { headers: { 'Content-Type': null } });
        return await uploadApi.post(formData);
    }

    async uploadOptions(questionnaire, question, file) {
        const formData = new FormData();
        formData.append('id', questionnaire);
        formData.append('questionId', question);
        formData.append('csvFile', file);
        const uploadApi = mande('/questionnaire/EditOptions', { headers: { 'Content-Type': null } });
        return await uploadApi.post(formData);
    }
}

function qs(obj) {
    var esc = encodeURIComponent;

    return Object.keys(obj)
        .map(k => esc(k) + '=' + esc(obj[k]))
        .join('&');
}

export default OptionsApi;
