//import Vue from 'vue'

const Headquarters = () => import('./Headquarters')
const Manage = () => import('./Manage')
const UserWorkspaces = () => import('./UserWorkspaces')
const Observers = () => import('./Observers')
const ApiUsers = () => import('./ApiUsers')
const Create = () => import('./Create')
const Supervisors = () => import('./Supervisors')
const Interviewers = () => import('./Interviewers')
const TwoFactorAuthentication = () => import('./TwoFactorAuthentication')
const SetupAuthenticator = () => import('./SetupAuthenticator')
const ResetAuthenticator = () => import('./ResetAuthenticator')
const ShowRecoveryCodes = () => import('./ShowRecoveryCodes')
const ResetRecoveryCodes = () => import('./ResetRecoveryCodes')
const Disable2fa = () => import('./Disable2fa')
const ChangePassword = () => import('./ChangePassword')
const ApiTokens = () => import('./ApiTokens')

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
            path: '/Manage/:userId', component: Manage, name: 'usersManage',
        },
        {
            path: '/Manage/', component: Manage,
        },
        {
            path: '/Create', component: Create,
        },
        {
            path: '/TwoFactorAuthentication', component: TwoFactorAuthentication,
        },
        {
            path: '/TwoFactorAuthentication/:userId', component: TwoFactorAuthentication, name: 'users2fa',
        },
        {
            path: '/SetupAuthenticator', component: SetupAuthenticator,
        },
        {
            path: '/SetupAuthenticator/:userId',
            name: 'two-fa-setup',
            component: SetupAuthenticator,
        },
        {
            path: '/ResetAuthenticator', component: ResetAuthenticator,
        },
        {
            path: '/ResetAuthenticator/:userId',
            name: 'two-fa-reset-authenticator',
            component: ResetAuthenticator,
        },
        {
            path: '/ShowRecoveryCodes', component: ShowRecoveryCodes,
        },
        {
            path: '/ShowRecoveryCodes/:userId', component: ShowRecoveryCodes,
        },
        {
            path: '/ResetRecoveryCodes', component: ResetRecoveryCodes,
        },
        {
            path: '/ResetRecoveryCodes/:userId',
            name: 'two-fa-reset-recovery-codes',
            component: ResetRecoveryCodes,
        },
        {
            path: '/Disable2fa',
            //name: 'two-fa-disable',
            component: Disable2fa,
        },
        {
            path: '/Disable2fa/:userId',
            name: 'two-fa-disable',
            component: Disable2fa,
        },
        {
            path: '/ChangePassword/:userId', component: ChangePassword, name: 'usersChangePassword',
        },
        {
            path: '/Workspaces/:userId', component: UserWorkspaces,
        },
        {
            path: '/ChangePassword/', component: ChangePassword,
        },
        {
            path: '/ApiTokens/:userId', component: ApiTokens, name: 'usersApiTokens',
        },
        {
            path: '/ApiTokens', component: ApiTokens,
        }]
    }
}
