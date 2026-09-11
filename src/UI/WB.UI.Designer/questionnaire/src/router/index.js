import { createRouter, createWebHistory } from 'vue-router';
import PageNotFound from '../views/PageNotFound.vue';
import { useRosterStore } from '../stores/roster';
import { useGroupStore } from '../stores/group';
import { useQuestionStore } from '../stores/question';
import { useStaticTextStore } from '../stores/staticText';
import { useVariableStore } from '../stores/variable';
import { i18n } from '../plugins/localization';

const DYNAMIC_IMPORT_RETRY_KEY = 'dynamic-import-retry-count';
const MAX_DYNAMIC_IMPORT_RETRIES = 2;
const dynamicImportErrorPatterns = [
    'Failed to fetch dynamically imported module',
    'Unable to preload CSS for'
];
let recoveryScheduled = false;

function isDynamicImportError(error) {
    const message = error instanceof Error ? error.message : String(error);
    return dynamicImportErrorPatterns.some(pattern => message.includes(pattern));
}

function delayDynamicImportRetry(retryCount) {
    const delay = 300 * 2 ** retryCount + Math.floor(Math.random() * 200);
    return new Promise(resolve => window.setTimeout(resolve, delay));
}

async function loadRouteComponent(loader) {
    for (let retryCount = 0; retryCount <= MAX_DYNAMIC_IMPORT_RETRIES; retryCount++) {
        try {
            return await loader();
        } catch (error) {
            if (!isDynamicImportError(error) || retryCount === MAX_DYNAMIC_IMPORT_RETRIES) {
                throw error;
            }

            await delayDynamicImportRetry(retryCount);
        }
    }
}

function getRetryCount() {
    try {
        const value = Number(window.sessionStorage.getItem(DYNAMIC_IMPORT_RETRY_KEY) ?? 0);
        return Number.isFinite(value) ? value : 0;
    } catch {
        return null;
    }
}

function setRetryCount(count) {
    try {
        window.sessionStorage.setItem(DYNAMIC_IMPORT_RETRY_KEY, String(count));
        return true;
    } catch {
        return false;
    }
}

function clearRetryCount() {
    try {
        window.sessionStorage.removeItem(DYNAMIC_IMPORT_RETRY_KEY);
    } catch {
        return;
    }
}

function hasUnsavedChanges(routeName) {
    switch (routeName) {
        case 'roster':
            return useRosterStore().getIsDirty;
        case 'group':
            return useGroupStore().getIsDirty;
        case 'question':
            return useQuestionStore().getIsDirty;
        case 'statictext':
            return useStaticTextStore().getIsDirty;
        case 'variable':
            return useVariableStore().getIsDirty;
        default:
            return false;
    }
}

function retryDynamicImport(error) {
    if (recoveryScheduled) return;

    const retryCount = getRetryCount();

    if (retryCount === null) {
        console.error('Dynamic import recovery skipped: session storage is unavailable.', error);
        return;
    }

    if (retryCount >= MAX_DYNAMIC_IMPORT_RETRIES) {
        console.error('Dynamic import retry budget exhausted:', error);
        return;
    }

    recoveryScheduled = true;

    if (hasUnsavedChanges(router.currentRoute.value.name)) {
        const confirmed = window.confirm(
            i18n.t('QuestionnaireEditor.UnsavedChangesLeave')
        );
        if (!confirmed) {
            recoveryScheduled = false;
            return;
        }
    }

    if (!setRetryCount(retryCount + 1)) {
        recoveryScheduled = false;
        console.error('Dynamic import recovery skipped: session storage is unavailable.', error);
        return;
    }

    delayDynamicImportRetry(retryCount).then(() => window.location.reload());
}

const OptionsEditor = () =>
    loadRouteComponent(() => import('../views/OptionsEditor/OptionsEditor.vue'));

const Questionnaire = () => loadRouteComponent(() => import('../views/Questionnaire.vue'));
const LeftSidePanel = () => loadRouteComponent(() => import('../views/App/components/LeftSidePanel.vue'));
const Panels = () => loadRouteComponent(() => import('../views/App/components/Panels.vue'));
const RightPanel = () => loadRouteComponent(() => import('../views/App/components/RightPanel.vue'));
const Tree = () => loadRouteComponent(() => import('../views/App/components/Tree.vue'));
const Variable = () => loadRouteComponent(() => import('../views/App/components/Variable.vue'));
const Question = () => loadRouteComponent(() => import('../views/App/components/Question.vue'));
const StaticText = () => loadRouteComponent(() => import('../views/App/components/StaticText.vue'));
const Group = () => loadRouteComponent(() => import('../views/App/components/Group.vue'));
const Roster = () => loadRouteComponent(() => import('../views/App/components/Roster.vue'));
const QuestionnaireHeader = () => loadRouteComponent(() => import('../views/App/components/Header.vue'));
const Comments = () => loadRouteComponent(() => import('../views/App/components/Comments.vue'));

const DesignerLayout = () => loadRouteComponent(() => import('../views/Designer/Layout.vue'));
const Classifications = () =>
    loadRouteComponent(() => import('../views/Designer/pages/Classifications.vue'));

import { useUnsavedChanges } from '../stores/unsavedChanges';

const routes = [
    {
        path: '/classifications',
        component: DesignerLayout,
        props: route => ({
            activePage: 'classifications'
        }),
        children: [
            {
                path: '',
                component: Classifications
            }
        ]
    },
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

router.onError(error => {
    if (isDynamicImportError(error)) {
        retryDynamicImport(error);
        return;
    }

    console.error('Router error:', error);
});

router.afterEach((to, from, failure) => {
    if (!failure) {
        clearRetryCount();
        recoveryScheduled = false;
    }
});

export default router;
