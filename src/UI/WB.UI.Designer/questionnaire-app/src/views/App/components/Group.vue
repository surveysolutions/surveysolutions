<template>
    <form role="form" id="roster-editor" name="groupForm" unsaved-warning-form v-if="activeGroup">
        <div class="form-holder">

            <Breadcrumbs :breadcrumbs="breadcrumbs">
            </Breadcrumbs>

            <div class="form-group">
                <label for="edit-group-title-highlight" class="wb-label">
                    {{ $t('QuestionnaireEditor.GroupTitle') }}
                </label>
                <br />
                <div class="pseudo-form-control">
                    <!--div id="edit-group-title-highlight"
                        ui-ace="{ onLoad : setupAceForSubstitutions, require: ['ace/ext/language_tools'] }"
                        ng-model="activeGroup.title"></div-->
                    <ExpressionEditor v-model="activeGroup.title"></ExpressionEditor>
                </div>
            </div>

            <div class="row">
                <div class="form-group input-variable-name col-xs-12 pull-right">
                    <label for="edit-group-variableName" class="wb-label">
                        {{ $t('QuestionnaireEditor.SectionVariableName') }}
                        <help link="variableName" placement="left" />
                    </label>
                    <br />
                    <input id="edit-group-variableName" type="text" class="wb-input bg-white width-auto"
                        v-model="activeGroup.variableName" maxlength="32" spellcheck="false">
                </div>
            </div>

            <div class="form-group"
                v-if="!activeGroup.isCoverPage && !((showEnablingConditions === undefined && activeGroup.enablementCondition) || showEnablingConditions)">
                <button type="button" class="btn btn-lg btn-link" @click="showEnablingConditions = true">
                    {{ $t('QuestionnaireEditor.AddEnablingCondition') }}
                </button>
            </div>

            <div class="row"
                v-if="!activeGroup.isCoverPage && ((showEnablingConditions === undefined && activeGroup.enablementCondition) || showEnablingConditions)">
                <div class="form-group col-xs-11">
                    <div class="enabling-group-marker" :class="{ 'hide-if-disabled': activeGroup.hideIfDisabled }"></div>
                    <label for="edit-group-condition">{{ $t('QuestionnaireEditor.EnablingCondition') }}
                        <help link="conditionExpression" />
                    </label>

                    <input type="checkbox" class="wb-checkbox" disabled="disabled" checked="checked"
                        v-if="questionnaire.hideIfDisabled" :title="$t('QuestionnaireEditor.HideIfDisabledNested')" />
                    <input v-if="!questionnaire.hideIfDisabled" id="cb-hideIfDisabled" type="checkbox" class="wb-checkbox"
                        v-model="activeGroup.hideIfDisabled" />
                    <label for="cb-hideIfDisabled">
                        <span :title="questionnaire.hideIfDisabled ? $t('QuestionnaireEditor.HideIfDisabledNested') : ''">
                        </span>
                        {{ $t('QuestionnaireEditor.HideIfDisabled') }}
                        <help link="hideIfDisabled" />
                    </label>
                    <br>
                    <div class="pseudo-form-control">
                        <!--div id="edit-group-condition" ui-ace="{ onLoad : aceLoaded , require: ['ace/ext/language_tools']}"
                            ng-model="activeGroup.enablementCondition"></div-->
                        <ExpressionEditor v-model="activeGroup.enablementCondition"></ExpressionEditor>
                    </div>
                </div>
                <div class="form-group col-xs-1">
                    <button type="button" class="btn cross instructions-cross"
                        @click="showEnablingConditions = false; activeGroup.enablementCondition = ''; activeGroup.hideIfDisabled = false; dirty = true;"></button>
                </div>
            </div>

        </div>
        <div class="form-buttons-holder">
            <div class="pull-left">
                <button type="button" v-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                    id="edit-chapter-save-button" class="btn btn-lg" :class="{ 'btn-primary': dirty }" @click="saveGroup()"
                    unsaved-warning-clear :disabled="!valid">
                    {{ $t('QuestionnaireEditor.Save') }}
                </button>
                <button type="reset" id="edit-chapter-cancel-button" class="btn btn-lg btn-link" @click="cancelGroup()"
                    unsaved-warning-clear>
                    {{ $t('QuestionnaireEditor.Cancel') }}
                </button>
            </div>
            <div class="pull-right">
                <button type="button" v-show="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                    id="add-comment-button" class="btn btn-lg btn-link" @click="toggleComments(activeQuestion)"
                    unsaved-warning-clear>
                    <span v-show="!isCommentsBlockVisible && commentsCount == 0">{{
                        $t('QuestionnaireEditor.EditorAddComment') }}
                    </span>
                    <span v-show="!isCommentsBlockVisible && commentsCount > 0">{{
                        $t('QuestionnaireEditor.EditorShowComments', {
                            count: commentsCount
                        }) }}
                    </span>
                    <span v-show="isCommentsBlockVisible">
                        {{ $t('QuestionnaireEditor.EditorHideComment') }}
                    </span>
                </button>
                <button type="button" v-show="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                    v-if="!activeGroup.isCoverPage" id="edit-chapter-delete-button" class="btn btn-lg btn-link"
                    @click="deleteItem()" unsaved-warning-clear>{{ $t('QuestionnaireEditor.Delete') }}</button>
                <MoveToChapterSnippet :item-id="groupId"
                    v-show="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                    v-if="!activeGroup.isChapter && !activeGroup.isCoverPage">
                </MoveToChapterSnippet>
            </div>
        </div>
    </form>
