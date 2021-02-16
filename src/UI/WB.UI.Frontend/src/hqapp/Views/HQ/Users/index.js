import Vue from 'vue'

import Headquarters from './Headquarters'
import Manage from './Manage'
import UserWorkspaces from './UserWorkspaces'
import Observers from './Observers'
import ApiUsers from './ApiUsers'
import Create from './Create'
import Supervisors from './Supervisors'
import Interviewers from './Interviewers'
import TwoFactorAuthentication from './TwoFactorAuthentication'
import SetupAuthenticator from './SetupAuthenticator'
import ResetAuthenticator from './ResetAuthenticator'
import ShowRecoveryCodes from './ShowRecoveryCodes'
import ResetRecoveryCodes from './ResetRecoveryCodes'
import Disable2fa from './Disable2fa'
import ChangePassword from './ChangePassword'
import ProfileLayout from './ProfileLayout'

Vue.component('ProfileLayout', ProfileLayout)

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
            path: '/Create/:role', component: Create,
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
        }]
    }
}
