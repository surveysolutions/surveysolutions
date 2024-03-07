<template>
    <form id="staticText-editor" name="staticTextForm" unsaved-warning-form v-show="activeStaticText">
        <div id="show-reload-details-promt" class="ng-cloak" v-show="shouldUserSeeReloadDetailsPromt">
            <div class="inner">
                {{ $t('QuestionnaireEditor.QuestionToUpdateOptions') }}
                <a @click="fetch()" href="javascript:void(0);">{{ $t('QuestionnaireEditor.QuestionClickReload') }}</a>
            </div>
        </div>
        <div class="form-holder">

            <Breadcrumbs :breadcrumbs="activeStaticText.breadcrumbs">
            </Breadcrumbs>
            <div class="form-group">
                <label class="wb-label">
                    {{ $t('QuestionnaireEditor.StaticText') }}</label><br />
                <ExpressionEditor id="edit-static-text-highlight" mode="substitutions" v-model="activeStaticText.text" />
            </div>
            <div class="form-group">
                <label for="edit-static-attachment-name" class="wb-label">
                    {{
                        $t('QuestionnaireEditor.StaticTextAttachmentName')
                    }}&nbsp;
                    <help link="attachmentName" />
                </label><br />
                <input id="edit-static-attachment-name" type="text" class="form-control"
                    v-model="activeStaticText.attachmentName" spellcheck="false" maxlength="32" />
            </div>
            <div class="form-group" v-if="doesStaticTextSupportEnablementConditions() &&
                !(
                    (showEnablingConditions === undefined &&
                        activeStaticText.enablementCondition) ||
                    showEnablingConditions
                )
                ">
                <button type="button" class="btn btn-lg btn-link" @click="showEnablingConditions = true">
                    {{ $t('QuestionnaireEditor.AddEnablingCondition') }}
                </button>
            </div>

            <div class="row" v-if="doesStaticTextSupportEnablementConditions() &&
                ((showEnablingConditions === undefined &&
                    activeStaticText.enablementCondition) ||
                    showEnablingConditions)
                ">
                <div class="form-group col-xs-11">
                    <div class="enabling-group-marker" :class="{
                        'hide-if-disabled': activeStaticText.hideIfDisabled
                    }"></div>
                    <label>{{ $t('QuestionnaireEditor.EnablingCondition') }}
                        <help link="conditionExpression" />
                    </label>

                    <input type="checkbox" class="wb-checkbox" disabled="disabled" checked="checked"
                        v-if="questionnaire.hideIfDisabled" :title="$t('QuestionnaireEditor.HideIfDisabledNested')" />

                    <input v-if="!questionnaire.hideIfDisabled" id="cb-hideIfDisabled" type="checkbox" class="wb-checkbox"
                        v-model="activeStaticText.hideIfDisabled" />

                    <label for="cb-hideIfDisabled"><span :title="questionnaire.hideIfDisabled
                        ? $t(
                            'QuestionnaireEditor.HideIfDisabledNested'
                        )
                        : ''
                        "></span>
                        {{ $t('QuestionnaireEditor.HideIfDisabled') }}
                        <help link="hideIfDisabled" />
                    </label>
                    <ExpressionEditor id="edit-question-enablement-condition" v-model="activeStaticText.enablementCondition"
                        mode="expression" />

                </div>
                <div class="form-group col-xs-1">
                    <button type="button" class="btn cross instructions-cross"
                        @click="showEnablingConditions = false; activeStaticText.enablementCondition = ''; activeStaticText.hideIfDisabled = false;"></button>
                </div>
            </div>

            <div class="form-group validation-group" v-for="(validation, index) in activeStaticText.validationConditions"
                :id="'validationCondition' + index">
                <div class="validation-group-marker"></div>
                <label>{{ $t('QuestionnaireEditor.ValidationCondition') }}
                    {{ index + 1 }}
                    <help link="validationExpression" />
                </label>

                <input :id="'cb-isWarning' + index" type="checkbox" class="wb-checkbox" v-model="validation.severity"
                    :true-value="'Warning'" :false-value="'Error'" />
                <label :for="'cb-isWarning' + index"><span></span>{{ $t('QuestionnaireEditor.IsWarning') }}</label>

                <button class="btn delete-btn-sm delete-validation-condition" @click="removeValidationCondition(index)"
                    tabindex="-1"></button>
                <ExpressionEditor :id="'validation-expression-' + index" v-model="validation.expression"
                    mode="expression" />
                <label class="validation-message">{{
                    $t('QuestionnaireEditor.ErrorMessage') }}
                    <help link="validationMessage" />
                </label>
                <ExpressionEditor :id="'validation-message-' + index" v-model="validation.message" mode="substitutions" />
            </div>
            <div class="form-group"
                v-if="activeStaticText.validationConditions && activeStaticText.validationConditions.length < 10">
                <button type="button" class="btn btn-lg btn-link" @click="addValidationCondition()">
                    {{ $t('QuestionnaireEditor.AddValidationRule') }}
                </button>
            </div>
        </div>
        <div class="form-buttons-holder">
            <div class="pull-left">
                <button type="button" id="edit-static-text-save-button" v-if="!questionnaire.isReadOnlyForUser"
                    :class="{ 'btn-primary': isDirty }" :disabled="!isDirty" class="btn btn-lg" unsaved-warning-clear
                    @click="saveStaticText()">
                    {{ $t('QuestionnaireEditor.Save') }}
                </button>
                <button type="button" id="edit-static-text-cancel-button" class="btn btn-lg btn-link" unsaved-warning-clear
                    @click="cancel()">
                    {{ $t('QuestionnaireEditor.Cancel') }}
                </button>
            </div>
            <div class="pull-right">
                <button type="button" v-if="!questionnaire.isReadOnlyForUser" id="add-comment-button"
                    class="btn btn-lg btn-link" @click="toggleComments(activeStaticText)" unsaved-warning-clear>
                    <span v-if="!isCommentsBlockVisible && commentsCount == 0">{{
                        $t('QuestionnaireEditor.EditorAddComment') }}
                    </span>
                    <span v-if="!isCommentsBlockVisible && commentsCount > 0">
                        {{
                            $t('QuestionnaireEditor.EditorShowComments', {
                                count: commentsCount
                            })
                        }}
                    </span>
                    <span v-if="isCommentsBlockVisible">{{ $t('QuestionnaireEditor.EditorHideComment') }}
                    </span>
                </button>
                <button type="button" id="edit-static-text-delete-button" v-if="!questionnaire.isReadOnlyForUser"
                    class="btn btn-lg btn-link" @click="deleteStaticText()" unsaved-warning-clear>
                    {{ $t('QuestionnaireEditor.Delete') }}
                </button>
                <MoveToChapterSnippet :item-id="statictextId" :item-type="'StaticText'"
                    v-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly" />
            </div>
        </div>
    </form>
