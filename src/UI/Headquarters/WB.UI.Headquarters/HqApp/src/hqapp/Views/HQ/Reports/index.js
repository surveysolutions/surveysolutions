import InterviewersAndDevices from "./InterviewersAndDevices";
import StatusDuration from "./StatusDuration";
import MapReport from "./MapReport";
import SurveyStatistics from "./SurveyStatistics";
import Vue from 'vue'

export default class ReportComponent {
    constructor(rootStore) {
        this.rootStore = rootStore;
    }

    get routes() {
        return [{
                path: "/Reports/InterviewersAndDevices",
                component: InterviewersAndDevices
            }, {
                path: "/Reports/StatusDuration",
                component: StatusDuration
            }, {
                path: "/Reports/MapReport",
                component: MapReport
            }, {
                name: "surveyStatistics",
                path: "/Reports/surveyStatistics",
                component: SurveyStatistics,
                props: (route) => ({ 
                    questionnaireId: route.query.questionnaire,
                    min: parseInt(route.query.min),
                    max: parseInt(route.query.max),
                    questionId: route.query.question,
                    detailedView: String(route.query.detailedView).toLowerCase() === 'true'
                 })
            }
        ]
    }

    initialize() {
        const VeeValidate = require('vee-validate');
        Vue.use(VeeValidate);
    }    
}
