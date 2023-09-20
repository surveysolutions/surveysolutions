import { createRouter, createWebHistory } from 'vue-router'
import PageNotFound from '../views/PageNotFound.vue';

import OptionsEditor from '../views/OptionsEditor/OptionsEditor.vue'
import Questionnaire from '../views/App/Main.vue'
import Tree from '../views/App/components/tree.vue'
import Variable from '../views/App/components/variable.vue'
import Question from '../views/App/components/question.vue'
import StaticText from '../views/App/components/statictext.vue'
import Chapter from '../views/App/components/chapter.vue'
import QuestionnaireHeader from '../views/App/components/header.vue'


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
    /*{
        path: '/q/details/:questionnaireId',
        component: Questionnaire,
        props: route => ({
            questionnaireRev: route.params.questionnaireId            
        })
    },
    {
        path: '/q/details/:questionnaireId/chapter/:chapterId/:entityType/:entityId',
        component: Questionnaire,
        props: route => ({
            questionnaireRev: route.params.questionnaireId,            
            chapterId: route.params.chapterId,            
            entityType: route.params.entityType,            
            entityId: route.params.entityId,            
        })
    },*/
    {
        name: 'questionnaire',
        path: '/q/details/:questionnaireId',
        components: {
            //default: Question,
            tree: Tree,
            header: QuestionnaireHeader,
        },
        /*props: route => ({
            questionnaireRev: route.params.questionnaireId,            
        }),*/
        children: [
            /*{
                name: 'variable',
                path: '/chapter/:chapterId/variable/:variableId',
                component: Variable,
            },
            {
                name: 'question',
                path: '/chapter/:chapterId/question/:questionId',
                component: Question,
            },
            {
                name: 'statictext',
                path: '/chapter/:chapterId/statictext/:statictextId',
                component: StaticText,
            },*/


            {
                name: 'chapter',
                path: 'chapter/:chapterId',
                component: Chapter,
                /*components: {
                    default: Chapter,
                    tree: Tree,
                    header: QuestionnaireHeader,
                },*/
                /*props: route => ({
                    chapterId: route.params.chapterId            
                }),*/
                children: [
                    {
                        //name: 'variable',
                        path: 'variable/:variableId',
                        //component: Variable,
                        components: {
                            default: Variable,
                            tree: Tree,
                            header: QuestionnaireHeader,
                        },
                        /*props: route => ({
                            variableId: route.params.variableId            
                        }),*/
                    },
                    {
                        //name: 'question',
                        path: 'question/:questionId',
                        component: Question,
                        /*props: route => ({
                            questionId: route.params.questionId            
                        }),*/
                    },
                    {
                        //name: 'statictext',
                        path: 'statictext/:statictextId',
                        component: StaticText,
                        /*props: route => ({
                            statictextId: route.params.statictextId            
                        }),*/
                    },
        
        
                    /*{
                        name: 'chapter',
                        path: '',
                        component: Chapter,
                    },*/
                    
                ],
            },
            
        ],
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
