import { mande } from 'mande'

const api = mande('/api/questionnaire/verify/' /*, globalOptions*/)

class VerificationApi{

  async verify(questionnaireId) {
    return api.get(questionnaireId);
  }
}

export default new VerificationApi()
