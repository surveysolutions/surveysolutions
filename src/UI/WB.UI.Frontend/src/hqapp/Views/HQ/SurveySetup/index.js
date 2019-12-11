import UpgradeAssignments from "./UpgradeAssignments"
import UpgradeProgress from "./UpgradeProgress"

export default class MapComponent {
    get routes() {
        return [{
            path: '/SurveySetup/UpgradeAssignments/:questionnaireId',
            component: UpgradeAssignments
        },
        {
            path: '/SurveySetup/UpgradeProgress/:processId',
            component: UpgradeProgress
        }]
    }
}
