import { mande } from 'mande';
import { uploadFormData } from '../../../services/apiService';

const api = mande('/');

class OptionsApi {
    async getCategoryOptions(questionnaireRev, categoriesId) {
        var params = qs({ categoriesId });

        return await api.get(
            `/questionnaire/GetCategoryOptions/${questionnaireRev}?${params}`
        );
    }

    async getOptions(questionnaireRev, questionId, cascading) {
        var params = qs({ questionnaireRev, questionId, cascading });
        return await api.get(
            `/questionnaire/GetOptions/${questionnaireRev}?${params}`
        );
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

        try {
            return await api.post(
                `/questionnaire/applyoptions/${questionnaireRev}?${params}`,
                {
                    categories
                }
            );
        } catch (error) {
            // Command failures now return a non-2xx status; surface their envelope so callers still read isSuccess/error.
            const body = error && error.body;
            if (body && (body.isSuccess !== undefined || body.IsSuccess !== undefined)) {
                return body;
            }
            throw error;
        }
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
        await api.post('/questionnaire/ResetOptions', null);
    }

    async uploadCategory(file) {
        const formData = new FormData();
        formData.append('csvFile', file);
        return uploadFormData('/questionnaire/EditCategories', formData);
    }

    async uploadOptions(questionnaire, question, file) {
        const formData = new FormData();
        formData.append('id', questionnaire);
        formData.append('questionId', question);
        formData.append('csvFile', file);
        return uploadFormData('/questionnaire/EditOptions', formData);
    }
}

function qs(obj) {
    var esc = encodeURIComponent;

    return Object.keys(obj)
        .map(k => esc(k) + '=' + esc(obj[k]))
        .join('&');
}

export default OptionsApi;