</template>

<script>
import { useCommentsStore } from '../../../stores/comments';

import { useStaticTextStore } from '../../../stores/staticText';
import { deleteStaticText, updateStaticText } from '../../../services/staticTextService';

import { createQuestionForDeleteConfirmationPopup } from '../../../services/utilityService'
import { setFocusIn } from '../../../services/utilityService'

import Breadcrumbs from './Breadcrumbs.vue';
import ExpressionEditor from './ExpressionEditor.vue';
import Help from './Help.vue';
import MoveToChapterSnippet from './MoveToChapterSnippet.vue';
import { useMagicKeys } from '@vueuse/core';

export default {
    name: 'StaticText',
    components: {
        Breadcrumbs,
        ExpressionEditor,
        Help,
        MoveToChapterSnippet
    },
    inject: ['questionnaire', 'currentChapter'],
    props: {
        questionnaireId: { type: String, required: true },
        statictextId: { type: String, required: true }
    },
    data() {
        return {
            shouldUserSeeReloadDetailsPromt: null,
            showEnablingConditions: null,
        };
    },
    watch: {
        async statictextId(newValue, oldValue) {
            if (newValue != oldValue) {
                this.staticTextStore.clear();
                await this.fetch();
                this.scrollTo();
            }
        },
        ctrl_s: function (v) {
            if (v) {
                this.saveStaticText();
            }
        },
        $route: function (oldValue, newValue) {
            this.scrollTo();
        }
    },
    setup() {
        const staticTextStore = useStaticTextStore();
        const commentsStore = useCommentsStore();

        commentsStore.registerEntityInfoProvider(function () {
            const initial = staticTextStore.getInitialStaticText;

            return {
                title: initial.text,
                variable: '',
                type: 'statictext'
            };
        });

        const { ctrl_s } = useMagicKeys({
            passive: false,
            onEventFired(e) {
                if (e.ctrlKey && e.key === 's' && e.type === 'keydown')
                    e.preventDefault()
            },
        });

        return {
            staticTextStore,
            commentsStore,
            ctrl_s
        };
    },
    async beforeMount() {
        await this.fetch();
    },
    mounted() {
        this.scrollTo();
    },
    computed: {
        commentsCount() {
            return this.commentsStore.getCommentsCount;
        },
        isCommentsBlockVisible() {
            return this.commentsStore.getIsCommentsBlockVisible;
        },
        activeStaticText() {
            return this.staticTextStore.getStaticText;
        },
        isDirty() {
            return this.staticTextStore.getIsDirty;
        }
    },
    methods: {
        async fetch() {
            await this.staticTextStore.fetchStaticTextData(this.questionnaireId, this.statictextId);
            this.shouldUserSeeReloadDetailsPromt = false;
            this.showEnablingConditions = this.activeStaticText.enablementCondition ? true : false;
        },
        saveStaticText() {
            if (this.questionnaire.isReadOnlyForUser) return;
            if (!this.isDirty) return;
            updateStaticText(this.questionnaireId, this.activeStaticText);
        },

        cancel() {
            this.staticTextStore.discardChanges();
            this.showEnablingConditions = this.activeStaticText.enablementCondition ? true : false;
        },

        deleteStaticText() {
            var itemIdToDelete = this.statictextId;
            var questionnaireId = this.questionnaireId;

            const params = createQuestionForDeleteConfirmationPopup(
                this.activeStaticText.text ||
                this.$t('QuestionnaireEditor.UntitledStaticText')
            );

            params.callback = confirm => {
                if (confirm) {
                    deleteStaticText(questionnaireId, itemIdToDelete).then(() => {
                        const chapterId = this.currentChapter.chapter.itemId;
                        this.$router.push({
                            name: 'group',
                            params: {
                                chapterId: chapterId,
                                entityId: chapterId
                            },
                            force: true
                        });
                    });
                }
            };

            this.$confirm(params);
        },
        doesStaticTextSupportEnablementConditions() {
            return this.activeStaticText && !this.currentChapter.isCover;
        },
        removeValidationCondition(index) {
            this.activeStaticText.validationConditions.splice(index, 1);
        },
        addValidationCondition() {
            this.activeStaticText.validationConditions.push({
                expression: '',
                message: ''
            });
        },
        toggleComments() {
            this.commentsStore.toggleComments();
        },

        scrollTo() {
            const state = window.history.state;
            const property = (state || {}).property;
            if (!property)
                return;

            var focusId = null;
            switch (property) {
                case 'Title':
                    focusId = 'edit-static-text';
                    break;
                case 'EnablingCondition':
                    focusId = 'edit-question-enablement-condition';
                    break;
                case 'ValidationExpression':
                    focusId = 'validation-expression-' + state.indexOfEntityInProperty;
                    break;
                case 'ValidationMessage':
                    focusId = 'validation-message-' + state.indexOfEntityInProperty;
                    break;
                case 'AttachmentName':
                    focusId = 'edit-static-attachment-name';
                    break;
                default:
                    break;
            }

            setFocusIn(focusId);
        },
    }
};
</script>
