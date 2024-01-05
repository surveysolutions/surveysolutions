<template>
    <div class="options-editor">
        <div v-if="useListAsOptionsEditor">
            <div class="table-holder">
                <div class="table-row question-options-editor" style="height: 0">
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
                            <input type="number" v-focus min="-2147483648" max="2147483647" name="option_value"
                                v-model="option.value" v-pattern="/^([-+]?\d+)$/"
                                :class="{ 'has-error': !isInteger(option.value) }" @keypress="onKeyPressIsNumber($event)"
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
                    v-hide="activeQuestion.options.length >= MAX_OPTIONS_COUNT" @click="addOption()">{{
                        $t('QuestionnaireEditor.QuestionAddSpecialValues') }}</button>
                <button type="button" class="btn btn-link" v-if="activeQuestion.type != 'Numeric'"
                    v-hide="activeQuestion.options.length >= MAX_OPTIONS_COUNT" @click="addOption()">{{
                        $t('QuestionnaireEditor.QuestionAddOption') }}</button>
                <button type="button" class="btn btn-link" @click="showAddClassificationModal()">{{
                    $t('QuestionnaireEditor.QuestionAddClassification') }}</button>

                <button type="button" class="btn btn-link pull-right" @click="showOptionsInTextarea()">{{
                    $t('QuestionnaireEditor.StringsView') }}</button>
            </p>
        </div>
        <div v-if="!useListAsOptionsEditor">
            <div class="form-group" v-class="{ 'has-error': !stringifiedOptions.valid }">
                <textarea name="stringifiedOptions" class="form-control mono" v-model="activeQuestion.stringifiedOptions"
                    v-on:keyup="stringifiedOptionsValidate" match-options-pattern max-options-count msd-elastic></textarea>
                <p class="help-block">
                    <input class="btn btn-link" type="button" :value="$t('QuestionnaireEditor.TableView')" value="Show list"
                        @click="showOptionsInList()" :disabled="!stringifiedOptions.valid" />
                </p>
                <p class="help-block v-cloak" v-show="stringifiedOptions.$error.matchOptionsPattern">{{
                    $t('QuestionnaireEditor.OptionsListError') }}
                </p>
                <p class="help-block v-cloak" v-show="stringifiedOptions.$error.maxOptionsCount">{{
                    $t('QuestionnaireEditor.EnteredMoreThanAllowed', { count: 200 }) }}
                </p>
            </div>
        </div>
    </div>
    <add-classification :isReadOnlyForUser='questionnaire.isReadOnlyForUser || currentChapter.isReadOnly || false'
        :hasOptions='activeQuestion.optionsCount > 0'>
    </add-classification>
</template>

<script>

import { filter, isNull, isEmpty, isUndefined } from 'lodash'
import { newGuid } from '../../../../helpers/guid';
import { convertToText, validateText, convertToTable } from '../../../OptionsEditor/utils/tableToString';
import { isInteger } from '../../../../helpers/number';
import Help from '../Help.vue'
import AddClassification from './AddClassification.vue';

