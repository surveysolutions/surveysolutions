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
                <label for="edit-static-text-highlight" class="wb-label">
                    {{ $t('QuestionnaireEditor.StaticText') }}</label><br />
                <div class="pseudo-form-control">
                    <div>
                        <!-- ui-ace="{ onLoad : setupAceForSubstitutions, require: ['ace/ext/language_tools'] }" -->
                        <ExpressionEditor id="edit-static-text-highlight" mode="substitutions" v-model:value="activeStaticText.text" />
                    </div>
                </div>
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
                    <label for="edit-question-enablement-condition">{{ $t('QuestionnaireEditor.EnablingCondition') }}
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

                    <div class="pseudo-form-control">
                        <ExpressionEditor v-model="activeStaticText.enablementCondition"></ExpressionEditor>
                    </div>
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

                <div class="pseudo-form-control">
                    <!-- <div ui-ace="{ onLoad : aceLoaded, require: ['ace/ext/language_tools'] }" v-bind="validation.expression"
                    v-attr-id="'validation-expression-' + $index" v-attr-tabindex="$index + 1"></div> -->
                    <ExpressionEditor v-model="validation.expression" mode="expression"></ExpressionEditor>
                </div>

                <label for="validationMessage{{$index}}" class="validation-message">{{
                    $t('QuestionnaireEditor.ErrorMessage') }}
                    <help link="validationMessage" />
                </label>
                <div class="pseudo-form-control">
                    <!--div ng-attr-id="{{'validation-message-' + $index}}" ng-attr-tabindex="{{$index + 1}}"
                        ui-ace="{ onLoad : setupAceForSubstitutions, require: ['ace/ext/language_tools'] }"
                        v-model="validation.message"></div-->
                    <ExpressionEditor v-model="validation.message" mode="substitutions"></ExpressionEditor>
                </div>
            </div>
            <div class="form-group" v-if="activeStaticText.validationConditions.length < 10">
                <button type="button" class="btn btn-lg btn-link" @click="addValidationCondition()">
                    {{ $t('QuestionnaireEditor.AddValidationRule') }}
                </button>
            </div>
        </div>
        <div class="form-buttons-holder">
            <div class="pull-left">
                <button id="edit-static-text-save-button" v-show="!questionnaire.isReadOnlyForUser"
                    :class="{ 'btn-primary': dirty }" class="btn btn-lg" unsaved-warning-clear @click="saveStaticText()">
                    {{ $t('QuestionnaireEditor.Save') }}
                </button>
                <button id="edit-static-text-cancel-button" class="btn btn-lg btn-link" unsaved-warning-clear
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
                <button id="edit-static-text-delete-button" v-show="!questionnaire.isReadOnlyForUser"
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
            // this.staticTextStore.deleteStaticTextData(
            //     this.questionnaireId,
            //     this.statictextId
            // );
        },        
        doesStaticTextSupportEnablementConditions() {
            return (
                this.activeStaticText && !this.activeStaticText.parentIsCover
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
