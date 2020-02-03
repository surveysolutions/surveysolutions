import Supervisor from './Supervisor'
import Interviewer from './Interviewer'

export default class UsersComponent {
    constructor(rootStore) {
        this.rootStore = rootStore
    }

    get routes() {
        return [
            {
                path: '/Download', component: Interviewer,
            },
            {
                path: '/Download/Supervisor', component: Supervisor,
            },
        ]
    }
}
