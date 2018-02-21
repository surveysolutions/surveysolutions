import Vue from "vue"

Vue.component("wb-comments",        () => import(/* webpackChunkName: "questions" */ "./Comments"))
Vue.component("wb-instructions",    () => import(/* webpackChunkName: "questions" */ "./Instructions"))
Vue.component("wb-title",           () => import(/* webpackChunkName: "questions" */ "./Title"))
Vue.component("wb-validation",      () => import(/* webpackChunkName: "questions" */ "./Validation"))
Vue.component("wb-warnings",        () => import(/* webpackChunkName: "questions" */ "./Warnings"))
Vue.component("wb-remove-answer",   () => import(/* webpackChunkName: "questions" */ "./RemoveAnswer"))
Vue.component("wb-progress",        () => import(/* webpackChunkName: "questions" */ "./Progress"))
Vue.component("wb-attachment",      () => import(/* webpackChunkName: "questions" */ "./Attachment"))
Vue.component("wb-lock",            () => import(/* webpackChunkName: "questions" */ "./Lock"))
Vue.component("wb-flag",            () => import(/* webpackChunkName: "questions" */ "./Flag"))
