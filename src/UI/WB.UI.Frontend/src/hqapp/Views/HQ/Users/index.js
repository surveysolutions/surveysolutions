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
            path: '/Users/Manage/:userId', component: Manage, name: 'usersManage',
        },
        {
            path: '/Users/Manage/', component: Manage,
        },
        {
            path: '/Users/Create/:role', component: Create,
        },
        {
            path: '/Users/TwoFactorAuthentication', component: TwoFactorAuthentication,
        },
        {
            path: '/Users/TwoFactorAuthentication/:userId', component: TwoFactorAuthentication, name: 'users2fa',
        },
        {
            path: '/Users/SetupAuthenticator', component: SetupAuthenticator,
        },
        {
            path: '/Users/SetupAuthenticator/:userId',
            name: 'two-fa-setup',
            component: SetupAuthenticator,
        },
        {
            path: '/Users/ResetAuthenticator', component: ResetAuthenticator,
        },
        {
            path: '/Users/ResetAuthenticator/:userId',
            name: 'two-fa-reset-authenticator',
            component: ResetAuthenticator,
        },
        {
            path: '/Users/ShowRecoveryCodes', component: ShowRecoveryCodes,
        },
        {
            path: '/Users/ShowRecoveryCodes/:userId', component: ShowRecoveryCodes,
        },
        {
            path: '/Users/ResetRecoveryCodes', component: ResetRecoveryCodes,
        },
        {
            path: '/Users/ResetRecoveryCodes/:userId',
            name: 'two-fa-reset-recovery-codes',
            component: ResetRecoveryCodes,
        },
        {
            path: '/Users/Disable2fa',
            //name: 'two-fa-disable',
            component: Disable2fa,
        },
        {
            path: '/Users/Disable2fa/:userId',
            name: 'two-fa-disable',
            component: Disable2fa,
        },
        {
            path: '/Users/ChangePassword/:userId', component: ChangePassword, name: 'usersChangePassword',
        },
        {
            path: '/Users/Workspaces/:userId', component: UserWorkspaces,
        },
        {
            path: '/Users/ChangePassword/', component: ChangePassword,
        }]
    }
}
