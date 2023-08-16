const ControlPanelLayout = () => import('./ControlPanelLayout')
const AppUpdates = () => import('./AppUpdates')
const ReevaluateInterview = () => import('./ReevaluateInterview')

export default class MapComponent {
    get routes() {
        return [
            {
                path: '/ControlPanel',
                component: ControlPanelLayout,
                children: [
                    {
                        path: 'Configuration',
                        component: () => import('./Configuration'),
                    },
                    {
                        path: 'AppUpdates',
                        component: AppUpdates,
                    },
                    {
                        path: 'ReevaluateInterview',
                        component: ReevaluateInterview,
                    },
                    {
                        path: '',
                        component: AppUpdates,
                    },
                ],
            },
        ]
    }
}
