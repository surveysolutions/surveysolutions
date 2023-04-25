<template>
    <v-container fluid>
        <v-textarea
            ref="strings"
            v-model="categoriesAsText"
            rows="15"
            filled
            spellcheck="false"
            wrap="off"
            autocorrect="off"
            :rules="textRules"
            :disabled="loading || convert"
            :loading="loading || convert"
            :readonly="readonly"
            style="font-family: monospace, monospace"
            @change="change"
            @focus="onFocus"
            @blur="onBlur"
        >
            <template #message="{ message }">
                <div style="white-space: pre-wrap;">{{ message }}</div>
            </template>
        </v-textarea>
    </v-container>
</template>

<script>
import {
    convertToText,
    validateText,
    convertToTable
} from '../utils/tableToString';

export default {
    name: 'CategoriesStrings',

    props: {
        categories: { type: Array, required: true },
        showParentValue: { type: Boolean, required: true },
        loading: { type: Boolean, required: true },
        readonly: { type: Boolean, required: true }
    },

    data() {
        return {
            categoriesAsText: null,
            convert: false,
            validity: true
        };
    },

    computed: {
        textRules() {
            return [
                value => {
                    this.validity = this.validate(value);
                    return this.validity;
                }
            ];
        },

        categoriesAsTextSplit() {
            return (this.categoriesAsText || '').split(/\r\n|\r|\n/);
        },

        lineCount() {
            return this.categoriesAsTextSplit.length;
        },

        valid() {
            return this.$refs.strings.valid;
        }
    },

    watch: {
        categories() {
            this.reload();
        },

        validity(to, from) {
            if (to != from) {
                this.$emit('string-valid', to === true);
            }
        }
    },

    mounted() {
        this.reload();
    },

    methods: {
        validate(value) {
            if (this.lineCount == 0) {
                return true;
            }

            if (this.lineCount > 15000) {
                return this.$t('QuestionnaireEditor.OptionsSizeLimit', {
                    max_rows: 15000
                });
            }

            const top5Errors = validateText(value, this.showParentValue).slice(
                0,
                5
            );

            if (top5Errors.length > 0) {
                const error = [
                    this.showParentValue
                        ? this.$t(
                              'QuestionnaireEditor.OptionsCascadingListError'
                          )
                        : this.$t('QuestionnaireEditor.OptionsListError'),
                    '',
                    ...top5Errors
                ].join('\r\n');
                return error;
            }

            return true;
        },
        change(value) {
            if (this.validate(this.categoriesAsText)) {
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

        reload() {
            if (this.convert) return;

            this.convert = true;
            this.$emit('inprogress', true);

            convertToText(this.categories, this.showParentValue).then(data => {
                this.$nextTick(() => {
                    this.categoriesAsText = data;
                });

                this.convert = false;
                this.$emit('inprogress', false);
            });
        }
    }
};
</script>
