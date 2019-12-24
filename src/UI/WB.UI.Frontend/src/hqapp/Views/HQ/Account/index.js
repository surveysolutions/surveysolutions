import Manage from "./Manage"

export default class AccountComponent {
    get routes() {
        return [{
            path: '/Account/Manage/', component: Manage
        }]
    }
}
