import axios from 'axios';

axios.defaults.headers.common['Content-Type'] = 'application/json';

class OptionsApi {
    async getCategoryOptions(questionnaireRev, categoryId) {
        const response = await axios.get(
            `/questionnaire/GetCategoryOptions/${questionnaireRev}?categoriesId=${categoryId}`
        );
        return response.data;
    }

    async getOptions(questionnaireRev, questionId, cascading) {
        const response = await axios.get(
            `/questionnaire/GetOptions/${questionnaireRev}?questionId=${questionId}&isCascading=${cascading}`
        );
        return response.data;
    }

    async applyOptions(categories) {
        const response = await axios.post(`/questionnaire/applyoptions`, {
            categories
        });

        return response.data;
    }

    async resetOptions() {
        await axios.post('/questionnaire/ResetOptions');
    }
}

export default OptionsApi;
