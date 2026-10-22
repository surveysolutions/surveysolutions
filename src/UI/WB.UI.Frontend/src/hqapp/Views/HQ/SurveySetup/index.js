const UpgradeAssignments = () => import('./UpgradeAssignments')
const UpgradeProgress = () => import('./UpgradeProgress')
const Questionnaires = () => import('./Questionnaires')

export default class MapComponent {
    get routes() {
        return [{
            path: '/SurveySetup/UpgradeAssignments/:questionnaireId',
            component: UpgradeAssignments,
        },
        {
            path: '/SurveySetup/UpgradeProgress/:processId',
            component: UpgradeProgress,
        },
        {
            path: '/SurveySetup',
            component: Questionnaires,
        },
        ]
    }
}
