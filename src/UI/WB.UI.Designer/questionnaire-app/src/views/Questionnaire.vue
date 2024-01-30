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
    <notifications position="top right">
        <template #body="props">
            <div
                class="ui-pnotify ui-pnotify-fade-normal ui-pnotify-mobile-able ui-pnotify-in ui-pnotify-fade-in ui-pnotify-move"
                aria-live="assertive"
                aria-role="alertdialog"
                style="position: inherit;"
            >
                <div
                    class="alert ui-pnotify-container ui-pnotify-shadow"
                    :class="{
                        'alert-warning': props.item.type != 'error',
                        'alert-danger': props.item.type == 'error'
                    }"
                    role="alert"
                    style="min-height: 16px;"
                >
                    <div
                        class="ui-pnotify-closer"
                        aria-role="button"
                        tabindex="0"
                        title="Close"
                        @click="props.close"
                        style="cursor: pointer;"
                    >
                        <span class="glyphicon glyphicon-remove"></span>
                    </div>
                    <div
                        class="ui-pnotify-sticker"
                        aria-role="button"
                        aria-pressed="false"
                        tabindex="0"
                        title="Stick"
                        style="cursor: pointer; visibility: hidden;"
                    >
                        <span
                            class="glyphicon glyphicon-pause"
                            aria-pressed="false"
                        ></span>
                    </div>
                    <div class="ui-pnotify-icon">
                        <span
                            class="glyphicon glyphicon-exclamation-sign"
                        ></span>
                    </div>
                    <h4 class="ui-pnotify-title" style="display: none;">
                        {{ props.item.title }}
                    </h4>
                    <div
                        class="ui-pnotify-text"
                        aria-role="alert"
                        v-dompurify-html="props.item.text"
                    ></div>
                    <div
                        class="ui-pnotify-action-bar"
                        style="margin-top: 5px; clear: both; text-align: right; display: none;"
                    ></div>
                </div>
            </div>
        </template>
    </notifications>
</template>

<style lang="scss">
.vue-notification-group {
    position: absolute;

    .vue-notification-wrapper {
        overflow: visible;
        padding-top: 36px;
        padding-right: 36px;
    }
}
</style>

<script>
import { useProgressStore } from '../stores/progress';
import { computed, readonly } from 'vue';

import { useQuestionnaireStore } from '../stores/questionnaire';
import { useTreeStore } from '../stores/tree';
import { useUserStore } from '../stores/user';

export default {
    name: 'QuestionnaireApp',
    components: {},
    provide() {
        return {
            questionnaire: computed(() => this.questionnaire),
            currentChapter: computed(() => this.currentChapter),
            isCover: readonly(computed(() => this.isCover)),
            isReadOnlyForUser: readonly(computed(() => this.isReadOnlyForUser)),

            currentUser: readonly(computed(() => this.currentUser))
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
        currentUser() {
            return this.userStore.getInfo || {};
        },
        isActionRunning() {
            var progress = this.$Progress;
            if (progress) {
                this.$nextTick(() => {
                    if (this.progressStore.getIsRunning) progress.start();
                    else progress.finish();
                });
            }

            return this.progressStore.getIsRunning || false;
        },
        isCover() {
            return this.treeStore.getChapter.isCover;
        },
        isReadOnlyForUser() {
            return this.questionnaire.isReadOnlyForUser;
        },
    },
    methods: {
        async fetch() {
            await this.userStore.fetchUserInfo();
            await this.questionnaireStore.fetchQuestionnaireInfo(
                this.$route.params.questionnaireId
            );

            if (!this.$route.params.chapterId) {
                var chapter = this.questionnaireStore.getInfo.chapters[0];
                this.$router.push({
                    name: 'group',
                    params: {
                        chapterId: chapter.itemId,
                        entityId: chapter.itemId
                    },
                    force: true
                });
            }
        }
    }
};
</script>
