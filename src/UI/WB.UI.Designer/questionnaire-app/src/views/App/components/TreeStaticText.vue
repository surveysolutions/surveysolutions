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
                <!--span
                    ng-if="item.text"
                    ng-bind-html="item.text | escape | highlight:search.searchText"
                ></span>
                <span
                    ng-if="!item.text && item.attachmentName"
                    ng-bind-html="'Attachment: ' + item.attachmentName | escape | highlight:search.searchText"
                ></span>
                <span
                    ng-if="!item.text && !item.attachmentName"
                    ng-bind-html="item.text | escape | highlight:search.searchText"
                ></span-->

                <span v-if="item.text" v-sanitize-html="item.text"></span>
                <span v-if="!item.text && item.attachmentName"
                    v-sanitize-html="'Attachment: ' + item.attachmentName"></span>
                <span v-if="!item.text && !item.attachmentName" v-sanitize-html="item.text"></span>
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

export default {
    name: 'TreeStaticText',
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
