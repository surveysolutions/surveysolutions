const AdministrationLayout = () => import( /*  webpackChunkName: "diagnostics" */ './AdministrationLayout')
const Diagnostics = () => import( /*  webpackChunkName: "diagnostics" */ './Diagnostics')
const TabletInfos = () => import( /*  webpackChunkName: "controlpanel" */ './TabletInfos')

export default class MapComponent {
    get routes() {
        return [
            {
                path: '/Administration',
                component: AdministrationLayout,
                children: [
                    {
                        path: 'Diagnostics',
                        component: Diagnostics,
                    },
                    {
                        path: 'TabletInfos',
                        component: TabletInfos,
                    },
                ],
            },
        ]
    }
}
