const ControlPanelLayout = () => import( /*  webpackChunkName: "controlpanel" */ './ControlPanelLayout')
const TabletInfos = () => import( /*  webpackChunkName: "controlpanel" */ './TabletInfos')
const AppUpdates = () => import( /*  webpackChunkName: "controlpanel" */ './AppUpdates')
const InterviewPackages = () => import( /*  webpackChunkName: "controlpanel" */ './InterviewPackages')
const ChangePassword = () => import( /*  webpackChunkName: "controlpanel" */ './ChangePassword')
const ReevaluateInterview = () => import( /*  webpackChunkName: "controlpanel" */ './ReevaluateInterview')
const Dashboard = () => import( /*  webpackChunkName: "controlpanel" */ './Dashboard')

export default class MapComponent {
    get routes() {
        return [
            {
                path: '/ControlPanel',
                component: ControlPanelLayout,
                children: [
                    {
                        path: 'Configuration',
                        component: () => import(/* webpackChunkName: "controlpanel" */'./Configuration'),
                    },
                    {
                        path: 'TabletInfos',
                        component: TabletInfos,
                    },
                    {
                        path: 'AppUpdates',
                        component: AppUpdates,
                    },
                    {
                        path: 'InterviewPackages',
                        component: InterviewPackages,
                    },
                    {
                        path: 'ResetPrivilegedUserPassword',
                        component: ChangePassword,
                    },
                    {
                        path: 'ReevaluateInterview',
                        component: ReevaluateInterview,
                    },
                    {
                        path: '',
                        component: Dashboard,
                    },
                ],
            },
        ]
    }
}
