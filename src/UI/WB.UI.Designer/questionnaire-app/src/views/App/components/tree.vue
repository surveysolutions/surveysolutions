<template>
  <div class="questionnaire-tree-holder col-xs-6">
    <div class="chapter-title" v-switch on="filtersBoxMode" ui-sref-active="selected"
      ui-sref="questionnaire.chapter.group({ chapterId: currentChapter.itemId, itemId: currentChapter.itemId})">
      <div v-click="$event.stopPropagation()" class="search-box" v-switch-when="search">
        <div class="input-group">
          <span class="input-group-addon glyphicon glyphicon-search" v-i18next="[title]Search"></span>
          <input type="text" v-model="search.searchText" v-model-options="{ debounce: 300 }" focus-on-out="focusSearch"
            hotkey="{esc: hideSearch}" hotkey-allow-in="INPUT" />
          <span class="input-group-addon glyphicon glyphicon-option-horizontal pointer"
            v-i18next="[title]FindReplaceTitle" v-click="showFindReplaceDialog()"></span>
        </div>
        <button v-click="
          $event => {
            hideSearch();
            $event.stopPropagation();
          }" v-i18next="[title]Cancel" type="button"></button>
      </div>

      <div v-switch-when="default" class="chapter-name">
        <a id="group-{{currentChapter.itemId}}" class="chapter-title-text"
          ui-sref="questionnaire.chapter.group({ chapterId: currentChapter.itemId, itemId: currentChapter.itemId})">
          <span v-bind-html="currentChapter.title | escape"></span>
          <span v-if="currentChapter.isCover && currentChapter.isReadOnly" class="warniv-message"
            v-i18next>VirtualCoverPage</span>
          <help v-if="currentChapter.isCover && currentChapter.isReadOnly" key="virtualCoverPage" />
          <a v-if="!questionnaire.isReadOnlyForUser && currentChapter.isCover && currentChapter.isReadOnly"
            href="javascript:void(0);" v-click="$event => {
              migrateToNewVersion(); $event.stopPropagation();
            }" v-i18next>MigrateToNewCover</a>
        </a>
        <div class="qname-block chapter-condition-block">
          <div class="conditions-block">
            <div class="enabliv-group-marker" v-class="{ 'hide-if-disabled': currentChapter.hideIfDisabled }"
              v-if="currentChapter.hasCondition"></div>
          </div>
        </div>
        <ul class="controls-right">
          <li>
            <a href="javascript:void(0);" v-click="showSearch(); $event.stopPropagation()" class="search"
              v-i18next="[title]ToggleSearch"></a>
          </li>
        </ul>
      </div>
    </div>
    <perfect-scrollbar class="scroller">
      <div class="question-list" ui-tree="groupsTree" data-empty-placeholder-enabled="false">
        <div ui-tree-nodes v-model="items">
          <div v-repeat="item in items | filter: searchItem as results" ui-tree-node
            data-nodrag="{{ currentChapter.isReadOnly }}">
            <v-include src="itemTemplate(item.itemType)"></v-include>
          </div>
          <div class="section item" v-if="filtersBoxMode == 'search' && results.length === 0">
            <div class="item-text">
              <span v-i18next="NothingFound"></span>
            </div>
          </div>
          <div class="chapter-level-buttons" v-show="!search.searchText">
            <button type="button" class="btn lighter-hover"
              v-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly" v-click="addQuestion(currentChapter)"
              v-i18next="AddQuestion"></button>
            <button type="button" class="btn lighter-hover"
              v-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly && !currentChapter.isCover"
              v-click="addGroup(currentChapter)" v-i18next="AddSubsection"></button>
            <button type="button" class="btn lighter-hover"
              v-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly && !currentChapter.isCover"
              v-click="addRoster(currentChapter)" v-i18next="AddRoster"></button>
            <button type="button" class="btn lighter-hover"
              v-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
              v-click="addStaticText(currentChapter)" v-i18next="AddStaticText"></button>
            <button type="button" class="btn lighter-hover"
              v-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly" v-click="addVariable(currentChapter)"
              v-i18next="AddVariable"></button>

            <button type="button" class="btn lighter-hover"
              v-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
              v-click="searchForQuestion(currentChapter)" v-i18next="SearchForQuestion"></button>

            <input type="button" class="btn lighter-hover pull-right" v-disabled="!readyToPaste"
              v-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly" v-i18next="[value]Paste"
              v-click="pasteItemInto(currentChapter)" />
          </div>
        </div>

        <div class="start-box" v-if="showStartScreen()">
          <p v-i18next="EmptySectionLine1"></p>
          <p>
            <span v-bind-html="emptySectionHtmlLine1">
            </span>
            <br />
            <span
              v-i18next="[html]({ panel: '&lt;span class=&quot;left-panel-glyph&q uot;&gt;&lt;/span&gt;'})EmptySectionLine3"></span>
          </p>

          <p>
            <span v-i18next="EmptySectionLine4"></span>
            <br />
            <span v-bind-html="emptySectionHtmlLine2"></span>
          </p>7b7
        </div>
      </div>
    </perfect-scrollbar>
  </div>

<div class="question-editor col-xs-6" v-class="{ commenting : isCommentsBlockVisible}" ui-view>
</div>

<div class="comments-editor col-xs-6" ui-view="comments">

</div></template>
<script>
export default {
  name: 'Tree',
  props: {},
  data() {
    return {
      items =[],
    }
  }
}
</script>
