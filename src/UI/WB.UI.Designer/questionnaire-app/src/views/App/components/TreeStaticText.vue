<template>
    <TreeItem :item="item">
        <router-link class="item-body" :id="item.itemId" :to="{
            name: 'statictext',
            params: {
                entityId: item.itemId
            }
        }">
            <div class="item-text">
                <div class="icon icon-statictext"></div>
                <TreeHighlighter :searchText="searchText" :text="title" />
            </div>
            <div class="qname-block">
                <div class="conditions-block">
                    <div class="enabling-group-marker" :class="{ 'hide-if-disabled': item.hideIfDisabled }"
                        v-if="item.hasCondition"></div>
                    <div class="validation-group-marker" v-if="item.hasValidation"></div>
                </div>
            </div>
        </router-link>
    </TreeItem>
</template>

<script>
import TreeItem from './TreeItem.vue';
import TreeHighlighter from './TreeHighlighter.vue'

export default {
    name: 'TreeStaticText',
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
        title() {
            if (this.item.text)
                return this.item.text;
            if (this.item.attachmentName)
                return 'Attachment: ' + this.item.attachmentName
            return this.item.text;
        }
    }
};
</script>
