// //import Vue from 'vue'

import { createRouter, createWebHistory } from 'vue-router';
// import VueRouter from 'vue-router'
// import { assign } from 'lodash'

// //Vue.use(VueRouter)
// const UsersManagement = () => import('../Views/HQ/Users/UsersManagment/UsersManagement.vue')

// const Manage = () => import('../Views/HQ/Users/Manage.vue')

// const routes = [
//     {
//         path: '/users/UsersManagement',
//         component: UsersManagement,
//         // props: route => ({
//         //     //questionnaireRev: route.params.questionnaireId,
//         //     id: route.query.id,
//         //     //isCategory: true,
//         //     //cascading: route.query.cascading == 'true'
//         // })
//     },
//     {
//         path: '/UsersManagement',
//         component: UsersManagement,
//         // props: route => ({
//         //     //questionnaireRev: route.params.questionnaireId,
//         //     id: route.query.id,
//         //     //isCategory: true,
//         //     //cascading: route.query.cascading == 'true'
//         // })
//     },


//     // {
//     //     path: '/Manage/:userId', component: Manage, name: 'usersManage',
//     // },
//     // {
//     //     path: '/Manage/', component: Manage,
//     // },

//     {
//         path: '/users/Manage/:userId', component: Manage, name: 'usersManage',
//     },
//     {
//         path: '/users/Manage/', component: Manage,
//     },
// ]


export default class HqRouter {
    constructor(routes) {
        this.routes = routes
    }
    get router() {
        return createRouter({
            history: createWebHistory(window.CONFIG.basePath),
            routes: Object.entries(this.routes)[0][1]
            //TODO: MIGRATION
        })
    }
}

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
