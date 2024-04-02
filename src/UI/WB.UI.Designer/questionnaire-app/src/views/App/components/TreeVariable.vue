<template>
    <TreeItem :item="item">
        <router-link class="item-body" :id="item.itemId" :to="{
            name: 'variable',
            params: {
                entityId: item.itemId
            }
        }">
            <div class="item-text">
                <div class="icon icon-variable"></div>
                <TreeHighlighter :searchText="searchText" :text="item.variableData.label" />
            </div>
            <div class="qname-block">
                <TreeHighlighter :searchText="searchText" :text="item.variableData.name" />
                <span>&nbsp;</span>
                <div class="conditions-block"></div>                
            </div>
        </router-link>
    </TreeItem>
</template>

<script>
import TreeItem from './TreeItem.vue';
import TreeHighlighter from './TreeHighlighter.vue'

export default {
    name: 'TreeVariable',
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
    }
};
</script>
