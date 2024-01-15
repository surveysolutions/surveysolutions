<template>
    Chapters
    <!--div class="chapters">
        <perfect-scrollbar class="scroller">
            <h3>
                <span>{{ $t('QuestionnaireEditor.SideBarSectionsCounter', {
                    count: questionnaire.chapters.length
                }) }}</span>
            </h3>
            <ul ui-tree-nodes ng-model="questionnaire.chapters" class="chapters-list">
                <li class="chapter-panel-item" v-for="chapter in questionnaire.chapters" ui-tree-node
                    data-nodrag="{{ chapter.isCover }}" :class="{ current: isCurrentChapter(chapter) }" context-menu
                    data-target="chapter-context-menu-{{ chapter.itemId }}" context-menu-hide-on-mouse-leave="true">
                    <div class="holder" @click="editChapter(chapter); $event.stopPropagation();">
                        <div class="inner">
                            <a class="handler" ui-tree-handle
                                v-if="!questionnaire.isReadOnlyForUser && !chapter.isCover"><span></span></a>
                            <a class="chapter-panel-item-body"
                                ui-sref="questionnaire.chapter.group({ chapterId: chapter.itemId, itemId: chapter.itemId})">
                                <span v-bind-html="chapter.title | escape"></span>
                                <help link="coverPage" v-if="chapter.isCover" />
                            </a>
                            <div class="qname-block chapter-panel-item-condition">
                                <div class="conditions-block">
                                    <div class="enabling-group-marker"
                                        :class="{ 'hide-if-disabled': chapter.hideIfDisabled }" v-if="chapter.hasCondition">
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="dropdown position-fixed" id="chapter-context-menu-{{ chapter.itemId }}">
                        <ul class="dropdown-menu" role="menu">
                            <li><a @click="editChapter(chapter);">{{ $t('QuestionnaireEditor.Open') }}</a></li>
                            <li><a @click="copyRef(chapter);">{{ $t('QuestionnaireEditor.Copy') }}</a></li>
                            <li>
                                <a :disabled="!readyToPaste" @click="pasteAfterChapter(chapter); $event.stopPropagation();"
                                    v-if="!questionnaire.isReadOnlyForUser && !chapter.isReadOnly">{{
                                        $t('QuestionnaireEditor.PasteAfter') }}</a>
                            </li>
                            <li><a @click="deleteChapter(chapter); $event.stopPropagation();"
                                    v-if="!questionnaire.isReadOnlyForUser && !chapter.isCover">{{
                                        $t('QuestionnaireEditor.Delete') }}</a></li>
                        </ul>
                    </div>
                </li>
            </ul>
            <div class="button-holder">
                <button type="button" class="btn lighter-hover" value="ADD NEW SECTION" @click="addNewChapter()"
                    v-if="!questionnaire.isReadOnlyForUser">{{ $t('QuestionnaireEditor.AddNewSection')
                    }}</button>
            </div>
        </perfect-scrollbar>
    </div-->
</template>
  
<script>
export default {
    name: 'Chapters',
    props: {},
    data() {
        return {

        }
    }
}
</script>
  