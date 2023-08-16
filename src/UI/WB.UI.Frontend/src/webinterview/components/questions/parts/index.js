import Vue from 'vue'

Vue.component('wb-comments',        () => import( './Comments'))
Vue.component('wb-comment-item',    () => import( './CommentItem'))
Vue.component('wb-instructions',    () => import( './Instructions'))
Vue.component('wb-title',           () => import( './Title'))
Vue.component('wb-validation',      () => import( './Validation'))
Vue.component('wb-warnings',        () => import( './Warnings'))
Vue.component('wb-remove-answer',   () => import( './RemoveAnswer'))
Vue.component('wb-progress',        () => import( './Progress'))
Vue.component('wb-attachment',      () => import( './Attachment'))
Vue.component('wb-lock',            () => import( './Lock'))
Vue.component('wb-flag',            () => import( './Flag'))
