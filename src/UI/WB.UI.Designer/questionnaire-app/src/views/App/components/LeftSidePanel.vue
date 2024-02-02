<template>
    <Transition name="slide">
        <div v-if="isUnfoldedChapters" class="left-side-panel chapters" :class="{ unfolded: isFolded }"
            ng-controller="ChaptersCtrl" data-empty-place-holder-enabled="false">
            <div class="foldback-region" @click.stop="foldback()"></div>
            <div class="left-side-panel-content chapter-panel" ui-tree="chaptersTree">
                <div class="foldback-button" @click.stop="foldback()"></div>
                <div class="ul-holder">
                    <Chapters :questionnaireId="questionnaireId"></Chapters>
                </div>
            </div>
        </div>
    </Transition>
    <Transition name="slide">
        <div v-if="isUnfoldedScenarios" class="left-side-panel scenarios" :class="{ unfolded: isFolded }"
            ng-controller="ScenariosCtrl">
            <div class="foldback-region" @click.stop="foldback()"></div>
            <div class="left-side-panel-content macros-panel">
                <div class="foldback-button-region" @click.stop="foldback()">
                    <div class="foldback-button"></div>
                </div>
                <Scenarios :questionnaireId="questionnaireId"></Scenarios>
            </div>
        </div>
    </Transition>
    <Transition name="slide">
        <div v-if="isUnfoldedMacros" class="left-side-panel macroses" :class="{ unfolded: isFolded }"
            ng-controller="MacrosCtrl">
            <div class="foldback-region" @click.stop="foldback()"></div>
            <div class="left-side-panel-content macros-panel">
                <div class="foldback-button-region" @click.stop="foldback()">
                    <div class="foldback-button"></div>
                </div>
                <Macros></Macros>
            </div>
        </div>
    </Transition>
    <Transition name="slide">
        <div v-if="isUnfoldedLookupTables" class="left-side-panel lookup-tables" :class="{ unfolded: isFolded }"
            ng-controller="LookupTablesCtrl">
            <div class="foldback-region" @click.stop="foldback()"></div>
            <div class="left-side-panel-content lookup-tables-panel">
                <div class="foldback-button-region" @click.stop="foldback()">
                    <div class="foldback-button"></div>
                </div>
                <LookupTables :questionnaireId="questionnaireId"></LookupTables>
            </div>
        </div>
    </Transition>
    <Transition name="slide">
        <div v-if="isUnfoldedAttachments" class="left-side-panel attachments" :class="{ unfolded: isFolded }"
            ng-controller="AttachmentsCtrl">
            <div class="foldback-region" @click.stop="foldback()"></div>
            <div class="left-side-panel-content attachments-panel">
                <div class="foldback-button-region" @click.stop="foldback()">
                    <div class="foldback-button"></div>
                </div>
                <Attachments :questionnaireId="questionnaireId"></Attachments>
            </div>
        </div>
    </Transition>
    <Transition name="slide">
        <div v-if="isUnfoldedTranslations" class="left-side-panel translations" :class="{ unfolded: isFolded }"
            ng-controller="TranslationsCtrl">
            <div class="foldback-region" @click.stop="foldback()"></div>
            <div class="left-side-panel-content translations-panel">
                <div class="foldback-button-region" @click.stop="foldback()">
                    <div class="foldback-button"></div>
                </div>
                <Translations :questionnaireId="questionnaireId"></Translations>
            </div>
        </div>
    </Transition>
    <Transition name="slide">
        <div v-if="isUnfoldedCategories" class="left-side-panel categories" :class="{ unfolded: isFolded }"
            ng-controller="CategoriesCtrl">
            <div class="foldback-region" @click.stop="foldback()"></div>
            <div class="left-side-panel-content categories-panel">
                <div class="foldback-button-region" @click.stop="foldback()">
                    <div class="foldback-button"></div>
                </div>
                <Categories :questionnaireId="questionnaireId"></Categories>
            </div>
        </div>
    </Transition>
    <Transition name="slide">
        <div v-if="isUnfoldedMetadata" class="left-side-panel metadata" :class="{ unfolded: isFolded }"
            ng-controller="MetadataCtrl">
            <div class="foldback-region" @click.stop="foldback()"></div>
            <div class="left-side-panel-content metadata-panel">
                <div class="foldback-button-region" @click.stop="foldback()">
                    <div class="foldback-button"></div>
                </div>
                <Metadata :questionnaireId="questionnaireId"></Metadata>
            </div>
        </div>
    </Transition>
    <Transition name="slide">
        <div v-if="isUnfoldedComments" class="left-side-panel comments" :class="{ unfolded: isFolded }"
            ng-controller="CommentsCtrl" data-empty-place-holder-enabled="false">
            <div class="foldback-region" @click.stop="foldback()"></div>
            <div class="left-side-panel-content comments-panel">
                <div class="foldback-button-region" @click.stop="foldback()">
                    <div class="foldback-button"></div>
                </div>
                <Comments :questionnaireId="questionnaireId"></Comments>
            </div>
        </div>
    </Transition>
    <div id="left-menu" ng-controller="LeftMenuCtrl">
        <ul>
            <li>
                <a class="left-menu-chapters" :class="{ unfolded: isUnfoldedChapters }" @click="openPanel = 'chapters';"
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
<style scoped>
.slide-enter-from {
    transform: translateX(-500px);
}

