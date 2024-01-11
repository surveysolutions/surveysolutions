<template>
    <TreeItem :item="item">
        <router-link class="item-body" :id="item.itemId" :to="{
            name: 'statictext',
            params: {
                statictextId: item.itemId
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

                <span v-if="item.text" v-text="item.text"></span>
                <span v-if="!item.text && item.attachmentName" v-text="'Attachment: ' + item.attachmentName"></span>
                <span v-if="!item.text && !item.attachmentName" v-text="item.text"></span>
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

    <!--div
        class="question item statictext"
        :class="{ highlight: item.itemId === highlightedId }"
        context-menu
        :data-target="'context-menu-' + item.itemId"
        ui-sref-active="selected"
        context-menu-hide-on-mouse-leave="true"
    >
        <span class="cursor"></span>
        <a class="handler" ui-tree-handle></a>

        <div
            class="dropdown position-fixed"
            :id="'context-menu-' + item.itemId"
        >
            <ul class="dropdown-menu" role="menu">
                <li>
                    <a
                        ng-click="addQuestionAfter(item)"
                        ng-if="!questionnaire.isReadOnlyForUser  && !currentChapter.isReadOnly"
                        ng-i18next="TreeAddQuestionAfter"
                        >Add question after</a
                    >
                </li>
                <li>
                    <a
                        ng-click="addRosterAfter(item)"
                        ng-if="!questionnaire.isReadOnlyForUser  && !currentChapter.isReadOnly && !currentChapter.isCover"
                        ng-i18next="TreeAddRosterAfter"
                        >Add roster after</a
                    >
                </li>
                <li>
                    <a
                        ng-click="addGroupAfter(item)"
                        ng-if="!questionnaire.isReadOnlyForUser  && !currentChapter.isReadOnly && !currentChapter.isCover"
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
                        ng-if="!questionnaire.isReadOnlyForUser  && !currentChapter.isReadOnly"
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
                        ng-i18next
                        >PasteAfter</a
                    >
                </li>
                <li>
                    <a
                        ng-click="deleteStaticText(item.itemId)"
                        ng-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                        ng-i18next
                        >Delete</a
                    >
                </li>
            </ul>
        </div>
    </div-->
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
