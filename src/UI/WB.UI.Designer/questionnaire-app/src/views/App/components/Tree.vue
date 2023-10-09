<template>
    <vue3-tree-vue
        :items="items"
        :isCheckable="false"
        :hideGuideLines="false"
        @onCheck="onItemChecked"
        @dropValidator="onBeforeItemDropped"
        @onSelect="onItemSelected"
        @onExpanded="lazyLoadNode"
    >
        <!-- Applying some simple styling to tree-items -->
        <template v-slot:item-prepend-icon="treeViewItem">
            <img
                alt="folder"
                v-if="treeViewItem.type === 'folder'"
                height="20"
                width="20"
            />
        </template>
    </vue3-tree-vue>

    <div class="questionnaire-tree-holder col-xs-6">
        <div
            class="chapter-title"
            ng-switch
            on="filtersBoxMode"
            ui-sref-active="selected"
            ui-sref="questionnaire.chapter.group({ chapterId: currentChapter.itemId, itemId: currentChapter.itemId})"
        >
            <div
                ng-click="$event.stopPropagation()"
                class="search-box"
                ng-switch-when="search"
            >
                <div class="input-group">
                    <span
                        class="input-group-addon glyphicon glyphicon-search"
                        ng-i18next="[title]Search"
                    ></span>
                    <input
                        type="text"
                        ng-model="search.searchText"
                        ng-model-options="{debounce: 300}"
                        focus-on-out="focusSearch"
                        hotkey="{esc: hideSearch}"
                        hotkey-allow-in="INPUT"
                    />
                    <span
                        class="input-group-addon glyphicon glyphicon-option-horizontal pointer"
                        ng-i18next="[title]FindReplaceTitle"
                        ng-click="showFindReplaceDialog()"
                    ></span>
                </div>
                <button
                    ng-click="hideSearch();$event.stopPropagation()"
                    ng-i18next="[title]Cancel"
                    type="button"
                ></button>
            </div>

            <div ng-switch-when="default" class="chapter-name">
                <a
                    id="group-{{currentChapter.itemId}}"
                    class="chapter-title-text"
                    ui-sref="questionnaire.chapter.group({ chapterId: currentChapter.itemId, itemId: currentChapter.itemId})"
                >
                    <span ng-bind-html="currentChapter.title | escape"></span>
                    <span
                        ng-if="currentChapter.isCover && currentChapter.isReadOnly"
                        class="warning-message"
                        ng-i18next
                        >VirtualCoverPage</span
                    >
                    <help
                        ng-if="currentChapter.isCover && currentChapter.isReadOnly"
                        key="virtualCoverPage"
                    />
                    <a
                        ng-if="!questionnaire.isReadOnlyForUser && currentChapter.isCover && currentChapter.isReadOnly"
                        href="javascript:void(0);"
                        ng-click="migrateToNewVersion();$event.stopPropagation()"
                        ng-i18next
                        >MigrateToNewCover</a
                    >
                </a>
                <div class="qname-block chapter-condition-block">
                    <div class="conditions-block">
                        <div
                            class="enabling-group-marker"
                            ng-class="{'hide-if-disabled': currentChapter.hideIfDisabled}"
                            ng-if="currentChapter.hasCondition"
                        ></div>
                    </div>
                </div>
                <ul class="controls-right">
                    <li>
                        <a
                            href="javascript:void(0);"
                            ng-click="showSearch();$event.stopPropagation()"
                            class="search"
                            ng-i18next="[title]ToggleSearch"
                        ></a>
                    </li>
                </ul>
            </div>
        </div>
        <perfect-scrollbar class="scroller">
            <div
                class="question-list"
                ui-tree="groupsTree"
                data-bs-empty-placeholder-enabled="false"
            >
                <div ui-tree-nodes ng-model="items">
                    <div
                        ng-repeat="item in items | filter:searchItem as results"
                        ui-tree-node
                        data-bs-nodrag="{{ currentChapter.isReadOnly }}"
                    >
                        <ng-include
                            src="itemTemplate(item.itemType)"
                        ></ng-include>
                    </div>
                    <div
                        class="section item"
                        ng-if="filtersBoxMode == 'search' && results.length === 0"
                    >
                        <div class="item-text">
                            <span ng-i18next="NothingFound"></span>
                        </div>
                    </div>
                    <div
                        class="chapter-level-buttons"
                        ng-show="!search.searchText"
                    >
                        <button
                            type="button"
                            class="btn lighter-hover"
                            ng-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                            ng-click="addQuestion(currentChapter)"
                            ng-i18next="AddQuestion"
                        ></button>
                        <button
                            type="button"
                            class="btn lighter-hover"
                            ng-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly && !currentChapter.isCover"
                            ng-click="addGroup(currentChapter)"
                            ng-i18next="AddSubsection"
                        ></button>
                        <button
                            type="button"
                            class="btn lighter-hover"
                            ng-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly && !currentChapter.isCover"
                            ng-click="addRoster(currentChapter)"
                            ng-i18next="AddRoster"
                        ></button>
                        <button
                            type="button"
                            class="btn lighter-hover"
                            ng-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                            ng-click="addStaticText(currentChapter)"
                            ng-i18next="AddStaticText"
                        ></button>
                        <button
                            type="button"
                            class="btn lighter-hover"
                            ng-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                            ng-click="addVariable(currentChapter)"
                            ng-i18next="AddVariable"
                        ></button>

                        <button
                            type="button"
                            class="btn lighter-hover"
                            ng-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                            ng-click="searchForQuestion(currentChapter)"
                            ng-i18next="SearchForQuestion"
                        ></button>

                        <input
                            type="button"
                            class="btn lighter-hover pull-right"
                            ng-disabled="!readyToPaste"
                            ng-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                            ng-i18next="[value]Paste"
                            ng-click="pasteItemInto(currentChapter)"
                        />
                    </div>
                </div>

                <div class="start-box" ng-if="showStartScreen()">
                    <p ng-i18next="EmptySectionLine1"></p>
                    <p>
                        <span ng-bind-html="emptySectionHtmlLine1"> </span>
                        <br />
                        <span
                            ng-i18next="[html]({panel: '&lt;span class=&quot;left-panel-glyph&quot;&gt;&lt;/span&gt;'})EmptySectionLine3"
                        ></span>
                    </p>

                    <p>
                        <span ng-i18next="EmptySectionLine4"></span>
                        <br />
                        <span ng-bind-html="emptySectionHtmlLine2"></span>
                    </p>
                </div>
            </div>
        </perfect-scrollbar>
    </div>

    <div
        class="question-editor col-xs-6"
        ng-class="{ commenting : isCommentsBlockVisible}"
        ui-view
    ></div>

    <div class="comments-editor col-xs-6" ui-view="comments"></div>
</template>

<script>
import 'vue3-tree-vue/dist/style.css';
import { useTreeStore } from '../../../stores/tree';

export default {
    name: 'Tree',
    props: {
        questionnaireId: { type: String, required: true },
        chapterId: { type: String, required: true }
    },
    data() {
        return {
            items: []
        };
    },
    setup() {
        const treeStore = useTreeStore();

        const onItemSelected = item => console.log(item);
        const onBeforeItemDropped = (droppedItem, dropHost) => {
            // dropHost == undefined means dropping at the root of the tree.

            // Here you can specify any kind of drop validation you will like.
            // this function should return true if the drop operation is valid.

            if (dropHost.type !== playlist) return false;
            return true;
        };

        const onExpanded = expandedItem => {
            //fetch data
            //const lazyLoadedItems = fetchFromApi(...);
            //expandedItem.children.push(...lazyLoadedItems)
        };

        return {
            treeStore,
            onItemSelected,
            onBeforeItemDropped
        };
    },
    async beforeMount() {
        await this.fetch();
    },
    methods: {
        async fetch() {
            await this.treeStore.fetchTree(
                this.questionnaireId,
                this.chapterId
            );
        }
    }
};
</script>
