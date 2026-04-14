import { mande } from 'mande'

const api = mande('/api/questionnaire' /*, globalOptions*/)

class WebTesterApi{

  async run(questionnaireId, scenarioId) {
      var webTesterWindow = window.open("about:blank", '_blank');

      const response = await api.get('webTest/' + questionnaireId, { responseAs: 'response' })
      const url = await response.json()
      const jwt = response.headers.get('X-WebTester-Token')
      this.setLocation(webTesterWindow, url, scenarioId, jwt);
  }

  setLocation(webTesterWindow, url, scenarioId, jwt) {
      const params = []
      if (jwt) params.push('jwt=' + encodeURIComponent(jwt))
      if (scenarioId) params.push('scenarioId=' + scenarioId)
      if (params.length > 0) url += '?' + params.join('&')
      webTesterWindow.location.href = url;
  }

  getScenarioSteps(questionnaireId, scenarioId) {
      return api.get(questionnaireId + '/scenarios/' + scenarioId);
  }
}

export default new WebTesterApi()
