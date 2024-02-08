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
                <!--                <span
                    ng-bind-html="item.title | escape | highlight:search.searchText"
                ></span>
-->
                <span v-sanitize-html="item.title"></span>
            </div>
            <div class="qname-block">
                <div class="conditions-block">
                    <div class="enabling-group-marker" :class="{ 'hide-if-disabled': item.hideIfDisabled }"
                        v-if="item.hasCondition"></div>
                    <div class="validation-group-marker" v-if="item.hasValidation"></div>
                </div>
                <!--span
                    ng-bind-html="item.variable | escape | highlight:search.searchText"
                ></span>
-->
                &nbsp;<span v-sanitize-html="item.variable"></span>
            </div>
        </router-link>
    </TreeItem>
</template>

<script>
import TreeItem from './TreeItem.vue';
import { answerTypeClass } from '../../../helpers/question'

export default {
    name: 'TreeQuestion',
    components: {
        TreeItem
    },
    props: {
        item: { type: Object, required: true }
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
