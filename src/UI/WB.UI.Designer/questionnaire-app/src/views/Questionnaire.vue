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
            <div class="ui-pnotify ui-pnotify-fade-normal ui-pnotify-mobile-able ui-pnotify-in ui-pnotify-fade-in ui-pnotify-move"
                aria-live="assertive" aria-role="alertdialog" style="position: inherit;">
                <div class="alert ui-pnotify-container ui-pnotify-shadow" :class="{
        'alert-warning': props.item.type != 'error',
        'alert-danger': props.item.type == 'error'
    }" role="alert" style="min-height: 16px;">
                    <div class="ui-pnotify-closer" aria-role="button" tabindex="0" title="Close" @click="props.close"
                        style="cursor: pointer;">
                        <span class="glyphicon glyphicon-remove"></span>
                    </div>
                    <div class="ui-pnotify-sticker" aria-role="button" aria-pressed="false" tabindex="0" title="Stick"
                        style="cursor: pointer; visibility: hidden;">
                        <span class="glyphicon glyphicon-pause" aria-pressed="false"></span>
                    </div>
                    <div class="ui-pnotify-icon">
                        <span class="glyphicon glyphicon-exclamation-sign"></span>
                    </div>
                    <h4 class="ui-pnotify-title" style="display: none;">
                        {{ props.item.title }}
                    </h4>
                    <div class="ui-pnotify-text" aria-role="alert" v-dompurify-html="props.item.text"></div>
                    <div class="ui-pnotify-action-bar"
                        style="margin-top: 5px; clear: both; text-align: right; display: none;"></div>
                </div>
            </div>
        </template>
    </notifications>
    <div v-if="isBlocked" block-ui-container="" class="block-ui-container ng-scope">
        <div class="block-ui-overlay"></div>
        <div class="block-ui-message-container" aria-live="assertive" aria-atomic="true">
            <div class="block-ui-message ng-binding">Please wait...</div>
        </div>
    </div>
    <div class="cfp-hotkeys-container fade" :class="{ 'in': cheatSheetVisible }" v-if="cheatSheetVisible"
        style="overflow-y: auto;display: flex !important; justify-content: center;">
        <div class="cfp-hotkeys" style="height: 100vh; width: 100vw; padding: 20px;">
            <h4 class="cfp-hotkeys-title">{{ $t('QuestionnaireEditor.HotkeysShortcuts') }}</h4>
            <table role="presentation">
                <tbody>
                    <tr v-for="hotkey in hotkeys">
                        <td class="cfp-hotkeys-keys">
                            <span v-for="key in hotkey.keys" class="cfp-hotkeys-key">
                                {{ key }}
                            </span>
                        </td>
                        <td class="cfp-hotkeys-text">{{ hotkey.description }}</td>
                    </tr>
                </tbody>
            </table>
            <div class="cfp-hotkeys-close" @click="changeCheatSheetVisibility">&#215;</div>
        </div>
    </div>
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
import { useBlockUIStore } from '../stores/blockUI';
import { computed, readonly } from 'vue';

import { useQuestionnaireStore } from '../stores/questionnaire';
import { useTreeStore } from '../stores/tree';
import { useUserStore } from '../stores/user';
import { useHotkeysStore } from '../stores/hotkeys';
import { useUnsavedChanges } from '../stores/unsavedChanges';
import { useMagicKeys } from '@vueuse/core';

//import '../../content/markup.less'
import '../../../questionnaire/content/markup.less'

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
    data() {
        return {
            showCheatSheet: false
        };
    },
    setup() {
        const questionnaireStore = useQuestionnaireStore();
        const treeStore = useTreeStore();
        const userStore = useUserStore();
        const progressStore = useProgressStore();
        const blockUIStore = useBlockUIStore();
        const hotkeysStore = useHotkeysStore();
        const unsavedChanges = useUnsavedChanges();

        const { shift_question } = useMagicKeys({
            aliasMap: {
                question: '?',
            },
            passive: false,
            onEventFired(e) {
                if (e.shiftKey && e.key === '?' && e.type === 'keydown') {
                    //workaround for the issue with useMagicKeys useActiveElement      
                    if (!(document.activeElement?.tagName.toUpperCase() == 'INPUT'
                        || document.activeElement?.tagName.toUpperCase() == 'TEXTAREA')) {
                        e.preventDefault();
                    }
                }
            },
        })

        const { ctrl_p } = useMagicKeys({
            passive: false,
            onEventFired(e) {
                if (e.ctrlKey && e.key === 'p' && e.type === 'keydown')
                    e.preventDefault()
            },
        })

        return {
            questionnaireStore,
            treeStore,
            userStore,
            progressStore,
            blockUIStore,
            hotkeysStore,
            unsavedChanges,
            ctrl_p,
            shift_question
        };
    },
    async beforeMount() {
        await this.fetch();
    },
    watch: {
        isBlocked: function (val) {
            var body = document.body;
            if (val) {
                body.classList.add('block-ui-anim-fade', 'block-ui-active', 'block-ui-visible');
            } else {
                body.classList.remove('block-ui-anim-fade', 'block-ui-active', 'block-ui-visible');
            }
        },
        shift_question: function (val) {
            if (!(document.activeElement?.tagName.toUpperCase() == 'INPUT'
                || document.activeElement?.tagName.toUpperCase() == 'TEXTAREA')) {
                if (val)
                    this.changeCheatSheetVisibility();
            }

        },
        ctrl_p: function (val) {
            if (val)
                window.open("/pdf/printpreview/" + this.questionnaire.questionnaireId, "_blank");
        }
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
        hotkeys() {
            return this.hotkeysStore.getHotkeys;
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
        isBlocked() {
            return this.blockUIStore.isBlocked;
        },
        cheatSheetVisible() {
            return this.showCheatSheet;
        }
    },
    methods: {
        async fetch() {
            await this.userStore.fetchUserInfo();
            await this.questionnaireStore.fetchQuestionnaireInfo(
                this.$route.params.questionnaireId
            );

            if (!this.$route.params.chapterId) {
                var info = this.questionnaireStore.getInfo;
                if (info?.chapters && info.chapters.length > 0) {
                    var chapter = info.chapters[0];
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
        },
        changeCheatSheetVisibility() {
            this.showCheatSheet = !this.showCheatSheet;
        }
    }
};
</script>
