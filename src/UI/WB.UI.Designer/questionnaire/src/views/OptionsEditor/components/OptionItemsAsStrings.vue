<template>
    <v-container fluid class="categories-strings pa-4">
        <v-textarea ref="strings" v-model="categoriesAsText" rows="15" variant="outlined" spellcheck="false"
            wrap="off" autocorrect="off" hide-details="auto" :error="!validity" :disabled="loading || convert"
            :loading="loading || convert" :readonly="readonly" class="categories-strings__textarea"
            style="font-family: monospace, monospace" @change="change" @focus="onFocus" @blur="onBlur">
            <template #prepend-inner>
                <div ref="lineNumbers" class="categories-strings__line-numbers" :style="lineNumbersStyle"
                    aria-hidden="true">{{ lineNumbersText }}</div>
            </template>
            <template #details>
                <div v-if="validationMessage" class="categories-strings__validation">
                    <div class="categories-strings__validation-summary">{{ validationMessage }}</div>
                    <ul v-if="validationErrors.length > 0" class="categories-strings__validation-list">
                        <li v-for="error in validationErrors" :key="`${error.lineNumber}:${error.line}`">
                            <button type="button" class="categories-strings__validation-link"
                                @click="goToLine(error.lineNumber)">
                                {{ error.lineNumber }}: {{ error.line }}
                            </button>
                        </li>
                    </ul>
                </div>
            </template>
        </v-textarea>
    </v-container>
</template>

<script>
import {
    convertToText,
    getValidationErrors,
    convertToTable
} from '../utils/tableToString';
import { isEqual } from 'lodash';

