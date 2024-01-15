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
import { useUserStore } from './stores/user';
//import ConfirmDialog from './views/App/components/Confirm.vue';

export default {
    name: 'QuestionnaireApp',
    components: {
        //ConfirmDialog
    },
    provide() {
        return {
            questionnaire: this.questionnaire,
            currentChapter: this.currentChapter
        };
    },
    setup() {
        const questionnaireStore = useQuestionnaireStore();
        const treeStore = useTreeStore();
        const userStore = useUserStore();

        return {
            questionnaireStore,
            treeStore,
            userStore
        };
    },
    async beforeMount() {
        await this.fetch();
    },
    computed: {
        questionnaire() {
            return this.questionnaireStore.getInfo || {};
        },
        currentChapter() {
            return this.treeStore.getChapter || {};
        }
    },
    methods: {
        async fetch() {
            await this.userStore.fetchUserInfo();
            await this.questionnaireStore.fetchQuestionnaireInfo(
                this.$route.params.questionnaireId
            );
        },

    }
};
</script>
