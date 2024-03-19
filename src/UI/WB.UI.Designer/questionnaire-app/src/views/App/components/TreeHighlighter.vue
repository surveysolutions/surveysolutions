<template>
    <WordHighlighter :query="queryText" :htmlToHighlight="sanitizeText(text)" highlightClass="ui-match" />
</template>

<script>

import { sanitize } from '../../../services/utilityService';
import WordHighlighter from "vue-word-highlighter";

export default {
    name: 'TreeHighlighter',
    components: { WordHighlighter },
    props: {
        text: { type: String, required: true },
        searchText: { type: String, required: true },
    },
    data() {
        return {

        }
    },
    computed: {
        queryText() {
            return this.searchText || '';
        }
    },
    methods: {
        sanitizeText(value) {
            return sanitize(value);
        }
    }
}
</script>
