// //import Vue from 'vue'

import { createRouter, createWebHistory } from 'vue-router';
// import VueRouter from 'vue-router'
// import { assign } from 'lodash'

// //Vue.use(VueRouter)

const routes = []


const router = createRouter({
    history: createWebHistory(),
    //base: import.meta.env.BASE_URL,
    routes
});

export default router;

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
