import { mande } from 'mande'

const api = mande('/api/questionnaire' /*, globalOptions*/)

class WebTesterApi{

  async run(questionnaireId, scenarioId) {
      var webTesterWindow = window.open("about:blank", '_blank');

      // Designer returns the full redirect URL with ?code=<one-time-code> already embedded.
      // JWT never appears in the browser — code is exchanged server-to-server by WebTester.
      const url = await api.get('webTest/' + questionnaireId)
      this.setLocation(webTesterWindow, url, scenarioId);
  }

  setLocation(webTesterWindow, url, scenarioId) {
      if (scenarioId) {
          const separator = url.includes('?') ? '&' : '?'
          url += separator + 'scenarioId=' + scenarioId
      }
      webTesterWindow.location.href = url;
  }

  getScenarioSteps(questionnaireId, scenarioId) {
      return api.get(questionnaireId + '/scenarios/' + scenarioId);
  }
}

export default new WebTesterApi()
