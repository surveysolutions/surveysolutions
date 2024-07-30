<template>
    <vee-form ref="form" v-slot="{ errors, meta }">
        <div class="scroller">
            <ul class="breadcrumb">
                <li>{{ activeGroup.title }}</li>
                <li class="active">{{ activeClassification.title }}</li>
            </ul>
            <div class="categories-holder" v-if="activeClassification.id">
                <div v-if="!isEditMode" class="categories-holder-body readonly">
                    <div class="option-cell" v-for="(category, index) in categories" :key="category.id">
                        <span>{{ category.title }} <span>...</span></span>
                        <span>{{ category.value }}</span>
                    </div>
                </div>
                <div v-else class="categories-holder-body">
                    <div class="options-editor" v-if="optionsMode">
                        <div class="option-line" v-for="(category, index) in categories" :key="category.id">
                            <div class="input-group">
                                <div class="option-cell" :class="{ 'has-error': errors['value' + category.id] }">
                                    <vee-field :name="'value' + category.id" v-on:keyup.enter="moveFocus"
                                        v-validate="'required|integer'" key="value" type="number"
                                        rules="required|integer" v-model="category.value" class="form-control"
                                        :validateOnInput="true"
                                        :placeholder="$t('QuestionnaireEditor.OptionsUploadValue')" />
                                </div>
                                <div class="option-cell" :class="{ 'has-error': errors['title' + category.id] }">
                                    <vee-field :name="'title' + category.id" v-on:keyup.enter="moveFocus"
                                        v-validate="'required'" key="title" type="text" v-model="category.title"
                                        rules="required" class="form-control" :validateOnInput="true"
                                        :placeholder="$t('QuestionnaireEditor.OptionsUploadTitle')" />
                                </div>
                                <div class="input-group-btn">
                                    <button @click="deleteCategory(index)" type="button"
                                        class="btn btn-link btn-delete">
                                        <svg width="20" height="20">
                                            <polyline points="0,0 20,20"></polyline>
                                            <polyline points="20,0 0,20"></polyline>
                                        </svg>
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div v-else class="form-group" :class="{ 'has-error': errors.stringifiedOptions }">
                        <vee-field as="textarea" name="stringifiedOptions" v-autosize v-model="stringifiedOptions"
                            :rules="validateStringOptions" key="stringifiedOptions" class="form-control js-elasticArea"
                            :validateOnInput="true"
                            :placeholder="$t('QuestionnaireEditor.ClassificationsStringOptionsEditorPlaceholder')" />
                        <p v-if="errors.stringifiedOptions" class="text-danger">{{ errors.stringifiedOptions }}</p>
                    </div>
                </div>
                <div v-if="isEditMode" class="categories-holder-footer clearfix">
                    <button v-if="optionsMode" type="button" class="btn btn-link pull-left" @click="addCategory">{{
        $t('QuestionnaireEditor.QuestionAddOption') }}</button>
                    <button v-if="optionsMode" type="button" class="btn btn-link pull-right show-strings"
                        @click="showStrings">{{ $t('QuestionnaireEditor.StringsView') }}</button>
                    <button v-else type="button" class="btn btn-link pull-left" @click="showList">{{
        $t('QuestionnaireEditor.TableView') }}</button>
                </div>
            </div>
        </div>
        <div class="form-buttons-holder">
            <vee-field type="hidden" name="collectionSizeTracker" v-validate />
            <button type="button" class="btn btn-lg" :class="{ 'btn-success': meta.dirty }"
                :disabled="!meta.dirty ? 'disabled' : null" @click="save">{{ $t('QuestionnaireEditor.Save') }}</button>
        </div>
    </vee-form>
</template>

<script>

import { newGuid } from '../../../../../helpers/guid'
import { Form, Field, ErrorMessage } from 'vee-validate';
import _ from 'lodash'

