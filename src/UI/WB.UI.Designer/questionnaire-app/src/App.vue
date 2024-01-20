<template>
    <div id="designer-editor" class="questionnaire container vapp">
        <router-view name="header"></router-view>
        <section id="spacer" class="row">
            <div class="left"></div>
            <div class="right"></div>
        </section>
        <section id="main" class="row">
            <router-view name="leftSidePanel"></router-view>
            <router-view />
        </section>
        <vue-progress-bar></vue-progress-bar>
    </div>
    <div id="loading-logo" v-if="isActionRunning"></div>
    <confirm-dialog></confirm-dialog>
</template>

<script>
import { useProgressStore } from './stores/progress';
import { computed } from 'vue'

import { useQuestionnaireStore } from './stores/questionnaire';
import { useTreeStore } from './stores/tree';
import { useUserStore } from './stores/user';

export default {
    name: 'QuestionnaireApp',
    components: {},
    provide() {
        return {
            questionnaire: computed(() => this.questionnaire),
            currentChapter: computed(() => this.currentChapter),
            isCover: computed(() => this.isCover)
        };
    },
    setup() {
        const questionnaireStore = useQuestionnaireStore();
        const treeStore = useTreeStore();
        const userStore = useUserStore();
        const progressStore = useProgressStore();

        return {
            questionnaireStore,
            treeStore,
            userStore,
            progressStore
        };
    },
    async beforeMount() {
        await this.fetch();
    },
    mounted() {
        this.$Progress.finish();
    },
    computed: {
        questionnaire() {
            return this.questionnaireStore.getInfo || {};
        },
        currentChapter() {
            return this.treeStore.getChapter || {};
        },
        isActionRunning() {
            var progress = this.$Progress;
            if (progress) {
                this.$nextTick(() => {
                    if (this.progressStore.getIsRunning)
                        progress.start();
                    else
                        progress.finish();
                });
            }

            return this.progressStore.getIsRunning || false;
        },
        isCover() {
            return this.treeStore.getChapter.isCover;
        },
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
