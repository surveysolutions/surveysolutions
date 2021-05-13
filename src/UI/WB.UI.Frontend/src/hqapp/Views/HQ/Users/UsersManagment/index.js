import UsersManagement from './UsersManagement'

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
