<template>
    <WordHighlighter :query="searchText" :htmlToHighlight="sanitizeText(text)" highlightClass="ui-match" />
</template>

<script>

import { sanitize } from '../../../services/utilityService';
import WordHighlighter from "vue-word-highlighter";

export default {
    name: 'TreeHighlighter',
    components: { WordHighlighter },
    props: {
        text: { type: Object, required: true },
        searchText: { type: String, required: true },
    },
    data() {
        return {

        }
    },
    methods: {
        sanitizeText(value) {
            return sanitize(value);
        }
    }
}
</script>
