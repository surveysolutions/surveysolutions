<template>
    <div
        class="question item question"
        ui-sref-active="selected"
        :class="{ highlight: item.itemId === highlightedId }"
        context-menu
        :data-target="'context-menu-' + item.itemId"
        context-menu-hide-on-mouse-leave="true"
    >
        <span class="cursor" v-show="!currentChapter.isReadOnly"></span>
        <a
            class="handler"
            ui-tree-handle
            v-show="!currentChapter.isReadOnly"
        ></a>
        <a
            class="item-body"
            :id="item.itemId"
            ui-sref="questionnaire.chapter.question({itemId: item.itemId})"
        >
            <div class="item-text">
                <div class="icon" :class="[answerTypeClass[item.type]]"></div>
                <!--                <span
                    ng-bind-html="item.title | escape | highlight:search.searchText"
                ></span>
-->
                <span v-text="item.title"></span>
            </div>
            <div class="qname-block">
                <div class="conditions-block">
                    <div
                        class="enabling-group-marker"
                        :class="{ 'hide-if-disabled': item.hideIfDisabled }"
                        v-if="item.hasCondition"
                    ></div>
                    <div
                        class="validation-group-marker"
                        v-if="item.hasValidation"
                    ></div>
                </div>
                <!--span
                    ng-bind-html="item.variable | escape | highlight:search.searchText"
                ></span>
-->
                <span v-text="item.variable"></span>
            </div>
        </a>
        <div
            class="dropdown position-fixed"
            :id="'context-menu-' + item.itemId"
        >
            <ul class="dropdown-menu" role="menu">
                <li>
                    <a
                        @click="addQuestionAfter(item)"
                        v-if="
                            !questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly
                        "
                        ng-i18next="TreeAddQuestionAfter"
                    ></a>
                </li>
                <li>
                    <a
                        @click="addRosterAfter(item)"
                        v-if="
                            !questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly &&
                                !currentChapter.isCover
                        "
                        ng-i18next="TreeAddRosterAfter"
                        >Add roster after</a
                    >
                </li>
                <li>
                    <a
                        @click="addGroupAfter(item)"
                        v-if="
                            !questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly &&
                                !currentChapter.isCover
                        "
                        ng-i18next="TreeAddSectionAfter"
                        >Add sub-section after</a
                    >
                </li>
                <li>
                    <a
                        ng-click="addStaticTextAfter(item)"
                        ng-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                        ng-i18next="TreeAddStaticTextAfter"
                        >Add static text after</a
                    >
                </li>
                <li>
                    <a
                        ng-click="addVariableAfter(item)"
                        ng-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                        ng-i18next="TreeAddVariableAfter"
                        >Add variable after</a
                    >
                </li>
                <li>
                    <a ng-click="copyRef(item)" ng-i18next>Copy</a>
                </li>
                <li>
                    <a
                        ng-disabled="!readyToPaste"
                        ng-click="pasteItemAfter(item)"
                        ng-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                        ng-i18next="PasteAfter"
                        >Paste after</a
                    >
                </li>
                <li>
                    <a
                        ng-click="deleteQuestion(item)"
                        ng-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                        ng-i18next
                        >Delete</a
                    >
                </li>
            </ul>
        </div>
    </div>
</template>

<script>
export default {
    name: 'TreeQuestion',
    props: {
        id: { type: String, required: true },
        item: { type: Object, required: true },
        highlightedId: { type: String, required: true }
    },
    data() {
        return {};
    }
};
</script>
