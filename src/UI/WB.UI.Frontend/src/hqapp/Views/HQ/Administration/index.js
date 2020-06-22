const AdministrationLayout = () => import( /*  webpackChunkName: "diagnostics" */ './AdministrationLayout')
const Diagnostics = () => import( /*  webpackChunkName: "diagnostics" */ './Diagnostics')

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
                ],
            },
        ]
    }
}
