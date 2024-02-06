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
                <!--span
                    ng-bind-html="item.variableData.label | escape | highlight:search.searchText"
                ></span-->
                <span v-sanitize-html="item.variableData.label"></span>
            </div>
            <div class="qname-block">
                <div class="conditions-block"></div>
                <!--span
                    ng-bind-html="item.variableData.name | escape | highlight:search.searchText"
                ></span-->
                <span v-sanitize-html="item.variableData.name"></span>
            </div>
        </router-link>
    </TreeItem>
</template>

<script>
import TreeItem from './TreeItem.vue';

export default {
    name: 'TreeVariable',
    components: {
        TreeItem
    },
    props: {
        item: { type: Object, required: true }
    },
    data() {
        return {};
    }
};
</script>
