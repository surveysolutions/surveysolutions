import Headquarters from './Headquarters'
import Manage from './Manage'
import Observers from './Observers'
import ApiUsers from './ApiUsers'
import Create from './Create'
import Supervisors from './Supervisors'
import Interviewers from './Interviewers'

export default class UsersComponent {
    constructor(rootStore) {
        this.rootStore = rootStore
    }

    get routes() {
        return [{
            path: '/Headquarters', component: Headquarters,
        },
        {
            path: '/Observers', component: Observers,
        },
        {
            path: '/Interviewers', component: Interviewers,
        },
        {
            path: '/ApiUsers', component: ApiUsers,
        },
        {
            path: '/Supervisors', component: Supervisors,
        },
        {
            path: '/Users/Manage/:userId', component: Manage,
        },
        {
            path: '/Users/Manage/', component: Manage,
        },
        {
            path: '/Users/Create/:role', component: Create,
        }]
    }
}
