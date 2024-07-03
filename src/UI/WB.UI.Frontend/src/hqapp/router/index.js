// //import Vue from 'vue'

import { createRouter, createWebHistory } from 'vue-router';
// import VueRouter from 'vue-router'
// import { assign } from 'lodash'



// //Vue.use(VueRouter)
//import UsersManagement from '../Views/HQ/Users/UsersManagment/UsersManagement.vue'

const UsersManagement = () => import('../Views/HQ/Users/UsersManagment/UsersManagement.vue')

const routes = [
    {
        path: '/users/UsersManagement',
        component: UsersManagement,
        // props: route => ({
        //     //questionnaireRev: route.params.questionnaireId,
        //     id: route.query.id,
        //     //isCategory: true,
        //     //cascading: route.query.cascading == 'true'
        // })
    },
    {
        path: '/UsersManagement',
        component: UsersManagement,
        // props: route => ({
        //     //questionnaireRev: route.params.questionnaireId,
        //     id: route.query.id,
        //     //isCategory: true,
        //     //cascading: route.query.cascading == 'true'
        // })
    },
    {
        path: '/*',
        component: UsersManagement,
        // props: route => ({
        //     //questionnaireRev: route.params.questionnaireId,
        //     id: route.query.id,
        //     //isCategory: true,
        //     //cascading: route.query.cascading == 'true'
        // })
    },
]

const router = createRouter({
    history: createWebHistory(),
    //base: import.meta.env.BASE_URL,
    routes
});

export default router;

// export default class HqRouter {
//     constructor(routes) {
//         this.routes = routes
//     }
//     get router() {
//         return createRouter({
//             history: createWebHistory(),
//             routes//: Object.entries(this.routes)[0][1]
//             //TODO: MIGRATION
//         })
//     }
// }

// export default class HqRouter {
//     constructor(options) {
//         this.options = assign({
//             base: window.CONFIG.basePath,
//             mode: 'history',
//             scrollBehavior() {
//                 return { x: 0, y: 0 }
//             },
//         }, options)
//     }

//     get router() {
//         return new VueRouter(this.options)
//     }
// }
