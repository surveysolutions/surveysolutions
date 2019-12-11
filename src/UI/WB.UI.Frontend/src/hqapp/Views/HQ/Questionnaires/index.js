import Details from "./Details"

export default class MapComponent {
    get routes() {
        return [{
            path: '/Questionnaires/Details/:questionnaireId', component: Details
        }]
    }
}
