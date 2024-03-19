<template>
    <TreeItem :item="item">
        <router-link class="item-body" :id="item.itemId" :to="{
            name: 'question',
            params: {
                entityId: item.itemId
            }
        }">
            <div class="item-text">
                <div class="icon" :class="questionClass"></div>
                <TreeHighlighter :searchText="searchText" :text="item.title" />
            </div>
            <div class="qname-block">
                <div class="conditions-block">
                    <div class="criticality-group-marker" v-if="item.isCritical"></div>
                    <div class="enabling-group-marker" :class="{ 'hide-if-disabled': item.hideIfDisabled }"
                        v-if="item.hasCondition"></div>
                    <div class="validation-group-marker" v-if="item.hasValidation"></div>
                </div>&nbsp;
                <TreeHighlighter :searchText="searchText" :text="item.variable" />
            </div>
        </router-link>
    </TreeItem>
</template>

<script>
import TreeItem from './TreeItem.vue';
import TreeHighlighter from './TreeHighlighter.vue'
import { answerTypeClass } from '../../../helpers/question'

export default {
    name: 'TreeQuestion',
    components: {
        TreeItem,
        TreeHighlighter,
    },
    props: {
        item: { type: Object, required: true },
        searchText: { type: String, required: true },
    },
    data() {
        return {};
    },
    computed: {
        questionClass() {
            return [answerTypeClass[this.item.type]];
        }
    },
    methods: {}
};
</script>
