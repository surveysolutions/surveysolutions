<template>
    <form role="form" method="POST" id="staticText-editor" name="staticTextForm" unsaved-warning-form
        v-show="activeStaticText">
        <div id="show-reload-details-promt" class="ng-cloak" v-show="shouldUserSeeReloadDetailsPromt">
            <div class="inner">
                {{ $t('QuestionnaireEditor.QuestionToUpdateOptions') }}
                <a @click="fetch()" href="javascript:void(0);"
                    v-t="{ path: 'QuestionnaireEditor.QuestionClickReload' }"></a>
            </div>
        </div>
        <div class="form-holder">

            <Breadcrumbs :breadcrumbs="activeStaticText.breadcrumbs">
            </Breadcrumbs>
            <div class="form-group">
                <label class="wb-label">
                    {{ $t('QuestionnaireEditor.StaticText') }}</label><br />
                <ExpressionEditor mode="substitutions"
                            v-model="activeStaticText.text" />
            </div>
            <div class="form-group">
                <label for="edit-static-attachment-name" class="wb-label">
                    {{
                        $t('QuestionnaireEditor.StaticTextAttachmentName')
                    }}&nbsp;
                    <help key="attachmentName" />
                </label><br />
                <input id="edit-static-attachment-name" type="text" class="form-control"
                    v-model="activeStaticText.attachmentName" spellcheck="false" maxlength="32" />
            </div>
            <div class="form-group" v-show="doesStaticTextSupportEnablementConditions() &&
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

            <div class="row" v-show="doesStaticTextSupportEnablementConditions() &&
                ((showEnablingConditions === undefined &&
                    activeStaticText.enablementCondition) ||
                    showEnablingConditions)
                ">
                <div class="form-group col-xs-11">
                    <div class="enabling-group-marker" :class="{
                        'hide-if-disabled': activeStaticText.hideIfDisabled
                    }"></div>
                    <label>{{ $t('QuestionnaireEditor.EnablingCondition') }}
                        <help key="conditionExpression" />
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
                        <help key="hideIfDisabled" />
                    </label>
                    <ExpressionEditor v-model="activeStaticText.enablementCondition"/>
                    
                </div>
                <div class="form-group col-xs-1">
                    <button type="button" class="btn cross instructions-cross" @click="
                        showEnablingConditions = false;
                    activeStaticText.enablementCondition = '';
                    activeStaticText.hideIfDisabled = false;
                    dirty = false;
                    "></button>
                </div>
            </div>

            <div class="form-group validation-group" v-for="(validation,
                index) in activeStaticText.validationConditions" :id="'validationCondition' + index">
                <div class="validation-group-marker"></div>
                <label>{{ $t('QuestionnaireEditor.ValidationCondition') }}
                    {{ index + 1 }}
                    <help key="validationExpression" />
                </label>

                <input :id="'cb-isWarning' + index" type="checkbox" class="wb-checkbox" v-model="validation.severity"
                    :true-value="'Warning'" :false-value="'Error'" />
                <label :for="'cb-isWarning' + index"><span></span>{{ $t('QuestionnaireEditor.IsWarning') }}</label>

                <button class="btn delete-btn-sm delete-validation-condition" @click="removeValidationCondition(index)"
                    tabindex="-1"></button>                
                <ExpressionEditor v-model="validation.expression" mode="expression"/>
                <label class="validation-message">{{
                    $t('QuestionnaireEditor.ErrorMessage') }}
                    <help link="validationMessage" />
                </label>
                <ExpressionEditor v-model="validation.message" mode="substitutions"/>
            </div>
            <div class="form-group" v-if="activeStaticText.validationConditions.length < 10">
                <button type="button" class="btn btn-lg btn-link" @click="addValidationCondition()">
                    {{ $t('QuestionnaireEditor.AddValidationRule') }}
                </button>
            </div>
        </div>
        <div class="form-buttons-holder">
            <div class="pull-left">
                <button type="button" id="edit-static-text-save-button" v-show="!questionnaire.isReadOnlyForUser"
                    :class="{ 'btn-primary': dirty }" class="btn btn-lg" unsaved-warning-clear @click="saveStaticText()">
                    {{ $t('QuestionnaireEditor.Save') }}
                </button>
                <button type="button" id="edit-static-text-cancel-button" class="btn btn-lg btn-link" unsaved-warning-clear
                    @click="cancelStaticText()">
                    {{ $t('QuestionnaireEditor.Cancel') }}
                </button>
            </div>
            <div class="pull-right">
                <button type="button" v-show="!questionnaire.isReadOnlyForUser" id="add-comment-button"
                    class="btn btn-lg btn-link" @click="toggleComments(activeStaticText)" unsaved-warning-clear>
                    <span v-show="!isCommentsBlockVisible && commentsCount == 0">{{
                        $t('QuestionnaireEditor.EditorAddComment') }}
                    </span>
                    <span v-show="!isCommentsBlockVisible && commentsCount > 0">
                        {{
                            $t('QuestionnaireEditor.EditorShowComments', {
                                count: commentsCount
                            })
                        }}
                    </span>
                    <span v-show="isCommentsBlockVisible">{{ $t('QuestionnaireEditor.EditorHideComment') }}
                    </span>
                </button>
                <button type="button" id="edit-static-text-delete-button" v-show="!questionnaire.isReadOnlyForUser"
                    class="btn btn-lg btn-link" @click="deleteStaticText()" unsaved-warning-clear>
                    {{ $t('QuestionnaireEditor.Delete') }}
                </button>
                <MoveToChapterSnippet :item-id="staticTextId" v-show="!questionnaire.isReadOnlyForUser &&
                    !currentChapter.isReadOnly
                    ">
                </MoveToChapterSnippet>
            </div>
        </div>
    </form>
