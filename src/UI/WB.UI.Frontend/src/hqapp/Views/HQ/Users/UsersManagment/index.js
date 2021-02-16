import UsersManagement from './UsersManagement'
import CreateUser from './CreateUser'


export default class UsersManagementComponent {
    get routes() {
        return [
            {
                path: '/UsersManagement',
                component: UsersManagement,
                name: 'users-management',
            },
        ]
    }
    initialize() {

    }
}
