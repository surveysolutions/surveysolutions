import Interviews from './HqInterviews'

export default class InterviewsComponent {
    constructor(rootStore) {
        this.rootStore = rootStore
    }
    get routes() {
        return [{
            name: 'interviews',
            path: '/Interviews/',
            component: Interviews,
        },
        ]
    }
    initialize() { }
}