export default {
    name: 'CategoriesEditor',
    components: {
        VeeForm: Form,
        VeeField: Field,
        ErrorMessage: ErrorMessage,
    },
    data: function () {
        return {
            optionsMode: true,
            stringifiedOptions: '',
            optionsParseRegex: new RegExp(/^(.+?)[\…\.\s]+([-+]?\d+)\s*$/),
        };
    },
    computed: {
        categories() {
            return this.$store.state.categories;
        },
        isEditMode() {
            return (
                this.$store.state.isAdmin ||
                this.$store.state.userId === this.$store.state.activeClassification.userId
            );
        },
        activeGroup() {
            return this.$store.state.activeGroup;
        },
        activeClassification() {
            return this.$store.state.activeClassification;
        },
        form() {
            return this.$refs.form;
        }
    },
    watch: {
        activeClassification: function (val) {
            this.optionsMode = true;
        }
    },
    methods: {
        validateOptionAsText(option) {
            return this.optionsParseRegex.test(option || '');
        },

        stringifyCategories() {
            var stringifiedOptions = '';
            var maxLength =
                _.max(
                    _.map(this.categories, function (o) {
                        return o.title.length;
                    })
                ) + 3;
            _.each(this.categories, function (category) {
                if (!_.isEmpty(category)) {
                    stringifiedOptions +=
                        (category.title || '') +
                        Array(
                            maxLength + 1 - (category.title || '').length
                        ).join('.') +
                        (category.value === 0 ? '0' : category.value || '');
                    stringifiedOptions += '\n';
                }
            });
            this.stringifiedOptions = stringifiedOptions.trim('\n');
        },

        parseOptions() {
            var self = this;
            var optionsStringList = (this.stringifiedOptions || '').split('\n');
            optionsStringList = _.filter(optionsStringList, function (line) {
                return !_.isEmpty(line);
            });

            var options = _.map(optionsStringList, function (item) {
                var matches = item.match(self.optionsParseRegex);
                return {
                    value: matches[2] * 1,
                    title: matches[1]
                };
            });

            return options;
        },
        moveFocus(event) {
            var parentCell = event.target.closest('div.option-cell');
            var nextCell = parentCell.nextElementSibling;

            while (nextCell && !nextCell.classList.contains('option-cell')) {
                nextCell = nextCell.nextElementSibling;
            }

            if (nextCell) {
                var input = nextCell.querySelector('input');
                if (input) {
                    input.focus();
                }
            } else {
                var parentLine = parentCell.closest('div.option-line');
                var nextLine = parentLine.nextElementSibling;

                while (nextLine && !nextLine.classList.contains('option-line')) {
                    nextLine = nextLine.nextElementSibling;
                }

                if (nextLine) {
                    var firstInput = nextLine.querySelector('input');
                    if (firstInput) {
                        firstInput.focus();
                    }
                }
            }
        },
        deleteCategory(index) {
            this.$store.dispatch('deleteCategory', index);
            this.form.setFieldValue('collectionSizeTracker', this.categories.length);
        },
        showStrings() {
            this.stringifyCategories();
            this.optionsMode = false;
        },
        showList() {
            var self = this;
            this.form.validate().then(function (result) {
                if (self.form.errors.stringifiedOptions) {
                } else {
                    var parsedCategories = self.parseOptions();

                    var commonLength = Math.min(
                        self.categories.length,
                        parsedCategories.length
                    );
                    for (var i = 0; i < commonLength; i++) {
                        self.$store.dispatch('updateCategory', {
                            index: i,
                            title: parsedCategories[i].title,
                            value: parsedCategories[i].value
                        });
                    }

                    if (self.categories.length < parsedCategories.length) {
                        // need to add
                        for (
                            var i = commonLength;
                            i < parsedCategories.length;
                            i++
                        ) {
                            self.$store.dispatch('addCategory', {
                                id: newGuid(),
                                isNew: true,
                                title: parsedCategories[i].title,
                                value: parsedCategories[i].value,
                                parent:
                                    self.$store.state.activeClassification.id
                            });
                        }
                    } else if (
                        self.categories.length > parsedCategories.length
                    ) {
                        //  need to remove
                        for (
                            var i = commonLength - 1;
                            i < self.categories.length;
                            i++
                        ) {
                            self.$store.dispatch('deleteCategory', i);
                        }
                    }

                    self.optionsMode = true;
                }
            });
        },
        save() {
            if (!this.optionsMode) {
                this.showList();
            }

            var self = this;
            this.$refs.form.validate().then(function (result) {
                if (result.valid) {
                    self.$store
                        .dispatch(
                            'updateCategories',
                            self.$store.state.activeClassification.id
                        )
                        .then(function () {
                            self.$nextTick(() => {
                                self.form.resetForm({ values: self.form.values });
                            });
                        });
                }
            });
        },
        addCategory() {
            this.$store.dispatch('addCategory', {
                id: newGuid(),
                isNew: true,
                title: '',
                value: null,
                parent: this.$store.state.activeClassification.id
            });
            this.form.setFieldValue('collectionSizeTracker', this.categories.length);
        },

        validateStringOptions(value) {
            if (!_.isEmpty(value)) {
                var options = (value || '').split('\n');
                var matchPattern = true;
                var invalidLines = [];
                _.forEach(options, (option, index) => {
                    var currentLineValidationResult = this.optionsParseRegex.test(
                        option || ''
                    );
                    matchPattern = matchPattern && currentLineValidationResult;
                    if (!currentLineValidationResult)
                        invalidLines.push(index + 1);
                });
                return invalidLines.length > 0
                    ? this.getMessageStringOptions(invalidLines)
                    : matchPattern
            }
            return true;
        },
        getMessageStringOptions(data) {
            return (
                "You entered an invalid input. Each line should follow the format: 'Title...Value[...Attachment name]'. 'Value' must be an integer number. 'Title' must be an alpha-numeric string. 'Attachment name' is optional. No empty lines are allowed. Lines: " +
                data +
                '.'
            );
        },
    }
}
</script>
