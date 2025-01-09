import { mande } from 'mande'

const api = mande('/api/questionnaire' /*, globalOptions*/)

class WebTesterApi{

  async run(questionnaireId, scenarioId) {
      var webTesterWindow = window.open("about:blank", '_blank');

      const url = await api.get('webTest/' + questionnaireId)
      this.setLocation(webTesterWindow, url, scenarioId);
  }

  setLocation(webTesterWindow, url, scenarioId) {
      if (scenarioId) {
          url += "?scenarioId=" + scenarioId;
      }
      webTesterWindow.location.href = url;
  }

  getScenarioSteps(questionnaireId, scenarioId) {
      return api.get(questionnaireId + '/scenarios/' + scenarioId);
  }
}

export default new WebTesterApi()
