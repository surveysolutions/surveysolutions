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
                <span v-text="filter(item.title)"></span>
            </div>
            <div class="qname-block">
                <div class="conditions-block">
                    <div class="enabling-group-marker" :class="{ 'hide-if-disabled': item.hideIfDisabled }"
                        v-if="item.hasCondition"></div>
                </div>
                <span v-text="filter(item.variable)"></span>
            </div>
        </router-link>
    </TreeItem>
</template>

<script>
import { useQuestionnaireStore } from '../../../stores/questionnaire';
import { useTreeStore } from '../../../stores/tree';

import TreeItem from './TreeItem.vue';

export default {
    name: 'TreeGroup',
    components: {
        TreeItem
    },
    props: {
        item: { type: Object, required: true },
        stat: { type: Object, required: true }
    },
    data() {
        return {
            is_highlighted: false
        };
    },
    setup(props) {
        const treeStore = useTreeStore();
        const questionnaireStore = useQuestionnaireStore();

        return {
            treeStore,
            questionnaireStore
        };
    },
    computed: {
        questionnaire() {
            return this.questionnaireStore.info || {};
        },
        currentChapter() {
            return this.treeStore.getChapter;
        }
    },
    methods: {
        filter(value) {
            return value; // TODO | escape | highlight:search.searchText
        }
    }
};
</script>
