<template>
    <Highlighter highlightClassName="ui-match" :searchWords="keywords" :autoEscape="true" :sanitize="sanitizeText"
        :textToHighlight="text" />
</template>

<script>

import { sanitize } from '../../../services/utilityService';
import Highlighter from 'vue-highlight-words'

export default {
    name: 'TreeHighlighter',
    components: { Highlighter },
    props: {
        text: { type: Object, required: true },
        searchText: { type: String, required: true },
    },
    data() {
        return {

        }
    },
    computed: {
        keywords() {
            if (this.searchText)
                return [this.searchText]
            return []
        }
    },
    methods: {
        sanitizeText(value) {
            return sanitize(value);
        }
    }
}
</script>
