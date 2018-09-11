import axios from "axios";

class QuestionnaireApi {
    constructor(questionnaireId, version, http) {
        this.http = http
        this.base = "questionnaires/"
        this.details = this.base + `${questionnaireId}/${version}`       
    }

    async List() {
        const result = []
        let offset = 1
        
        while(offset > 0){
            const response = await this.http.get(this.base, { params: { limit: 40, offset}})
            const data = response.data
            offset = data.Offset + 1
            
            data.Questionnaires.forEach(q => result.push(q))

            if(data.Questionnaires == null || data.Questionnaires.length == 0) break
        }

        return result
    }
}

class SurveyStatistics {
    constructor(http) {
        this.reportPath = "statistics"
        this.http = http
    }

    async Questions(questionnaireId) {
        return (await this.http.get(`${this.reportPath}/questions`, {
            params: { questionnaireId }
        })).data
    }

    async Questionnaires() {
        return (await this.http.get(`${this.reportPath}/questionnaires`)).data
    }

    get Uri() {
        return this.http.defaults.baseURL + '/' + this.reportPath
    }
}

class Reports {
    constructor(http, uriOnly = false){
        this.http = http
        this.uriOnly = uriOnly
    }

    get SurveyStatistics() {
        return new SurveyStatistics(this.http)
    }
}

class HqApiClient {
    constructor(basePath, apiPath) {
        this.basePath = basePath;
        this.http = axios.create({
            baseURL: basePath + apiPath
        });
    }

    Questionnaire(questionnaireId, version) {
        return new QuestionnaireApi(questionnaireId, version, this.http)
    }

    get Report() { return new Reports(this.http) }
}

/*  the Plugin */
export default {
    install: function(vue) {
        const instance = new HqApiClient(vue.$config.basePath, "api/v1");

        // /*  expose a global API method  */
        Object.defineProperty(vue, "$hq", {
            get() {
                return instance;
            }
        });

        /*  expose a local API method  */
        Object.defineProperty(vue.prototype, "$hq", {
            get() {
                return instance;
            }
        });
    }
};