.slide-enter-active {
    transition: all .2s ease-in;
    z-index: 400;
}

.slide-leave-active {
    transition: all .2s ease-in;
    z-index: 300;
}

.slide-leave-to {
    transform: translateX(-500px);
}
</style>
<script>

import Attachments from './leftSidePanel/Attachments.vue';
import Categories from './leftSidePanel/Categories.vue';
import Chapters from './leftSidePanel/Chapters.vue';
import Comments from './leftSidePanel/Comments.vue';
import LookupTables from './leftSidePanel/LookupTables.vue'
import Macros from './leftSidePanel/Macros.vue'
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
        Macros,
        Metadata,
        Scenarios,
        Translations
    },
    inject: ['questionnaire', 'currentChapter'],
    props: {
        questionnaireId: { type: String, required: true },
    },
    data() {
        return {
            openPanel: null,//'categories',
        };
    },
    mounted() {
        this.$emitter.on('openChaptersList', this.setChaptersPanel);
        this.$emitter.on('openCategoriesList', this.setCategoriesPanel);
        this.$emitter.on('openLookupTables', this.setLookupTablesPanel);
        this.$emitter.on('openAttachments', this.setAttachmentsPanel);
        this.$emitter.on('openTranslations', this.setTranslationsPanel);
        this.$emitter.on('openMetadata', this.setMetadataPanel);
        this.$emitter.on('openComments', this.setCommentsPanel);
        this.$emitter.on('openScenariosList', this.setScenariosPanel);
        this.$emitter.on('openMacrosList', this.setMacrosesPanel);

        this.$emitter.on('closeCategories', this.closeAllPanel);
        this.$emitter.on('closeChaptersList', this.closeAllPanel);
        this.$emitter.on('closeScenariosList', this.closeAllPanel);
        this.$emitter.on('closeMacrosList', this.closeAllPanel);
        this.$emitter.on('closeLookupTables', this.closeAllPanel);
        this.$emitter.on('closeAttachments', this.closeAllPanel);
        this.$emitter.on('closeTranslations', this.closeAllPanel);
        this.$emitter.on('closeMetadata', this.closeAllPanel);
        this.$emitter.on('closeComments', this.closeAllPanel);

        this.$emitter.on('verifing', this.closeOpenPanelIfAny);
    },
    unmounted() {
        this.$emitter.off('openChaptersList', this.setChaptersPanel);
        this.$emitter.off('openCategoriesList', this.setCategoriesPanel);
        this.$emitter.off('openLookupTables', this.setLookupTablesPanel);
        this.$emitter.off('openAttachments', this.setAttachmentsPanel);
        this.$emitter.off('openTranslations', this.setTranslationsPanel);
        this.$emitter.off('openMetadata', this.setMetadataPanel);
        this.$emitter.off('openComments', this.setCommentsPanel);
        this.$emitter.off('openScenariosList', this.setScenariosPanel);
        this.$emitter.off('openMacrosList', this.setMacrosesPanel);

        this.$emitter.off('closeCategories', this.closeAllPanel);
        this.$emitter.off('closeChaptersList', this.closeAllPanel);
        this.$emitter.off('closeScenariosList', this.closeAllPanel);
        this.$emitter.off('closeMacrosList', this.closeAllPanel);
        this.$emitter.off('closeLookupTables', this.closeAllPanel);
        this.$emitter.off('closeAttachments', this.closeAllPanel);
        this.$emitter.off('closeTranslations', this.closeAllPanel);
        this.$emitter.off('closeMetadata', this.closeAllPanel);
        this.$emitter.off('closeComments', this.closeAllPanel);

        this.$emitter.off('verifing', this.closeOpenPanelIfAny);
    },
    computed: {
        isFolded() {
            return this.openPanel != null;
        },
        isUnfoldedScenarios() { return this.openPanel == 'scenarios' },
        isUnfoldedMacros() { return this.openPanel == 'macroses' },
        isUnfoldedChapters() { return this.openPanel == 'chapters' },
        isUnfoldedLookupTables() { return this.openPanel == 'lookup-tables' },
        isUnfoldedAttachments() { return this.openPanel == 'attachments' },
        isUnfoldedTranslations() { return this.openPanel == 'translations' },
        isUnfoldedMetadata() { return this.openPanel == 'metadata' },
        isUnfoldedComments() { return this.openPanel == 'comments' },
        isUnfoldedCategories() { return this.openPanel == 'categories' },
    },
    methods: {
        foldback() {
            this.openPanel = null;
        },

        closeOpenPanelIfAny() {
            if (this.openPanel != null)
                return;

            if (this.isUnfoldedChapters) {
                this.$emitter.emit("closeChaptersListRequested", {});
            }
            if (this.isUnfoldedScenarios) {
                this.$emitter.emit("closeScenariosListRequested", {});
            }
            if (this.isUnfoldedMacros) {
                this.$emitter.emit("closeMacrosListRequested", {});
            }
            if (this.isUnfoldedLookupTables) {
                this.$emitter.emit("closeLookupTablesRequested", {});
            }
            if (this.isUnfoldedAttachments) {
                this.$emitter.emit("closeAttachmentsRequested", {});
            }
            if (this.isUnfoldedTranslations) {
                this.$emitter.emit("closeTranslationsRequested", {});
            }
            if (this.isUnfoldedMetadata) {
                this.$emitter.emit("closeMetadataRequested", {});
            }
            if (this.isUnfoldedComments) {
                this.$emitter.emit("closeCommentsRequested", {});
            }
            if (this.isUnfoldedCategories) {
                this.$emitter.emit("closeCategoriesRequested", {});
            }
        },

        closeAllPanel() {
            this.openPanel = null;
        },

        unfoldChapters() {
            if (this.isUnfoldedChapters)
                return;
            this.closeOpenPanelIfAny();
            this.setChaptersPanel();
            this.$emitter.emit("openChaptersList", {});
        },
        setChaptersPanel() { this.openPanel = 'chapters'; },

        unfoldCategories() {
            if (this.isUnfoldedCategories)
                return;

            this.closeOpenPanelIfAny();
            this.setCategoriesPanel();
            this.$emitter.emit("openCategories", {});
        },
        setCategoriesPanel() { this.openPanel = 'categories'; },

        unfoldMacros() {
            if (this.isUnfoldedMacros)
                return;

            this.closeOpenPanelIfAny();
            this.setMacrosesPanel();
            this.$emitter.emit("openMacrosList", {});
        },
        setMacrosesPanel() { this.openPanel = 'macroses'; },

        unfoldScenarios() {
            if (this.isUnfoldedScenarios)
                return;

            this.closeOpenPanelIfAny();
            this.setScenariosPanel();
            this.$emitter.emit("openScenariosList", {});
        },
        setScenariosPanel() { this.openPanel = 'scenarios'; },

        unfoldLookupTables() {
            if (this.isUnfoldedLookupTables)
                return;

            this.closeOpenPanelIfAny();
            this.setLookupTablesPanel();
            this.$emitter.emit("openLookupTables", {});
        },
        setLookupTablesPanel() { this.openPanel = 'lookup-tables'; },

        unfoldAttachments() {
            if (this.isUnfoldedAttachments)
                return;

            this.closeOpenPanelIfAny();
            this.setAttachmentsPanel();
            this.$emitter.emit("openAttachments", {});
        },
        setAttachmentsPanel() { this.openPanel = 'attachments'; },

        unfoldTranslations() {
            if (this.isUnfoldedTranslations)
                return;

            this.closeOpenPanelIfAny();
            this.setTranslationsPanel();
            this.$emitter.emit("openTranslations", {});
        },
        setTranslationsPanel() { this.openPanel = 'translations'; },

        unfoldMetadata() {
            if (this.isUnfoldedMetadata)
                return;

            this.closeOpenPanelIfAny();
            this.setMetadataPanel();
            this.$emitter.emit("openMetadata", {});
        },
        setMetadataPanel() { this.openPanel = 'metadata'; },

        unfoldComments() {
            if (this.isUnfoldedComments)
                return;

            this.closeOpenPanelIfAny();
            this.setCommentsPanel();
            this.$emitter.emit("openComments", {});
        },
        setCommentsPanel() { this.openPanel = 'comments'; },
    }
};
</script>
