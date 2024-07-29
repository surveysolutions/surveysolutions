export function registerPartsComponents(vue) {

    vue.component('wb-comments', () => import('./Comments'))
    vue.component('wb-comment-item', () => import('./CommentItem'))
    vue.component('wb-instructions', () => import('./Instructions'))
    vue.component('wb-title', () => import('./Title'))
    vue.component('wb-validation', () => import('./Validation'))
    vue.component('wb-warnings', () => import('./Warnings'))
    vue.component('wb-remove-answer', () => import('./RemoveAnswer'))
    vue.component('wb-progress', () => import('./Progress'))
    vue.component('wb-attachment', () => import('./Attachment'))
    vue.component('wb-lock', () => import('./Lock'))
    vue.component('wb-flag', () => import('./Flag'))
}