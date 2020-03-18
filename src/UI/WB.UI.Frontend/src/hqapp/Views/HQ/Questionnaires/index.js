import Details from './Details'
import Clone from './Clone'

export default class MapComponent {
    get routes() {
        return [{
            path: '/Questionnaires/Details/:questionnaireId', component: Details,
        },
        {
            path: '/Questionnaires/Clone/:questionnaireId', component: Clone,
        }]
    }
}