export default {
    name: 'CategoriesStrings',
    expose: ['isDirty'],

    props: {
        categories: { type: Array, required: true },
        showParentValue: { type: Boolean, required: true },
        loading: { type: Boolean, required: true },
        readonly: { type: Boolean, required: true }
    },

    data() {
        return {
            categoriesAsText: null,
            initialCategoriesAsText: null,
            convert: false,
            validity: true,
            validationMessage: null,
            validationErrors: [],
            textareaElement: null
        };
    },

    computed: {
        categoriesAsTextSplit() {
            return (this.categoriesAsText || '').split(/\r\n|\r|\n/);
        },

        lineCount() {
            return this.categoriesAsTextSplit.length;
        },

        lineNumbersText() {
            return Array.from(
                { length: Math.max(this.lineCount, 1) },
                (_, index) => index + 1
            ).join('\n');
        },

        lineNumbersStyle() {
            return {
                '--categories-strings-line-numbers-width': `${Math.max(
                    `${Math.max(this.lineCount, 1)}`.length + 2,
                    4
                )}ch`
            };
        },

        valid() {
            return this.$refs.strings.valid;
        },

        isDirty() {
            const equal = isEqual(this.categoriesAsText, this.initialCategoriesAsText)
            return !equal;
        },
    },

    watch: {
        categories() {
            this.reload();
        },

        categoriesAsText(value) {
            this.validate(value);
        },

        validity(to, from) {
            if (to != from) {
                this.$emit('string-valid', to === true);
            }
        },

        isDirty(newVal) {
            this.$emit('isDirty', newVal)
        }
    },

    mounted() {
        this.reload();
        this.$nextTick(() => this.attachTextarea());
    },

    beforeUnmount() {
        this.detachTextarea();
    },

    methods: {
        validate(value) {
            if (this.lineCount == 0) {
                this.validationMessage = null;
                this.validationErrors = [];
                this.validity = true;
                return true;
            }

            if (this.lineCount > 15000) {
                this.validationMessage = this.$t('QuestionnaireEditor.OptionsSizeLimit', {
                    max_rows: 15000
                });
                this.validationErrors = [];
                this.validity = false;
                return false;
            }

            const top5Errors = getValidationErrors(value, this.showParentValue).slice(
                0,
                5
            );

            if (top5Errors.length > 0) {
                this.validationMessage = this.showParentValue
                    ? this.$t('QuestionnaireEditor.OptionsCascadingListError')
                    : this.$t('QuestionnaireEditor.OptionsListError');
                this.validationErrors = top5Errors;
                this.validity = false;
                return false;
            }

            this.validationMessage = null;
            this.validationErrors = [];
            this.validity = true;
            return true;
        },
        change(value) {
            const validateResult = this.validate(this.categoriesAsText);
            if (validateResult === true) {
                const categories = convertToTable(this.categoriesAsText, this.showParentValue);
                this.$emit('changeCategories', categories);
            }
        },

        onFocus() {
            this.$emit('editing', true);
        },

        onBlur() {
            this.$emit('editing', false);
        },

        attachTextarea() {
            this.detachTextarea();
            this.textareaElement = this.$refs.strings?.$el?.querySelector('textarea');
            this.textareaElement?.addEventListener('scroll', this.syncLineNumbersScroll, {
                passive: true
            });
            this.syncLineNumbersScroll();
        },

        detachTextarea() {
            this.textareaElement?.removeEventListener('scroll', this.syncLineNumbersScroll);
            this.textareaElement = null;
        },

        syncLineNumbersScroll() {
            if (this.$refs.lineNumbers != null && this.textareaElement != null) {
                this.$refs.lineNumbers.scrollTop = this.textareaElement.scrollTop;
            }
        },

        goToLine(lineNumber) {
            if (this.textareaElement == null) {
                this.attachTextarea();
            }

            if (this.textareaElement == null) {
                return;
            }

            const range = this.getLineRange(lineNumber);
            const lineHeight = parseFloat(getComputedStyle(this.textareaElement).lineHeight) || 24;

            this.textareaElement.focus();
            this.textareaElement.setSelectionRange(range.start, range.end);
            this.textareaElement.scrollTop = Math.max(lineHeight * (lineNumber - 2), 0);
            this.syncLineNumbersScroll();
        },

        getLineRange(lineNumber) {
            const text = this.categoriesAsText || '';
            let currentLine = 1;
            let start = 0;

            for (let index = 0; index < text.length && currentLine < lineNumber; index++) {
                if (text[index] === '\r') {
                    if (text[index + 1] === '\n') {
                        index++;
                    }
                    currentLine++;
                    start = index + 1;
                } else if (text[index] === '\n') {
                    currentLine++;
                    start = index + 1;
                }
            }

            let end = start;
            while (end < text.length && text[end] !== '\r' && text[end] !== '\n') {
                end++;
            }

            return { start, end };
        },

        reload() {
            if (this.convert) return;

            this.convert = true;
            this.$emit('inprogress', true);

            convertToText(this.categories, this.showParentValue).then(data => {
                this.$nextTick(() => {
                    this.categoriesAsText = data;
                    this.initialCategoriesAsText = data;
                    this.attachTextarea();
                });

                this.convert = false;
                this.$emit('inprogress', false);
            });
        }
    }
};
</script>

<style scoped>
.categories-strings__textarea :deep(.v-field__overlay) {
    display: none;
}

.categories-strings__textarea :deep(.v-field__prepend-inner) {
    align-self: stretch;
    padding-top: 0;
    margin-inline-end: 0;
}

.categories-strings__textarea :deep(.v-field__input) {
    padding-top: 0;
}

.categories-strings__textarea :deep(textarea) {
    padding-top: 16px;
}

.categories-strings__line-numbers {
    width: var(--categories-strings-line-numbers-width);
    min-width: var(--categories-strings-line-numbers-width);
    height: 100%;
    padding: 16px 8px 16px 0;
    border-right: 1px solid rgba(0, 0, 0, 0.12);
    overflow: hidden;
    color: rgba(0, 0, 0, 0.6);
    font-family: monospace, monospace;
    line-height: 1.5;
    text-align: right;
    white-space: pre;
    user-select: none;
}

.categories-strings__validation {
    width: 100%;
    padding-top: 4px;
    white-space: normal;
}

.categories-strings__validation-summary {
    white-space: pre-wrap;
}

.categories-strings__validation-list {
    margin: 8px 0 0;
    padding-left: 20px;
}

.categories-strings__validation-link {
    border: 0;
    padding: 0;
    background: transparent;
    color: inherit;
    text-align: left;
    white-space: pre-wrap;
    text-decoration: underline;
}
</style>
