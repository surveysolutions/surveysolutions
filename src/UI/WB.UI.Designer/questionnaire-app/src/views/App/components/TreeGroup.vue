<template>
    <div
        context-menu
        class="section item group"
        :data-bs-target="'context-menu-' + item.itemId"
        :class="{
            highlight: item.itemId === highlightedId,
            highlighted: is_highlighted,
            roster: item.isRoster
        }"
        ui-sref-active="selected"
        context-menu-hide-on-mouse-leave="true"
    >
        <span class="cursor"></span>
        <a class="handler" ui-tree-handle></a>
        <a
            class="item-body"
            :id="item.itemId"
            ui-sref="{{ !item.isRoster ? 'questionnaire.chapter.group({itemId: item.itemId})' : 'questionnaire.chapter.roster({itemId: item.itemId})' }}"
        >
            <div class="item-text">
                <button
                    type="button"
                    :class="{
                        'btn-expand': collapsed,
                        'btn-collapse': !collapsed
                    }"
                    data-nodrag
                    @click="toggle(this)"
                    @mouseenter="is_highlighted = true"
                    @mouseleave="is_highlighted = false"
                ></button>
                <span
                    class="roster-marker"
                    v-show="item.isRoster"
                    v-i18next="TreeRoster"
                ></span>
                <span v-text="filter(item.title)"></span>
            </div>
            <div class="qname-block">
                <div class="conditions-block">
                    <div
                        class="enabliv-group-marker"
                        :class="{ 'hide-if-disabled': item.hideIfDisabled }"
                        v-if="item.hasCondition"
                    ></div>
                </div>
                <span v-text="filter(item.variable)"></span>
            </div>
        </a>
        <div
            class="dropdown position-fixed"
            :id="'context-menu-' + item.itemId"
        >
            <ul class="dropdown-menu" role="menu">
                <li>
                    <a
                        @click="addQuestion(item)"
                        v-if="
                            !questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly
                        "
                        v-i18next="TreeAddQuestion"
                        >Add question</a
                    >
                </li>
                <li>
                    <a
                        @click="addGroup(item)"
                        v-if="
                            !questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly &&
                                !currentChapter.isCover
                        "
                        v-i18next="TreeAddSection"
                        >Add sub-section</a
                    >
                </li>
                <li>
                    <a
                        @click="addRoster(item)"
                        v-if="
                            !questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly &&
                                !currentChapter.isCover
                        "
                        v-i18next="TreeAddRoster"
                        >Add roster</a
                    >
                </li>
                <li>
                    <a
                        @click="addStaticText(item)"
                        v-if="
                            !questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly
                        "
                        v-i18next="TreeAddStaticText"
                        >Add static text</a
                    >
                </li>
                <li>
                    <a
                        @click="addVariable(item)"
                        v-if="
                            !questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly
                        "
                        v-i18next="TreeAddVariable"
                        >Add variable</a
                    >
                </li>
                <li><a @click="copyRef(item)" v-i18next>Copy</a></li>
                <li>
                    <a
                        v-disabled="!readyToPaste"
                        @click="pasteItemAfter(item)"
                        v-if="
                            !questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly
                        "
                        v-i18next="PasteAfter"
                    ></a>
                </li>
                <li>
                    <a
                        @click="deleteGroup(item)"
                        v-if="
                            !questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly
                        "
                        v-i18next
                        >Delete</a
                    >
                </li>
            </ul>
        </div>
    </div>
    <!--div
        v-hide="collapsed"
        class="slide"
        :class="{ highlighted: is_highlighted, 'roster-items': item.isRoster }"
        ui-tree-nodes="item.items"
        v-model="item.items"
    >
        <div
            v-repeat="item in item.items | filter:searchItem"
            class="filter-animate"
            ui-tree-node
            v-include="itemTemplate(item.itemType)"
        ></div>
    </div-->
</template>

<script>
import { useQuestionnaireStore } from '../../../stores/questionnaire';
import { useTreeStore } from '../../../stores/tree';

export default {
    name: 'TreeGroup',
    props: {
        id: { type: String, required: true },
        item: { type: Object, required: true },
        highlightedId: { type: String, required: true }
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
