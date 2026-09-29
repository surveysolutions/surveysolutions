const Details = () => import('./Details')
const Clone = () => import('./Clone')
const ExposedVariables = () => import('./ExposedVariables')

export default class MapComponent {
    get routes() {
        return [{
            path: '/Questionnaires/Details/:questionnaireId', component: Details, name: 'questionnairedetails',
        },
        {
            path: '/Questionnaires/Clone/:questionnaireId', component: Clone,
        },
        {
            path: '/Questionnaires/ExposedVariables/:questionnaireId', component: ExposedVariables,
        }]
    }
}
