import axios from 'axios';

axios.defaults.headers.common['Content-Type'] = 'application/json';

class OptionsApi {
    async getCategoryOptions(questionnaireRev, categoriesId) {
        var params = qs({ categoriesId });

        const response = await axios.get(
            `/questionnaire/GetCategoryOptions/${questionnaireRev}?${params}`
        );
        return response.data;
    }

    async getOptions(questionnaireRev, questionId, cascading) {
        var params = qs({ questionnaireRev, questionId, cascading });
        const response = await axios.get(
            `/questionnaire/GetOptions/${questionnaireRev}?${params}`
        );
        return response.data;
    }

    async applyOptions(
        categories,
        questionnaireRev,
        entityId,
        isCascading,
        isCategory
    ) {
        var params = qs({
            questionnaireRev,
            entityId,
            isCascading,
            isCategory
        });

        const response = await axios.post(
            `/questionnaire/applyoptions/${questionnaireRev}?${params}`,
            {
                categories
            }
        );

        return response.data;
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
        await axios.post('/questionnaire/ResetOptions');
    }

    async uploadCategory(file) {
        const formData = new FormData();

        formData.append('csvFile', file);

        return await axios.post(`/questionnaire/EditCategories`, formData);
    }

    async uploadOptions(questionnaire, question, file) {
        const formData = new FormData();
        formData.append('id', questionnaire);
        formData.append('questionId', question);
        formData.append('csvFile', file);

        return await axios.post('/questionnaire/EditOptions', formData);
    }
}

function qs(obj) {
    var esc = encodeURIComponent;

    return Object.keys(obj)
        .map(k => esc(k) + '=' + esc(obj[k]))
        .join('&');
}

export default OptionsApi;
