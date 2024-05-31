import { createRouter, createWebHistory } from 'vue-router';
import PageNotFound from '../views/PageNotFound.vue';

const OptionsEditor = () => import('../views/OptionsEditor/OptionsEditor.vue');

const Questionnaire = () => import('../views/Questionnaire.vue');
const LeftSidePanel = () => import('../views/App/components/LeftSidePanel.vue');
const Panels = () => import('../views/App/components/Panels.vue');
const RightPanel = () => import('../views/App/components/RightPanel.vue');
const Tree = () => import('../views/App/components/Tree.vue');
const Variable = () => import('../views/App/components/Variable.vue');
const Question = () => import('../views/App/components/Question.vue');
const StaticText = () => import('../views/App/components/StaticText.vue');
const Chapter = () => import('../views/App/components/Chapter.vue');
const Group = () => import('../views/App/components/Group.vue');
const Roster = () => import('../views/App/components/Roster.vue');
const QuestionnaireHeader = () => import('../views/App/components/Header.vue');
const Comments = () => import('../views/App/components/Comments.vue');

import { useUnsavedChanges } from '../stores/unsavedChanges';

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
        name: 'q',
        path: '/q',
        component: Questionnaire,
        children: [
            {
                name: 'questionnaire',
                path: 'details/:questionnaireId',
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
                                path: 'variable/:entityId',
                                components: {
                                    default: Variable,
                                    comments: Comments
                                },
                                props: route => ({
                                    questionnaireId:
                                        route.params.questionnaireId,
                                    variableId: route.params.entityId,
                                    entityId: route.params.entityId
                                })
                            },
                            {
                                name: 'question',
                                path: 'question/:entityId',
                                components: {
                                    default: Question,
                                    comments: Comments
                                },
                                props: route => ({
                                    questionnaireId:
                                        route.params.questionnaireId,
                                    questionId: route.params.entityId,
                                    entityId: route.params.entityId
                                })
                            },
                            {
                                name: 'statictext',
                                path: 'statictext/:entityId',
                                components: {
                                    default: StaticText,
                                    comments: Comments
                                },
                                props: route => ({
                                    questionnaireId:
                                        route.params.questionnaireId,
                                    statictextId: route.params.entityId,
                                    entityId: route.params.entityId
                                })
                            },
                            {
                                name: 'group',
                                path: 'group/:entityId',
                                components: {
                                    default: Group,
                                    comments: Comments
                                },
                                props: route => ({
                                    questionnaireId:
                                        route.params.questionnaireId,
                                    groupId: route.params.entityId,
                                    entityId: route.params.entityId
                                })
                            },
                            {
                                name: 'roster',
                                path: 'roster/:entityId',
                                components: {
                                    default: Roster,
                                    comments: Comments
                                },
                                props: route => ({
                                    rosterId: route.params.entityId,
                                    questionnaireId:
                                        route.params.questionnaireId,
                                    entityId: route.params.entityId
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
            }
        ]
    },
    {
        path: '/*',
        component: PageNotFound
    }
];

const router = createRouter({
    history: createWebHistory(),
    //base: import.meta.env.BASE_URL,
    routes
});

router.beforeEach((to, from, next) => {
    const { getUnsavedChanges, confirmLeave } = useUnsavedChanges();

    if (getUnsavedChanges(from.name) && !confirmLeave()) {
        next(false);
    } else {
        next();
    }
});

export default router;
