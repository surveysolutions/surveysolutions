import UpgradeAssignments from "./UpgradeAssignments"

export default class MapComponent {
    get routes() {
        return [{
            path: '/SurveySetup/UpgradeAssignments/:questionnaireId',
            component: UpgradeAssignments
        }]
    }
}
