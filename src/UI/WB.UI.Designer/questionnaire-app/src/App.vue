<template>
    <!--v-app>
        <v-main-->
    <div id="designer-editor" class="questionnaire container">
        <router-view name="header"></router-view>
        <section id="spacer" class="row">
            <div class="left"></div>
            <div class="right"></div>
        </section>
        <section id="main" class="row">
            <router-view name="leftSidePanel"></router-view>
            <router-view />
        </section>
    </div>
    <confirm-dialog></confirm-dialog>
    <!--/v-main>
    </v-app-->
</template>

<script>
import { useQuestionnaireStore } from './stores/questionnaire';
import { useTreeStore } from './stores/tree';

export default {
    name: 'QuestionnaireApp',
    provide() {
        return {
            questionnaire: this.questionnaire,
            currentChapter: this.currentChapter
        };
    },
    setup() {
        const questionnaireStore = useQuestionnaireStore();
        const treeStore = useTreeStore();

        return {
            questionnaireStore,
            treeStore
        };
    },
    computed: {
        questionnaire() {
            return this.questionnaireStore.info || {};
        },
        currentChapter() {
            return this.treeStore.getChapter || {};
        }
    }
};
</script>
