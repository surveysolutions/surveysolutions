import axios from 'axios';
import utilityService from './utilityService';

axios.defaults.headers.common['Content-Type'] = 'application/json';

class QuestionnaireService {

  urlBase = '../../api/questionnaire';  

  async getQuestionnaireById(questionnaireId) {
    var url = utilityService.format('{0}/get/{1}', this.urlBase, questionnaireId);
    const response = await axios.get(url);

    return response.data;
  };
}

export default new QuestionnaireService()