</template>

<script>
import { useGroupStore } from '../../../stores/group';
import { useCommentsStore } from '../../../stores/comments';
import MoveToChapterSnippet from './MoveToChapterSnippet.vue';
import ExpressionEditor from './ExpressionEditor.vue';
import Breadcrumbs from './Breadcrumbs.vue'
import Help from './Help.vue'

export default {
    name: 'Group',
    components: { MoveToChapterSnippet, ExpressionEditor, Breadcrumbs, Help },
    inject: ['questionnaire', 'currentChapter'],
    props: {
        questionnaireId: { type: String, required: true },
        groupId: { type: String, required: true }
    },
    data() {
        return {
            activeGroup: null,
            breadcrumbs: [],
            showEnablingConditions: undefined,
            dirty: false,
            valid: true
        };
    },
    watch: {
        activeGroup: {
            handler(newVal, oldVal) {
                if (oldVal != null) this.dirty = true;
            },
            deep: true
        },

        async groupId(newValue, oldValue) {
            if (newValue != oldValue) {
                this.groupStore.clear();
                await this.fetch();
            }
        }
    },
    setup() {
        const groupStore = useGroupStore();
        const commentsStore = useCommentsStore();

        return {
            groupStore, commentsStore
        };
    },
    async beforeMount() {
        await this.fetch();
    },
    computed: {
        typeName() {
            const option = this.activeGroup.typeOptions.find(
                p => p.value == this.activeGroup.type
            );
            return option != null ? option.text : null;
        },
        commentsCount() {
            return this.commentsStore.getCommentsCount;
        },
        isCommentsBlockVisible() {
            return this.commentsStore.getIsCommentsBlockVisible;
        }
    },
    methods: {
        async fetch() {
            await this.groupStore.fetchGroupData(
                this.questionnaireId,
                this.groupId
            );

            this.activeGroup = this.groupStore.getGroup;
            this.breadcrumbs = this.groupStore.getBreadcrumbs;
        },
        saveGroup() {
            this.groupStore.saveGroupData();
            this.dirty = false;
        },
        cancel() {
            this.groupStore.discardChanges();
            this.activeGroup = this.groupStore.getData;
            this.dirty = false;
        },
        toggleComments() {
            this.commentsStore.toggleComments();
        }
    }
};
</script>
