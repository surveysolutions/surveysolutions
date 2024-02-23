<template>
    <div class="options-editor">
        <div v-if="useListAsOptionsEditor">
            <div class="table-holder">
                <div class="table-row question-options-editor" style="height: 0"
                    v-if="activeQuestion.options && activeQuestion.options.length > 0">
                    <div class="column-2">
                        <label class="wb-label">{{ $t('QuestionnaireEditor.OptionsUploadValue') }}</label>
                        <help link="HelpOptionValue" />
                    </div>
                    <div class="column-3">
                        <label class="wb-label">{{ $t('QuestionnaireEditor.OptionsUploadTitle') }}</label>
                        <help link="HelpOptionTitle" />
                    </div>
                    <div class="column-35">
                        <label class="wb-label">{{ $t('QuestionnaireEditor.StaticTextAttachmentName') }}</label>
                        <help link="attachmentName" />
                    </div>
                </div>
                <div class="table-row question-options-editor" v-for="(option, index) in activeQuestion.options">
                    <div class="column-2">
                        <v-form name="options_value_form">
                            <input type="text" min="-2147483648" max="2147483647" v-model="option.value"
                                v-number="/^([-+]?\d*)$/" :name="'option_value_' + index"
                                :class="{ 'has-error': option.value && !isInteger(option.value) }"
                                @keypress="onKeyPressIsNumber($event)"
                                class="form-control question-option-value-editor border-right" />
                        </v-form>
                    </div>
                    <div class="column-3">
                        <input :attr-id="'option-title-' + index" type="text" v-model="option.title"
                            @keypress="onKeyPressInOptions($event)" class="form-control border-right" />
                    </div>
                    <div class="column-35">
                        <input :attr-id="'option-attachmentName-' + index" type="text" v-model="option.attachmentName"
                            @keypress="onKeyPressInOptions($event)" class="form-control border-right" />
                    </div>
                    <div class="column-4">
                        <a href="javascript:void(0);" @click="removeOption(index)" class="btn" tabindex="-1"></a>
                    </div>
                </div>
            </div>
            <p>
                <button type="button" class="btn btn-link" v-if="activeQuestion.type == 'Numeric'"
                    v-show="activeQuestion.options.length < MAX_OPTIONS_COUNT" @click="addOption()">{{
                        $t('QuestionnaireEditor.QuestionAddSpecialValues') }}</button>
                <button type="button" class="btn btn-link" v-if="activeQuestion.type != 'Numeric'"
                    v-show="activeQuestion.options.length < MAX_OPTIONS_COUNT" @click="addOption()">{{
                        $t('QuestionnaireEditor.QuestionAddOption') }}</button>
                <button type="button" class="btn btn-link" @click="showAddClassificationModal()">{{
                    $t('QuestionnaireEditor.QuestionAddClassification') }}</button>

                <button type="button" class="btn btn-link pull-right" @click="showOptionsInTextarea()">{{
                    $t('QuestionnaireEditor.StringsView') }}</button>
            </p>
        </div>
        <div v-if="!useListAsOptionsEditor">
            <div class="form-group" :class="{ 'has-error': !stringifiedCategoriesValidity.valid }">
                <textarea name="stringifiedOptions" class="form-control mono"
                    v-bind:value="activeQuestion.stringifiedCategories"
                    v-on:input="updateStringifiedCategoriesValue($event)" match-options-pattern max-options-count
                    v-autosize></textarea>
                <p class="help-block">
                    <input class="btn btn-link" type="button" :value="$t('QuestionnaireEditor.TableView')" value="Show list"
                        @click="showOptionsInList()" :disabled="!stringifiedCategoriesValidity.valid" />
                </p>
                <p class="help-block v-cloak" v-show="stringifiedCategoriesValidity.$error.matchOptionsPattern">{{
                    $t('QuestionnaireEditor.OptionsListError') }}
                </p>
                <p class="help-block v-cloak" v-show="stringifiedCategoriesValidity.$error.maxOptionsCount">{{
                    $t('QuestionnaireEditor.EnteredMoreThanAllowed', { count: 200 }) }}
                </p>
            </div>
        </div>
    </div>
    <add-classification ref="classification" :activeQuestion='activeQuestion' :questionnaireId="questionnaireId">
    </add-classification>
</template>

<script>

import { useQuestionStore } from '../../../../stores/question';
import { filter, isNull, isEmpty, isUndefined } from 'lodash'
import { newGuid } from '../../../../helpers/guid';
import { convertToText, validateText, convertToTable } from '../../../OptionsEditor/utils/tableToString';
import { isInteger } from '../../../../helpers/number';
import Help from '../Help.vue'
import AddClassification from './AddClassification.vue';
import { focusElementByName } from '../../../../services/utilityService'

