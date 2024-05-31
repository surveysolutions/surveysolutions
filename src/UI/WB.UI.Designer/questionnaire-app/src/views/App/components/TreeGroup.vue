<template>
    <TreeItem :item="item" :stat="stat">
        <router-link class="item-body" :id="item.itemId" :to="!item.isRoster
            ? {
                name: 'group',
                params: {
                    entityId: item.itemId
                }
            }
            : {
                name: 'roster',
                params: {
                    entityId: item.itemId
                }
            }
            ">
            <div class="item-text">
                <button type="button" :class="{
                    'btn-expand': !stat.open,
                    'btn-collapse': stat.open
                }" data-nodrag @click.native="stat.open = !stat.open" @mouseenter="is_highlighted = true"
                    @mouseleave="is_highlighted = false"></button>
                <span class="roster-marker" v-show="item.isRoster">{{
                    $t('QuestionnaireEditor.TreeRoster')
                }}&nbsp;</span>
                <TreeHighlighter :searchText="searchText" :text="item.title" />
            </div>
            <div class="qname-block">
                <TreeHighlighter :searchText="searchText" :text="item.variable" />
                <span>&nbsp;</span>
                <div class="conditions-block">
                    <div class="enabling-group-marker" :class="{ 'hide-if-disabled': item.hideIfDisabled }"
                        v-if="item.hasCondition"></div>
                </div>                
            </div>
        </router-link>
    </TreeItem>
</template>

<script>
import TreeItem from './TreeItem.vue';
import TreeHighlighter from './TreeHighlighter.vue'

export default {
    name: 'TreeGroup',
    components: {
        TreeItem,
        TreeHighlighter,
    },
    props: {
        item: { type: Object, required: true },
        stat: { type: Object, required: true },
        searchText: { type: String, required: true },
    },
    data() {
        return {
            is_highlighted: false
        };
    },
};
</script>
