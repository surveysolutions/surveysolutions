import { defineAsyncComponent } from 'vue';

export function registerPartsComponents(vue) {

    vue.component('wb-comments', defineAsyncComponent(() => import('./Comments')))
    vue.component('wb-comment-item', defineAsyncComponent(() => import('./CommentItem')))
    vue.component('wb-instructions', defineAsyncComponent(() => import('./Instructions')))
    vue.component('wb-title', defineAsyncComponent(() => import('./Title')))
    vue.component('wb-validation', defineAsyncComponent(() => import('./Validation')))
    vue.component('wb-warnings', defineAsyncComponent(() => import('./Warnings')))
    vue.component('wb-remove-answer', defineAsyncComponent(() => import('./RemoveAnswer')))
    vue.component('wb-progress', defineAsyncComponent(() => import('./Progress')))
    vue.component('wb-attachment', defineAsyncComponent(() => import('./Attachment')))
    vue.component('wb-lock', defineAsyncComponent(() => import('./Lock')))
    vue.component('wb-flag', defineAsyncComponent(() => import('./Flag')))
}