</template>

<script>
import { useCommentsStore } from '../../../stores/comments';
import { useStaticTextStore } from '../../../stores/staticText';
import { createQuestionForDeleteConfirmationPopup } from '../../../services/utilityService'

import Breadcrumbs from './Breadcrumbs.vue';
import ExpressionEditor from './ExpressionEditor.vue';
import Help from './Help.vue';
import MoveToChapterSnippet from './MoveToChapterSnippet.vue';

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
            activeStaticText: {
                breadcrumbs: '',
                text: '',
                validationConditions: [],
                enablementCondition: '',
                hideIfDisabled: false,
                attachmentName: ''
            },
            shouldUserSeeReloadDetailsPromt: true,
            showEnablingConditions: null,
            dirty: false,
            valid: true
        };
    },
    watch: {
        activeStaticText: {
            handler(newVal, oldVal) {
                if (oldVal != null) this.setDirty();
            },
            deep: true
        },

        async statictextId(newValue, oldValue) {
            if (newValue != oldValue) {
                this.questionStore.clear();
                await this.fetch();
            }
        }
    },
    setup() {
        const staticTextStore = useStaticTextStore();
        const commentsStore = useCommentsStore();

        return {
            staticTextStore,
            commentsStore
        };
    },
    async beforeMount() {
        await this.fetch();
    },
    computed: {
        commentsCount() {
            return this.commentsStore.getCommentsCount;
        },
        isCommentsBlockVisible() {
            return this.commentsStore.getIsCommentsBlockVisible;
        }
    },
    methods: {
        async fetch() {
            await this.staticTextStore.fetchStaticTextData(
                this.questionnaireId,
                this.statictextId
            );

            this.activeStaticText = this.staticTextStore.getStaticText;
            this.shouldUserSeeReloadDetailsPromt = false;
        },
        async saveStaticText() {
            if (this.dirty == false) return;

            if (!this.valid) return;

            this.staticTextStore.saveStaticTextData();

            this.dirty = false;
        },

        cancelStaticText() {
            this.staticTextStore.discardChanges();
            this.activeStaticText = this.staticTextStore.getData;
            this.dirty = false;
        },

        deleteStaticText() {
            var itemIdToDelete = this.statictextId;

            const params = createQuestionForDeleteConfirmationPopup(
                this.activeStaticText.text ||
                this.$t('QuestionnaireEditor.UntitledStaticText')
            );

            params.callback = confirm => {
                if (confirm) {
                    this.staticTextStore.deleteStaticText(itemIdToDelete);
                }
            };

            this.$confirm(params);
        },
        doesStaticTextSupportEnablementConditions() {
            return (
                this.activeStaticText && !this.currentChapter.isCover
            );
        },
        removeValidationCondition(index) {
            this.activeStaticText.validationConditions.splice(index, 1);
            this.setDirty();
        },
        addValidationCondition() {
            this.activeStaticText.validationConditions.push({
                expression: '',
                message: ''
            });
            this.setDirty();
        },

        toggleComments() {
            this.commentsStore.toggleComments();
        },

        setDirty() {
            this.dirty = true;
        }
    }
};
</script>
