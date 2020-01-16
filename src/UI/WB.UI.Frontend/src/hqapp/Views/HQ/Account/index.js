import Manage from "./Manage"
import Create from "./Create"

export default class AccountComponent {
    get routes() {
        return [{
            path: '/Account/Manage/:userId', component: Manage
        },
        {
            path: '/Account/Manage/', component: Manage
        },
        {
            path: '/Account/Create/:role', component: Create
        }]
    }
}