export default {
    name: 'OptionsEditorTemplate',
    components: {
        Help,
        AddClassification,
    },
    expose: ['showOptionsInList'],
    props: {
        questionnaireId: { type: String, required: true },
        activeQuestion: { type: Object, required: true }
    },
    data() {
        return {
            useListAsOptionsEditor: true,

            stringifiedCategoriesValidity: {
                valid: true,
                $error: {
                    matchOptionsPattern: false,
                    maxOptionsCount: false
                }
            },
            MAX_OPTIONS_COUNT: 200,
        }
    },
    setup() {
        const questionStore = useQuestionStore();

        return {
            questionStore
        };
    },
    mounted() {
        this.$emitter.on('questionChangesDiscarded', this.questionChangesDiscarded);
    },
    unmounted() {
        this.$emitter.off('questionChangesDiscarded', this.questionChangesDiscarded);
    },
    computed: {
        optionsCount() {
            return this.activeQuestion.options.length;
        }
    },
    methods: {
        async showOptionsInTextarea() {
            const text = await convertToText(this.activeQuestion.options);
            this.questionStore.initStringifiedCategories(text);
            this.useListAsOptionsEditor = false;
        },
        questionChangesDiscarded() {
            this.reset()
        },

        async showOptionsInList() {
            if (this.useListAsOptionsEditor) {
                return;
            }

            if (this.questionStore.haveStringifiedCategoriesChanded()) {

                this.stringifiedOptionsValidate();

                if (!this.stringifiedCategoriesValidity.valid) {
                    return;
                }

                if (this.activeQuestion.stringifiedCategories) {
                    this.activeQuestion.options = await convertToTable(this.activeQuestion.stringifiedCategories);
                }
            }

            this.questionStore.initStringifiedCategories('');

            this.useListAsOptionsEditor = true;
            this.setValidityState(true);
        },

        updateStringifiedCategoriesValue(e) {
            this.activeQuestion.stringifiedCategories = e.target.value;
            this.stringifiedOptionsValidate();
        },
        stringifiedOptionsValidate() {
            if (this.useListAsRosterTitleEditor == true)
                return true;

            const lines = (this.activeQuestion.stringifiedCategories || '').split(/\r\n|\r|\n/);
            const lineCount = lines.length

            if (lineCount > this.fixedRosterLimit) {
                this.stringifiedCategoriesValidity.$error.maxOptionsCount = true
                this.setValidityState(false);
                return;
            }
            else if (this.stringifiedCategoriesValidity.$error.maxOptionsCount == true) {
                this.stringifiedCategoriesValidity.$error.maxOptionsCount = false
            }

            const top5Errors = validateText(this.activeQuestion.stringifiedCategories, false).slice(0, 5);
            if (top5Errors.length > 0) {
                this.stringifiedCategoriesValidity.$error.matchOptionsPattern = true;
                this.setValidityState(false);
                return;
            }
            else if (this.stringifiedCategoriesValidity.$error.matchOptionsPattern == true) {
                this.stringifiedCategoriesValidity.$error.matchOptionsPattern = false
            }

            if (this.stringifiedCategoriesValidity.valid == false) {
                this.setValidityState(true);
            }
        },

        setValidityState(state) {
            this.stringifiedCategoriesValidity.valid = state;
            this.questionStore.setValidityState(state);
        },

        trimEmptyOptions() {
            var notEmptyOptions = filter(this.activeQuestion.options, function (o) {
                return !isNull(o.value || null) || !isEmpty(o.title || '');
            });
            this.activeQuestion.options = notEmptyOptions;
        },

        addOption() {
            if (this.activeQuestion.optionsCount >= this.MAX_OPTIONS_COUNT)
                return;

            this.activeQuestion.options.push({
                "value": null,
                "title": '',
                "id": newGuid()
            });
            this.activeQuestion.optionsCount += 1;

            this.$nextTick(() => {
                const index = this.activeQuestion.options.length - 1;
                focusElementByName('option_value_' + index)
            })
        },

        removeOption(index) {
            this.activeQuestion.options.splice(index, 1);
        },

        onKeyPressInOptions(keyEvent) {
            if (keyEvent && keyEvent.which === 13) {
                keyEvent.preventDefault();

                if (this.activeQuestion.options.length >= this.MAX_OPTIONS_COUNT)
                    return;

                this.addOption();
            }
        },

        onKeyPressIsNumber(keyEvent) {
            const charCode = (keyEvent.which) ? keyEvent.which : keyEvent.keyCode;
            if ((charCode > 31 && (charCode < 48 || charCode > 57))
                && charCode !== 13 // enter
                && charCode !== 45 // -
                && charCode !== 43 // +
            ) {
                keyEvent.preventDefault();;
            } else {
                this.onKeyPressInOptions(keyEvent)
                return true;
            }
        },

        isInteger(value) {
            return isInteger(value);
        },

        showAddClassificationModal() {
            this.$refs.classification.openDialog();
        },

        reset() {
            this.useListAsOptionsEditor = true;
            this.setValidityState(true);
        }
    }
}
</script>
