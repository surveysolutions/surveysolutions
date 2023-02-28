import axios from 'axios';

axios.defaults.headers.common['Content-Type'] = 'application/json';

class QuestionnaireService {

  urlBase = '../../api/questionnaire';

  async getQuestionnaireById(questionnaireId) {
    var url = string.format('{0}/get/{1}', urlBase, questionnaireId);
    const response = await axios.get(url);

    return response.data;
  };
}

export default QuestionnaireService
