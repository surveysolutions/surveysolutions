import { mande } from 'mande'

const api = mande('/api/questionnaire' /*, globalOptions*/)

class WebTesterApi{

  openWindow() {
      return window.open("about:blank", '_blank');
  }

  async run(questionnaireId, scenarioId, webTesterWindow = this.openWindow()) {

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
