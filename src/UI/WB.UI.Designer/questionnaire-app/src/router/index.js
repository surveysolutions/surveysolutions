import { createRouter, createWebHistory } from 'vue-router';
import PageNotFound from '../views/PageNotFound.vue';

const OptionsEditor = () => import('../views/OptionsEditor/OptionsEditor.vue');

const LeftSidePanel = () => import('../views/App/components/LeftSidePanel.vue');
const Panels = () => import('../views/App/components/Panels.vue');
const RightPanel = () => import('../views/App/components/RightPanel.vue');
const Tree = () => import('../views/App/components/Tree.vue');
const Variable = () => import('../views/App/components/Variable.vue');
const Question = () => import('../views/App/components/Question.vue');
const StaticText = () => import('../views/App/components/StaticText.vue');
const Chapter = () => import('../views/App/components/Chapter.vue');
const QuestionnaireHeader = () => import('../views/App/components/Header.vue');

const routes = [
    {
        path: '/questionnaire/editcategories/:questionnaireId',
        component: OptionsEditor,
        props: route => ({
            questionnaireRev: route.params.questionnaireId,
            id: route.query.categoriesid,
            isCategory: true,
            cascading: route.query.cascading == 'true'
        })
    },
    {
        path: '/questionnaire/editoptions/:questionnaireId',
        component: OptionsEditor,
        props: route => ({
            questionnaireRev: route.params.questionnaireId,
            id: route.query.questionid,
            isCategory: false,
            cascading: route.query.cascading == 'true'
        })
    },
    {
        name: 'questionnaire',
        path: '/q/details/:questionnaireId',
        components: {
            default: Panels,
            header: QuestionnaireHeader,
            leftSidePanel: LeftSidePanel
        },
        props: route => ({
            questionnaireId: route.params.questionnaireId
        }),
        children: [
            {
                name: 'chapter',
                path: 'chapter/:chapterId',
                props: route => ({
                    questionnaireId: route.params.questionnaireId,
                    chapterId: route.params.chapterId
                }),
                components: {
                    default: RightPanel,
                    tree: Tree
                },
                children: [
                    {
                        name: 'variable',
                        path: 'variable/:variableId',
                        component: Variable
                    },
                    {
                        name: 'question',
                        path: 'question/:questionId',
                        component: Question
                    },
                    {
                        name: 'statictext',
                        path: 'statictext/:statictextId',
                        component: StaticText,
                        props: route => ({
                            statictextId: route.params.statictextId
                        })
                    },
                    {
                        name: 'chapter',
                        path: '',
                        component: Chapter
                    }
                ]
            }
        ]
    },
    {
        path: '/*',
        component: PageNotFound
    }
];

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    //base: import.meta.env.BASE_URL,
    routes
});

export default router;
