<template>
    <div class="left-side-panel chapters" :class="{ unfolded: isFolded }" ng-controller="ChaptersCtrl"
        data-empty-place-holder-enabled="false">
        <div class="foldback-region" @click="foldback(); $event.stopPropagation()"></div>
        <div class="left-side-panel-content chapter-panel" ui-tree="chaptersTree">
            <div class="foldback-button" @click="foldback(); $event.stopPropagation()"></div>
            <div class="ul-holder">
                <Chapters></Chapters>
            </div>
        </div>
    </div>


    <div class="left-side-panel scenarios" :class="{ unfolded: isFolded }" ng-controller="ScenariosCtrl">
        <div class="foldback-region" @click="foldback(); $event.stopPropagation()"></div>
        <div class="left-side-panel-content macros-panel">
            <div class="foldback-button-region" @click="foldback(); $event.stopPropagation()">
                <div class="foldback-button"></div>
            </div>
            <Scenarios></Scenarios>
        </div>
    </div>

    <div class="left-side-panel macroses" :class="{ unfolded: isFolded }" ng-controller="MacrosCtrl">
        <div class="foldback-region" @click="foldback(); $event.stopPropagation()"></div>
        <div class="left-side-panel-content macros-panel">
            <div class="foldback-button-region" @click="foldback(); $event.stopPropagation()">
                <div class="foldback-button"></div>
            </div>
            <Macroses></Macroses>
        </div>
    </div>

    <div class="left-side-panel lookup-tables" :class="{ unfolded: isFolded }" ng-controller="LookupTablesCtrl">
        <div class="foldback-region" @click="foldback(); $event.stopPropagation()"></div>
        <div class="left-side-panel-content lookup-tables-panel">
            <div class="foldback-button-region" @click="foldback(); $event.stopPropagation()">
                <div class="foldback-button"></div>
            </div>
            <LookupTables></LookupTables>
        </div>
    </div>

    <div class="left-side-panel attachments" :class="{ unfolded: isFolded }" ng-controller="AttachmentsCtrl">
        <div class="foldback-region" @click="foldback(); $event.stopPropagation()"></div>
        <div class="left-side-panel-content attachments-panel">
            <div class="foldback-button-region" @click="foldback(); $event.stopPropagation()">
                <div class="foldback-button"></div>
            </div>
            <Attachments></Attachments>
        </div>
    </div>

    <div class="left-side-panel translations" :class="{ unfolded: isFolded }" ng-controller="TranslationsCtrl">
        <div class="foldback-region" @click="foldback(); $event.stopPropagation()"></div>
        <div class="left-side-panel-content translations-panel">
            <div class="foldback-button-region" @click="foldback(); $event.stopPropagation()">
                <div class="foldback-button"></div>
            </div>
            <Translations></Translations>
        </div>
    </div>

    <div class="left-side-panel categories" :class="{ unfolded: isFolded }" ng-controller="CategoriesCtrl">
        <div class="foldback-region" @click="foldback(); $event.stopPropagation()"></div>
        <div class="left-side-panel-content categories-panel">
            <div class="foldback-button-region" @click="foldback(); $event.stopPropagation()">
                <div class="foldback-button"></div>
            </div>
            <Categories></Categories>
        </div>
    </div>

    <div class="left-side-panel metadata" :class="{ unfolded: isFolded }" ng-controller="MetadataCtrl">
        <div class="foldback-region" @click="foldback(); $event.stopPropagation()"></div>
        <div class="left-side-panel-content metadata-panel">
            <div class="foldback-button-region" @click="foldback(); $event.stopPropagation()">
                <div class="foldback-button"></div>
            </div>
            <Metadata></Metadata>
        </div>
    </div>

    <div class="left-side-panel comments" :class="{ unfolded: isFolded }" ng-controller="CommentsCtrl"
        data-empty-place-holder-enabled="false">
        <div class="foldback-region" @click="foldback(); $event.stopPropagation()"></div>
        <div class="left-side-panel-content comments-panel">
            <div class="foldback-button-region" @click="foldback(); $event.stopPropagation()">
                <div class="foldback-button"></div>
            </div>
            <Comments></Comments>
        </div>
    </div>

    <div id="left-menu" ng-controller="LeftMenuCtrl">
        <ul>
            <li>
                <a class="left-menu-chapters" :class="{ unfolded: isUnfoldedChapters }" @click="unfoldChapters();"
                    :title="$t('QuestionnaireEditor.SideBarSectionsTitle')"></a>
            </li>
            <li>
                <a class="left-menu-metadata" :class="{ unfolded: isUnfoldedMetadata }" @click="unfoldMetadata();"
                    :title="$t('QuestionnaireEditor.SideBarMetadataTitle')"></a>
            </li>
            <li>
                <a class="left-menu-translations" :class="{ unfolded: isUnfoldedTranslations }"
                    @click="unfoldTranslations();" :title="$t('QuestionnaireEditor.SideBarTranslationsTitle')"></a>
            </li>
            <li>
                <a class="left-menu-categories" :class="{ unfolded: isUnfoldedCategories }" @click="unfoldCategories();"
                    :title="$t('QuestionnaireEditor.SideBarCategoriesTitle')"></a>
            </li>
            <li>
                <a class="left-menu-scenarios" v-if="questionnaire.questionnaireRevision === null"
                    :class="{ unfolded: isUnfoldedScenarios }" @click="unfoldScenarios();"
                    :title="$t('QuestionnaireEditor.SideBarScenarioTitle')"></a>
            </li>
            <li>
                <a class="left-menu-macroses" :class="{ unfolded: isUnfoldedMacros }" @click="unfoldMacros();"
                    :title="$t('QuestionnaireEditor.SideBarMacroTitle')"></a>
            </li>
            <li>
                <a class="left-menu-lookupTables" :class="{ unfolded: isUnfoldedLookupTables }"
                    @click="unfoldLookupTables();" :title="$t('QuestionnaireEditor.SideBarLookupTitle')"></a>
            </li>
            <li>
                <a class="left-menu-attachments" :class="{ unfolded: isUnfoldedAttachments }" @click="unfoldAttachments();"
                    :title="$t('QuestionnaireEditor.SideBarAttachmentsTitle')"></a>
            </li>
            <li>
                <a class="left-menu-comments" v-if="!questionnaire.isReadOnlyForUser"
                    :class="{ unfolded: isUnfoldedComments }" @click="unfoldComments();"
                    :title="$t('QuestionnaireEditor.SideBarCommentsTitle')"></a>
            </li>
        </ul>
    </div>
</template>

<script>

import Attachments from './leftSidePanel/Attachments.vue';
import Categories from './leftSidePanel/Categories.vue';
import Chapters from './leftSidePanel/Chapters.vue';
import Comments from './leftSidePanel/Comments.vue';
import LookupTables from './leftSidePanel/LookupTables.vue'
import Macroses from './leftSidePanel/Macroses.vue'
import Metadata from './leftSidePanel/Metadata.vue'
import Scenarios from './leftSidePanel/Scenarios.vue'
import Translations from './leftSidePanel/Translations.vue'

export default {
    name: 'LeftSidePanel',
    components: {
        Attachments,
        Categories,
        Chapters,
        Comments,
        LookupTables,
        Macroses,
        Metadata,
        Scenarios,
        Translations
    },
    inject: ['questionnaire', 'currentChapter'],
    props: {},
    data() {
        return {};
    }
};
</script>
