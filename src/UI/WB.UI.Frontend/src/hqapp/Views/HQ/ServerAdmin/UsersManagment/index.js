import UsersManagement from './UsersManagement'

export default class MapComponent {
    get routes() {
        return [{
            path: '/UsersManagement',
            component: UsersManagement,
            name: 'users-management',
        }]
    }
    initialize() {

    }
}
