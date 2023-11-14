<template>
    <v-ace-editor
        v-model:value="editorValue"
        @init="editorInit"
        lang="html"
        theme="chrome"
        style="height: 300px"
    />
</template>

<script>
import { VAceEditor } from 'vue3-ace-editor';

export default {
    name: 'ExpressionEditor',
    components: { VAceEditor },
    emits: ['update:modelValue'],
    props: {
        modelValue: { type: String, required: true }
    },
    data() {
        return {
            //
        };
    },
    computed: {
        editorValue: {
            get() {
                return this.modelValue;
            },
            set(newValue) {
                this.$emit('update:modelValue', newValue);
            }
        }
    },
    methods: {
        editorInit() {
            //
        }
    }
};
</script>
