import Vue from 'vue';
import VueRouter from 'vue-router';
import PageNotFound from '../views/PageNotFound.vue';

Vue.use(VueRouter);

const routes = [
    {
        path: '/questionnaire/editcategories/:questionnaireId',
        component: () =>
            import(
                /* webpackChunkName: "optionsEditor" */ '../views/OptionsEditor/OptionsEditor.vue'
            ),
        props: route => ({
            questionnaireRev: route.params.questionnaireId,
            id: route.query.categoriesid,
            isCategory: true,
            cascading: route.query.cascading
        })
    },
    {
        path: '/questionnaire/editoptions/:questionnaireId',
        component: () =>
            import(
                /* webpackChunkName: "optionsEditor" */ '../views/OptionsEditor/OptionsEditor.vue'
            ),
        props: route => ({
            questionnaireRev: route.params.questionnaireId,
            id: route.query.questionid,
            isCategory: false,
            cascading: route.query.cascading
        })
    },
    {
        path: '*',
        component: PageNotFound
    }
];

const router = new VueRouter({
    mode: 'history',
    base: process.env.BASE_URL,
    routes
});

export default router;
