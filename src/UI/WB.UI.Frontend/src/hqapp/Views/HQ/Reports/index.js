import InterviewersAndDevices from './InterviewersAndDevices'
import StatusDuration from './StatusDuration'
import MapReport from './MapReport'
import SurveyStatistics from './SurveyStatistics'
import TeamsAndStatuses from './TeamsAndStatuses'
import CumulativeChart from './CumulativeChartReport'
import SurveysAndStatuses from './SurveysAndStatuses'
import SurveysAndStatusesForSv from './SurveysAndStatusesForSv'
import SpeedAndQuantity from './SpeedAndQuantity'

export default class ReportComponent {
    constructor(rootStore) {
        this.rootStore = rootStore
    }

    get routes() {
        return [
            {
                path: '/Reports/InterviewersAndDevices/:supervisorId',
                component: InterviewersAndDevices,
            },
            {
                path: '/Reports/InterviewersAndDevices',
                component: InterviewersAndDevices,
            },
            {
                path: '/Reports/StatusDuration',
                component: StatusDuration,
            },
            {
                path: '/Reports/TeamStatusDuration',
                component: StatusDuration,
            },
            {
                path: '/Reports/SupervisorsAndStatuses',
                component: TeamsAndStatuses,
            },
            {
                path: '/Reports/SurveysAndStatuses',
                component: SurveysAndStatuses,
            },
            {
                path: '/Reports/SurveysAndStatusesForSv',
                component: SurveysAndStatusesForSv,
            },
            {
                path: '/Reports/TeamMembersAndStatuses',
                component: TeamsAndStatuses,
            },
            {
                path: '/Reports/InterviewsChart',
                component: CumulativeChart,
            },
            {
                path: '/Reports/MapReport',
                component: MapReport,
            },
            {
                path: '/Reports/QuantityBySupervisors',
                component: SpeedAndQuantity,
            },
            {
                path: '/Reports/SpeedBySupervisors',
                component: SpeedAndQuantity,
            },
            {
                path: '/Reports/SpeedByInterviewers',
                component: SpeedAndQuantity,
            },
            {
                path: '/Reports/QuantityByInterviewers',
                component: SpeedAndQuantity,
            },
            {
                name: 'surveyStatistics',
                path: '/Reports/surveyStatistics',
                component: SurveyStatistics,
                props: (route) => ({
                    questionnaireId: route.query.questionnaire,
                    min: parseInt(route.query.min),
                    max: parseInt(route.query.max),
                    questionId: route.query.question,
                    detailedView: String(route.query.detailedView).toLowerCase() === 'true',
                }),
            },
        ]
    }

    get modules() {
        return {}
    }
}
