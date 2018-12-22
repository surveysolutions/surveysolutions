import axios from "axios";

class QuestionnaireApi {
    constructor(questionnaireId, version, http) {
        this.http = http
        this.base = "api/v1/questionnaires/"
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
        this.reportPath = "api/v1/statistics"
        this.http = http
    }

    async Questions(questionnaireId, version) {
        return (await this.http.get(`${this.reportPath}/questions`, {
            params: { questionnaireId, version }
        })).data
    }

    async Questionnaires() {
        return (await this.http.get(`${this.reportPath}/questionnaires`)).data
    }

    get Uri() {
        return this.http.defaults.baseURL + '/' + this.reportPath
    }
}

class MapsReport {
    constructor(http) {
        this.http = http
    }

    async GpsQuestionsByQuestionnaire(questionnaireId, version){ 
        return await this.http.get(`api/ReportDataApi/QuestionInfo/${questionnaireId}?version=${version}`)
    }

    async Report(request) {
        return await this.http.post('api/ReportDataApi/MapReport', request)
    }

    async InteriewSummaryUrl(interviewId) {
        var response = await this.http.post('api/InterviewApi/InterviewSummaryForMapPoint', 
        {
            interviewId
        })

        return response
    }

    GetInterviewDetailsUrl(interviewId) {
        return `${this.http.defaults.baseURL}/Interview/Review/${interviewId}`
    }
}

class Users {
    constructor(http){
        this.http = http
    }

    async Supervisors(filter) {
        return (await this.http.get(`api/v1/users/supervisors`, {
            params: { filter }
        })).data
    }

    get SupervisorsUri() {
        return this.http.defaults.baseURL + 'api/v1/users/supervisors'
    }
}

class Reports {
    constructor(http, basePath){
        this.http = http
        this.basePath = basePath
    }

    get SurveyStatistics() {
        return new SurveyStatistics(this.http)
    }

    get MapReport() {
        return new MapsReport(this.http)
    }
}

class HqApiClient {
    constructor(basePath) {
        this.basePath = basePath;

        this.http = axios.create({
            baseURL: basePath
        });
    }

    Questionnaire(questionnaireId, version) {
        return new QuestionnaireApi(questionnaireId, version, this.http)
    }

    get Report() { return new Reports(this.http) }

    get Users() { return new Users(this.http) }
}

/*  the Plugin */
export default {
    install: function(vue) {
        const instance = new HqApiClient(vue.$config.basePath);

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