export default {
    name: 'OptionsEditorTemplate',
    components: {
        Help,
        AddClassification,
    },
    props: {
        activeQuestion: { type: Object, required: true }
    },
    data() {
        return {
            useListAsOptionsEditor: true,
            stringifiedOptions: {
                valid: true,
                $error: {
                    matchOptionsPattern: false,
                    maxOptionsCount: false
                }
            },
            MAX_OPTIONS_COUNT: 200,
            dirty: false,
        }
    },
    computed: {
        optionsCount() {
            return this.activeQuestion.options.length;
        }
    },
    methods: {
        async showOptionsInTextarea() {
            //this.activeQuestion.stringifiedOptions = optionsService.stringifyOptions(this.activeQuestion.options);
            this.activeQuestion.stringifiedOptions = await convertToText(this.activeQuestion.options);
            this.useListAsOptionsEditor = false;
        },

        async showOptionsInList() {
            if (this.useListAsOptionsEditor) {
                return;
            }

            this.stringifiedOptionsValidate();

            if (!this.stringifiedOptions.valid) {
                return;
            }

            if (this.activeQuestion.stringifiedOptions) {
                this.activeQuestion.options = await convertToTable(this.activeQuestion.stringifiedOptions);
                //this.activeQuestion.optionsCount = this.activeQuestion.options.length;
            }
            this.useListAsOptionsEditor = true;
        },

        stringifiedOptionsValidate() {
            if (this.useListAsRosterTitleEditor == true)
                return true;

            const lines = (this.activeQuestion.stringifiedOptions || '').split(/\r\n|\r|\n/);
            const lineCount = lines.length

            if (lineCount > this.fixedRosterLimit) {
                this.stringifiedOptions.$error.maxOptionsCount = true
                this.stringifiedOptions.valid = false;
                return;
            }
            else if (this.stringifiedOptions.$error.maxOptionsCount == true) {
                this.stringifiedOptions.$error.maxOptionsCount = false
            }

            const top5Errors = validateText(this.activeQuestion.stringifiedOptions, false).slice(0, 5);
            if (top5Errors.length > 0) {
                this.stringifiedOptions.$error.matchOptionsPattern = true;
                this.stringifiedOptions.valid = false;
                return;
            }
            else if (this.stringifiedOptions.$error.matchOptionsPattern == true) {
                this.stringifiedOptions.$error.matchOptionsPattern = false
            }

            if (this.stringifiedOptions.valid == false) {
                this.stringifiedOptions.valid = true;
            }
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
            this.markFormAsChanged();
        },

        markFormAsChanged() {
            this.dirty = true;
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
            var showModal = function () {
                var modalInstance = $uibModal.open({
                    templateUrl: 'views/add-classification.html',
                    backdrop: false,
                    windowClass: "add-classification-modal dragAndDrop",
                    controller: 'addClassificationCtrl',
                    resolve: {
                        isReadOnlyForUser: this.questionnaire.isReadOnlyForUser || this.currentChapter.isReadOnly || false,
                        hasOptions: this.activeQuestion.optionsCount > 0
                    }
                });

                modalInstance.result.then(function (selectedClassification) {
                    if (_.isNull(selectedClassification) || _.isUndefined(selectedClassification))
                        return;

                    var questionTitle = this.activeQuestion.title || $i18next.t('UntitledQuestion');
                    var replaceOptions = function () {

                        var optionsToInsertCount = selectedClassification.categoriesCount;

                        if (optionsToInsertCount > this.MAX_OPTIONS_COUNT) {
                            if (this.activeQuestion.type !== "SingleOption") {

                                var modalInstance = confirmService.open(
                                    utilityService.willBeTakenOnlyFirstOptionsConfirmationPopup(questionTitle,
                                        this.MAX_OPTIONS_COUNT));

                                modalInstance.result.then(function (confirmResult) {
                                    if (confirmResult === 'ok') {
                                        this.activeQuestion.options = selectedClassification.categories;
                                        this.activeQuestion.optionsCount = this.activeQuestion.options.length;
                                        markFormAsChanged();
                                    }
                                });
                            } else {
                                commandService.replaceOptionsWithClassification(
                                    $state.params.questionnaireId,
                                    this.activeQuestion.itemId,
                                    selectedClassification.id);

                                this.activeQuestion.isFilteredCombobox = true;
                                this.activeQuestion.options = selectedClassification.categories;
                                this.activeQuestion.optionsCount = this.activeQuestion.options.length;
                                markFormAsChanged();
                            }
                        } else {
                            if (this.activeQuestion.isFilteredCombobox) {
                                commandService.replaceOptionsWithClassification(
                                    $state.params.questionnaireId,
                                    this.activeQuestion.itemId,
                                    selectedClassification.id);
                            }
                            this.activeQuestion.options = selectedClassification.categories;
                            this.activeQuestion.optionsCount = selectedClassification.categories.length;
                            markFormAsChanged();
                        }
                    };

                    if (this.activeQuestion.options.length > 0) {
                        var modalInstance = confirmService.open(utilityService.replaceOptionsConfirmationPopup(questionTitle));
                        modalInstance.result.then(function (confirmResult) {
                            if (confirmResult === 'ok') {
                                replaceOptions();
                            }
                        });
                    } else {
                        replaceOptions();
                    }
                },
                    function () {

                    });
            };


            if (this.questionForm.$dirty) {
                this.saveQuestion(showModal);
            } else {
                showModal();
            }
        },
    }
}
</script>
