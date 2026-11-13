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
        <v-progress-linear :active="progressStore.getIsRunning" indeterminate color="#29d" height="3"
            style="position:fixed;top:0;left:0;right:0;z-index:9999" />
    </div>
    <div v-if="globalNotice" class="designer-global-notice" role="status">
        <span class="designer-global-notice-text">{{ globalNotice }}</span>
        <button type="button" class="designer-global-notice-close" :aria-label="$t('QuestionnaireEditor.Close')"
            @click="globalNoticeDismissed = true">
            <span aria-hidden="true">&times;</span>
        </button>
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
                    <div class="ui-pnotify-closer" aria-role="button" tabindex="0"
                        :title="$t('QuestionnaireEditor.Close')" @click="props.close" style="cursor: pointer;">
                        <span class="glyphicon glyphicon-remove"></span>
                    </div>
                    <div class="ui-pnotify-sticker" aria-role="button" aria-pressed="false" tabindex="0" :title="$t('QuestionnaireEditor.Stick')"
                        style="cursor: pointer; visibility: hidden;">
                        <span class="glyphicon glyphicon-pause" aria-pressed="false"></span>
                    </div>
                    <div class="ui-pnotify-icon">
                        <span class="glyphicon glyphicon-exclamation-sign"></span>
                    </div>
                    <h4 class="ui-pnotify-title" style="display: none;">
                        {{ props.item.title }}
                    </h4>
                    <div class="ui-pnotify-text" role="alert" v-sanitize-html="props.item.text"></div>
                    <div class="ui-pnotify-action-bar"
                        style="margin-top: 5px; clear: both; text-align: right; display: none;"></div>
                </div>
            </div>
        </template>
    </notifications>
    <div v-if="isBlocked" block-ui-container="" class="block-ui-container ng-scope">
        <div class="block-ui-overlay"></div>
        <div class="block-ui-message-container" aria-live="assertive" aria-atomic="true">
            <div class="block-ui-message ng-binding">{{ $t('QuestionnaireEditor.PleaseWait') }}</div>
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

.designer-global-notice {
    position: fixed;
    top: 8px;
    left: 50%;
    transform: translateX(-50%);
    z-index: 1060;
    display: flex;
    align-items: flex-start;
    gap: 12px;
    min-width: 300px;
    max-width: 40%;
    max-height: 30vh;
    padding: 8px 12px 8px 16px;
    background-color: #fff8d6;
    border: 1px solid #fad540;
    border-left: 4px solid #f0ad00;
    border-radius: 4px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
    color: #333;
    font-size: 14px;
    font-weight: normal;
    line-height: 20px;
    letter-spacing: normal;
    text-align: left;
    text-transform: none;

    .designer-global-notice-text {
        flex: 1;
        overflow-y: auto;
        overflow-wrap: anywhere;
        white-space: pre-line;
    }

    .designer-global-notice-close {
        flex: none;
        padding: 0;
        border: 0;
        background: none;
        color: #666;
        font-size: 20px;
        line-height: 20px;
        cursor: pointer;

        &:hover {
            color: #000;
        }
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
import { useKeyShortcut } from '../composables/useKeyShortcut';

import '../../content/external/angular-block-ui.css';
import '../../content/external/hotkeys.css';
import '../../content/external/loading-bar.css';
import '../../content/external/pnotify.css';
import '../../content/external/pnotify.buttons.css';
import '../../content/external/angular-ui-tree.css';

//import '../../content/markup.less'
import '../../content/markup.less'

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
            showCheatSheet: false,
            globalNoticeDismissed: false
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

        const shift_question = useKeyShortcut(e =>
            e.key === '?' &&
            !['INPUT', 'TEXTAREA'].includes(document.activeElement?.tagName?.toUpperCase() ?? '')
        );

        const ctrl_p = useKeyShortcut(e => e.ctrlKey && !e.altKey && !e.shiftKey && e.code === 'KeyP');

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
        },
        globalNotice() {
            return this.globalNoticeDismissed ? null : this.currentUser.globalNotice;
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
                        force: true,
                        replace: true
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
