import UsersManagement from './UsersManagement'
import CreateUser from './CreateUser'


export default class MapComponent {
    get routes() {
        return [
            {
                path: '/UsersManagement',
                component: UsersManagement,
                name: 'users-management',
            },
            {
                path: '/CreateUser',
                component: CreateUser,
                name: 'create-user',
            },
        ]
    }
    initialize() {

    }
}
