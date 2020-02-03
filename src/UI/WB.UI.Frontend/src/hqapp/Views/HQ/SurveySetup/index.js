import UpgradeAssignments from './UpgradeAssignments'
import UpgradeProgress from './UpgradeProgress'
import Questionnaires from './Questionnaires'

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
