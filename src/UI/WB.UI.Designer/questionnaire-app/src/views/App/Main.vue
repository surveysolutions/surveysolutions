<template>
  <section id="header" class="row">
    <a class="designer-logo" href="../../"></a>
    <div class="header-line">
        <div class="header-menu">
            <div class="buttons">
                <a class="btn" href="http://support.mysurvey.solutions/designer" target="_blank" v-i18next>Help</a>
                <a class="btn" href="https://forum.mysurvey.solutions" target="_blank" v-i18next>Forum</a>
            
                <a class="btn" href="../../questionnaire/questionnairehistory/{{questionnaire.questionnaireId}}" target="_blank"
                   v-if="questionnaire.hasViewerAdminRights || questionnaire.isSharedWithUser" v-i18next>History</a>
                <button class="btn" type="button" v-click="showDownloadPdf()" v-i18next>DownloadPdf</button>
                <a class="btn" type="button" v-show="questionnaire.hasViewerAdminRights" v-click="exportQuestionnaire()" v-i18next>SaveAs</a>

                <a class="btn"  v-if="questionnaire.questionnaireRevision || questionnaire.isReadOnlyForUser" href="../../questionnaire/clone/{{questionnaire.questionnaireId}}{{questionnaire.questionnaireRevision ? '$' + questionnaire.questionnaireRevision : ''}}" target="_blank" v-i18next>CopyTo</a>
                <button class="btn" type="button" v-disabled="questionnaire.isReadOnlyForUser && !questionnaire.hasViewerAdminRights && !questionnaire.isSharedWithUser" v-click="showShareInfo()" v-i18next>Settings</button>

                <div class="btn-group" v-if="currentUserIsAuthenticated">
                    <a class="btn btn-default" v-i18next="[i18next]({currentUserName: '{{ currentUserName }}' })HellowMessageBtn"></a>
                    <a class="btn btn-default dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                        <span class="caret"></span>
                        <span class="sr-only" v-i18next>ToggleDropdown</span>
                    </a>
                    <ul class="dropdown-menu dropdown-menu-right">
                        <li><a href="../../identity/account/manage" v-i18next>ManageAccount</a></li>
                        <li><a href="../../identity/account/logout" v-i18next>LogOut</a></li>
                    </ul>
                </div>
                <a class="btn" href="/" type="button" v-if="!currentUserIsAuthenticated" v-i18next>Login</a>
                <a class="btn" href="/identity/account/register" type="button" v-if="!currentUserIsAuthenticated" v-i18next>Register</a>
            </div>
        </div>
        <div class="questionnarie-title">
            <div class="title">
                <div class="questionnarie-title-text">
                    {{questionnaire.title}}
                </div>
                <div class="questionnarie-title-buttons">
                    <span class="text-muted" v-i18next="[i18next]({questionsCount: '{{questionnaire.questionsCount}}',
                                                                    groupsCount: '{{questionnaire.groupsCount}}',
                                                                    rostersCount: '{{questionnaire.rostersCount}}' })QuestionnaireSummary">

                    </span>
                    <input id="verification-btn" type="button" class="btn" v-i18next="[value]Compile" value="COMPILE" v-click="verify()" v-if="questionnaire.questionnaireRevision === null" />
                    <span v-show="verificationStatus.warnings!=null && verificationStatus.errors!=null">
                        <span data-toggle="modal" v-show="(verificationStatus.warnings.length + verificationStatus.errors.length) > 0" class="error-message v-hide" v-class="{'no-errors': verificationStatus.errors.length == 0}">
                            <a href="javascript:void(0);"
                               v-click="showVerificationErrors()"
                               v-i18next="[i18next]({count: verificationStatus.errors.length})ErrorsCounter">
                            </a>
                        </span>
                        <span data-toggle="modal" v-show="verificationStatus.warnings.length > 0" class="warniv-message v-hide">
                            <a href="javascript:void(0);"
                               v-click="showVerificationWarnings()"
                               v-i18next="[i18next]({count: verificationStatus.warnings.length})WarningsCounter">
                            </a>
                        </span>
                        <span class="text-success" v-show="(verificationStatus.warnings.length + verificationStatus.errors.length) === 0" v-i18next>Ok</span>

                        <span class="text-success"><em v-i18next="[i18next]({dateTime: verificationStatus.time})SavedAtTimestamp"></em></span>
                    </span>
                    <span class="error-message strong" v-show="questionnaire.isReadOnlyForUser" v-i18next>ReadOnly</span>
                    <input id="webtest-btn" type="button" class="btn" v-if="questionnaire.webTestAvailable && questionnaire.questionnaireRevision === null" v-i18next="[value]Test" value="Test" v-click="webTest()" />
                    <span class="error-message strong" v-show="questionnaire.previewRevision !== null && questionnaire.previewRevision !== undefined" v-i18next="({revision: questionnaire.previewRevision })Preview"></span>
                </div>
            </div>
        </div>
    </div>
  </section>

  <section id="spacer" class="row">
    <div class="left"></div>
    <div class="right"></div>
  </section>

  <section id="main" class="row">
    <div class="left-side-panel chapters" v-class="{unfolded: isFolded }" v-controller="ChaptersCtrl" data-empty-place-holder-enabled="false">
        <div class="foldback-region" v-click="foldback();$event.stopPropagation()"></div>
        <div class="left-side-panel-content chapter-panel" ui-tree="chaptersTree">
            <div class="foldback-button" v-click="foldback();$event.stopPropagation()"></div>
            <div class="ul-holder">
                <div class="chapters">
                    <perfect-scrollbar class="scroller">
                        <h3>
                            <span v-i18next="[i18next]({count: questionnaire.chapters.length})SideBarSectionsCounter"></span>
                        </h3>
                        <ul ui-tree-nodes v-model="questionnaire.chapters" class="chapters-list">
                            <li class="chapter-panel-item"
                                v-repeat="chapter in questionnaire.chapters"
                                ui-tree-node
                                data-nodrag="{{ chapter.isCover }}"
                                v-class="{ current: isCurrentChapter(chapter) }"
                                context-menu data-target="chapter-context-menu-{{ chapter.itemId }}"
                                context-menu-hide-on-mouse-leave="true">
                                <div class="holder" v-click="editChapter(chapter);$event.stopPropagation();">
                                    <div class="inner">
                                        <a class="handler" ui-tree-handle v-if="!questionnaire.isReadOnlyForUser && !chapter.isCover"><span></span></a>
                                        <a class="chapter-panel-item-body" ui-sref="questionnaire.chapter.group({ chapterId: chapter.itemId, itemId: chapter.itemId})">
                                            <span v-bind-html="chapter.title | escape"></span>
                                            <help key="coverPage" v-if="chapter.isCover" />                                            
                                        </a>
                                        <div class="qname-block chapter-panel-item-condition">
                                            <div class="conditions-block">
                                                <div class="enabliv-group-marker" v-class="{'hide-if-disabled': chapter.hideIfDisabled}" v-if="chapter.hasCondition"></div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="dropdown position-fixed" id="chapter-context-menu-{{ chapter.itemId }}">
                                    <ul class="dropdown-menu" role="menu">
                                        <li><a v-click="editChapter(chapter);" v-i18next>Open</a></li>
                                        <li><a v-click="copyRef(chapter);" v-i18next>Copy</a></li>
                                        <li>
                                            <a v-disabled="!readyToPaste" v-click="pasteAfterChapter(chapter);$event.stopPropagation();" v-if="!questionnaire.isReadOnlyForUser && !chapter.isReadOnly" v-i18next>PasteAfter</a>
                                        </li>
                                        <li><a v-click="deleteChapter(chapter);$event.stopPropagation();" v-if="!questionnaire.isReadOnlyForUser && !chapter.isCover" v-i18next>Delete</a></li>
                                    </ul>
                                </div>
                            </li>
                        </ul>
                        <div class="button-holder">
                            <input type="button" class="btn lighter-hover" v-i18next="[value]AddNewSection" value="ADD NEW SECTION" v-click="addNewChapter()" v-if="!questionnaire.isReadOnlyForUser">
                        </div>
                    </perfect-scrollbar>
                </div>
            </div>
        </div>
    </div>


    <div class="left-side-panel scenarios" v-class="{unfolded: isFolded }" v-controller="ScenariosCtrl">
        <div class="foldback-region" v-click="foldback();$event.stopPropagation()"></div>
        <div class="left-side-panel-content macros-panel">
            <div class="foldback-button-region" v-click="foldback();$event.stopPropagation()">
                <div class="foldback-button"></div>
            </div>
            <div class="macroses">
                <perfect-scrollbar class="scroller">
                    <h3>
                        <span v-i18next="[i18next]({count: scenarios.length})SideBarScenarioCounter"></span>
                    </h3>
                    <div class="empty-list" v-show="scenarios.length == 0">
                        <p v-i18next>SideBarScenarioEmptyLine1</p>
                        <p v-i18next>SideBarScenarioEmptyLine2</p>
                    </div>
                    <form role="form" name="scenariosForm" novalidate>
                        <ul v-model="scenarios">
                            <li class="macroses-panel-item" v-repeat="scenario in scenarios">
                                <v-form name="scenario.form">
                                    <a href="javascript:void(0)" v-if="!questionnaire.isReadOnlyForUser" v-click="deleteScenario($index)" class="btn delete-btn" tabindex="-1"></a>
                                    <div class="input-group macroses-name">
                                        <input focus-on-out="focusScenario{{scenario.id}}" v-i18next="[placeholder]SideBarScenarioName" maxlength="32" spellcheck="false" v-model="scenario.title" name="name" class="form-control" type="text" />
                                    </div>
                                    <div class="divider"></div>
                                    <button type="button" class="btn lighter-hover" v-click="runScenario(scenario)" v-i18next>Run</button>
                                    <button type="button" class="btn lighter-hover" v-click="showScenarioEditor(scenario.id, scenario.title)" v-i18next>View</button>
                                    <div class="actions" v-show="scenario.form.$dirty" v-if="!questionnaire.isReadOnlyForUser">
                                        <button type="submit" v-disabled="questionnaire.isReadOnlyForUser || scenario.form.$invalid" class="btn lighter-hover" v-click="saveScenario(scenario)" v-i18next>Save</button>
                                        <button type="button" class="btn lighter-hover" v-click="cancel(scenario)" v-i18next>Cancel</button>
                                    </div>
                                </v-form>
                            </li>
                        </ul>
                    </form>
                </perfect-scrollbar>
            </div>
        </div>
    </div>


    <div class="left-side-panel macroses" v-class="{unfolded: isFolded }" v-controller="MacrosCtrl">
        <div class="foldback-region" v-click="foldback();$event.stopPropagation()"></div>
        <div class="left-side-panel-content macros-panel">
            <div class="foldback-button-region" v-click="foldback();$event.stopPropagation()">
                <div class="foldback-button"></div>
            </div>
            <div class="macroses">
                <perfect-scrollbar class="scroller">
                    <h3>
                        <span v-i18next="[i18next]({count: macros.length})SideBarMacroCounter"></span>
                    </h3>
                    <div class="empty-list" v-show="macros.length == 0">
                        <p v-i18next>SideBarMacroEmptyLine1</p>
                        <p v-i18next>SideBarMacroEmptyLine2</p>
                        <p>
                            <span class="variable-name" v-i18next="VariableName"></span>
                            {{'SideBarMacroEmptyLine3' | i18next}}
                        </p>
                    </div>
                    <form role="form" name="macrosForm" novalidate>
                        <ul v-model="macros">
                            <li class="macros-panel-item" v-repeat="macro in macros">
                                <v-form name="macro.form">
                                    <a href v-click="deleteMacro($index)" v-disabled="questionnaire.isReadOnlyForUser" v-if="!questionnaire.isReadOnlyForUser" class="btn delete-btn" tabindex="-1"></a>
                                    <div class="input-group macros-name">
                                        <span class="input-group-addon">$</span>
                                        <input focus-on-out="focusMacro{{macro.itemId}}" v-i18next="[placeholder]SideBarMacroName" maxlength="32" spellcheck="false" v-model="macro.name" name="name" class="form-control" type="text" />
                                    </div>
                                    <div class="divider"></div>
                                    <div v-model="macro.content" ui-ace="{ onLoad : aceLoaded, require: ['ace/ext/language_tools'] }" type="text"></div>
                                    <div v-show="macro.isDescriptionVisible">
                                        <div class="divider"></div>
                                        <textarea v-i18next="[placeholder]SideBarMacroDescription" type="text" v-model="macro.description" class="form-control macros-description" msd-elastic></textarea></div>
                                    <div class="actions" v-show="macro.form.$dirty">
                                        <button type="submit" v-disabled="questionnaire.isReadOnlyForUser || macro.form.$invalid" class="btn lighter-hover" v-click="saveMacro(macro)" v-i18next>Save</button>
                                        <button type="button" class="btn lighter-hover" v-click="cancel(macro)" v-i18next>Cancel</button>
                                        <button class="btn btn-default pull-right" v-show="isDescriptionEmpty(macro)" type="button" v-click="toggleDescription(macro)"
                                                v-i18next="{{macro.isDescriptionVisible ? 'SideBarMacroHideDescription' : 'SideBarMacroShowDescription'}}"></button>
                                    </div>
                                </v-form>
                            </li>
                        </ul>

                    </form>
                    <div class="button-holder">
                        <input type="button" class="btn lighter-hover" v-disabled="questionnaire.isReadOnlyForUser" v-i18next="[value]SideBarAddMacro" value="ADD NEW Macro" v-click="addNewMacro()">
                    </div>
                </perfect-scrollbar>
            </div>
        </div>
    </div>
    <div class="left-side-panel lookup-tables" v-class="{unfolded: isFolded }" v-controller="LookupTablesCtrl">
        <div class="foldback-region" v-click="foldback();$event.stopPropagation()"></div>
        <div class="left-side-panel-content lookup-tables-panel">
            <div class="foldback-button-region" v-click="foldback();$event.stopPropagation()">
                <div class="foldback-button"></div>
            </div>
            <div class="lookup-tables">
                <perfect-scrollbar class="scroller">
                    <h3>
                        <span v-i18next="[i18next]({count: lookupTables.length})SideBarLookupTablesCounter"></span>
                    </h3>
                    <div class="empty-list" v-show="lookupTables.length === 0">
                        <p v-i18next>SideBarLookupEmptyLine1</p>
                        <p v-i18next>SideBarLookupEmptyLine2</p>
                        <p v-i18next>SideBarLookupEmptyLine3</p>
                    </div>
                    <form role="form" name="lookupTablesForm" novalidate>
                        <ul v-model="lookupTables">
                            <li class="lookup-table-panel-item" v-repeat="table in lookupTables" ngf-drop="" ngf-change="fileSelected(table, $file)" ngf-max-size="1MB" accept=".tab,.txt" ngf-drag-over-class="{accept:'dragover', reject:'dragover-err'}">
                                <v-form name="table.form">
                                    <a href="javascript:void(0);" v-click="deleteLookupTable($index)" v-disabled="questionnaire.isReadOnlyForUser" v-if="!questionnaire.isReadOnlyForUser" class="btn delete-btn" tabindex="-1"></a>
                                    <input focus-on-out="focusLookupTable{{table.itemId}}" required="" v-i18next="[placeholder]SideBarLookupTableName"
                                           maxlength="32" spellcheck="false" autocomplete="off"
                                           v-model="table.name" name="name" class="form-control table-name" type="text" />
                                    <div class="divider"></div>
                                    <input v-i18next="[placeholder]SideBarLookupTableFileName" required="" spellcheck="false" v-model="table.fileName" name="fileName" class="form-control" disabled="" type="text" />

                                    <div class="drop-box" v-i18next>SideBarLookupTableDropFile</div>

                                    <div class="actions clearfix" v-class="{dirty: table.form.$dirty }">
                                        <div v-show="table.form.$dirty" class="pull-left">
                                            <button type="submit" v-disabled="questionnaire.isReadOnlyForUser || table.form.$invalid" class="btn lighter-hover" v-click="saveLookupTable(table);$event.stopPropagation()" v-i18next>Save</button>
                                            <button type="button" class="btn lighter-hover" v-click="cancel(table);$event.stopPropagation()" v-i18next>Cancel</button>
                                        </div>
                                        <div class="permanent-actions clearfix">
                                            <a href="{{downloadLookupFileBaseUrl + questionnaire.questionnaireId +'?lookupTableId='+ table.itemId}}" v-show="table.hasUploadedFile" class="btn btn-default pull-right" target="_blank" v-i18next>Download</a>
                                            <button class="btn btn-default pull-right" ngf-select="" ngf-change="fileSelected(table, $file);$event.stopPropagation()" accept=".tab,.txt" ngf-max-size="2MB" type="file">
                                                <span v-hide="table.hasUploadedFile" v-i18next="SideBarLookupTableSelectFile">Select file</span>
                                                <span v-show="table.hasUploadedFile" v-i18next="SideBarLookupTableUpdateFile">Update file</span>
                                            </button>
                                        </div>
                                    </div>
                                </v-form>
                            </li>
                        </ul>
                    </form>
                    <div class="button-holder">
                        <input type="button" class="btn lighter-hover" v-disabled="questionnaire.isReadOnlyForUser" v-i18next="[value]SideBarLookupTableAdd" value="ADD NEW Lookup table" v-click="addNewLookupTable()">
                    </div>
                </perfect-scrollbar>
            </div>
        </div>

    </div>
    <div class="left-side-panel attachments" v-class="{unfolded: isFolded }" v-controller="AttachmentsCtrl">
        <div class="foldback-region" v-click="foldback();$event.stopPropagation()"></div>
        <div class="left-side-panel-content attachments-panel">
            <div class="foldback-button-region" v-click="foldback();$event.stopPropagation()">
                <div class="foldback-button"></div>
            </div>
            <div class="attachments">
                <perfect-scrollbar class="scroller">
                    <div class="panel-header clearfix">
                        <div class="title pull-left">
                            <h3 v-i18next="[i18next]({count: attachments.length, bytes: formatBytes(totalSize())})SideBarAttachmentsCounter" />
                            <p class="estimated-download-time"
                               v-i18next="[i18next]({timeString: formatSeconds(estimatedLoadingTime()), downloadSpeed: benchmarkDownloadSpeed})SideBarAttachmentsEstimate">
                            </p>

                        </div>
                        <button class="btn btn-default btn-lg pull-left" v-class="{'btn-primary': !isReadOnlyForUser}"
                                ngf-select ngf-change="createAndUploadFile($file);$event.stopPropagation()"
                                ngf-accept="'.pdf,image/*,video/*,audio/*'" ngf-max-size="100MB" type="file" ngf-select-disabled="isReadOnlyForUser"
                                ngf-drop-disabled="isReadOnlyForUser" v-disabled="isReadOnlyForUser"
                                v-i18next="SideBarAttachmentsUpload">
                            Upload new
                        </button>
                    </div>
                    <div class="empty-list" v-show="attachments.length == 0">
                        <p v-i18next="SideBarAttachmentsEmptyLine1"></p>
                        <p>
                            <span v-i18next="SideBarAttachmentsEmptyLine2"></span>
                            <a href="https://support.mysurvey.solutions/questionnaire-designer/limits/multimedia-reference" target="_blank" v-i18next="ClickHere">
                                click here
                            </a>
                        </p>
                        <p v-bind-html="emptyAttachmentsDescription" />
                    </div>
                    <form role="form" name="attachmentsForm" novalidate>
                        <div class="attachment-list">
                            <v-form name="attachment.form" v-repeat="attachment in attachments">
                                <div class="attachments-panel-item" v-class="{'has-error': attachment.form.name.$error.pattern}" ngf-drop="" ngf-max-size="100MB" ngf-change="fileSelected(attachment, $file)" ngf-drag-over-class="{accept:'dragover', reject:'dragover-err'}">
                                    <a href v-click="deleteAttachment($index)" v-disabled="questionnaire.isReadOnlyForUser" v-if="!questionnaire.isReadOnlyForUser" class="btn delete-btn" tabindex="-1"></a>
                                    <div class="attachment">
                                        <div class="attachment-preview">
                                            <div class="attachment-preview-cover clearfix">
                                                <img class="pull-right" v-click="previewAttachment(attachment)" ngf-size="{width: 156, height: 140}" v-src='{{downloadLookupFileBaseUrl + "/" + questionnaire.questionnaireId + "/thumbnail/" + attachment.attachmentId}}'>
                                            </div>
                                        </div>
                                        <div class="attachment-content">
                                            <input focus-on-out="focusAttachment{{attachment.attachmentId}}"
                                                   required=""
                                                   v-i18next="[placeholder]SideBarAttachmentName"
                                                   maxlength="32"
                                                   spellcheck="false"
                                                   v-model="attachment.name"
                                                   name="name"
                                                   class="form-control table-name"
                                                   type="text" />
                                            <div class="divider"></div>
                                            <div class="drop-box" v-i18next="SideBarLookupTableDropFile">
                                                Drop File here
                                            </div>
                                            <div class="attachment-meta" v-include="'Image-attachment-info-template.html'"></div>
                                            <div class="actions clearfix" v-class="{dirty: attachment.form.$dirty }">
                                                <div v-show="attachment.form.$dirty" class="pull-left">
                                                    <button type="submit" v-disabled="questionnaire.isReadOnlyForUser || attachment.form.$invalid" class="btn lighter-hover" v-click="saveAttachment(attachment);$event.stopPropagation()" v-i18next>Save</button>
                                                    <button type="button" class="btn lighter-hover" v-click="cancel(attachment);$event.stopPropagation()" v-i18next>Cancel</button>
                                                </div>
                                                <div class="permanent-actions pull-right clearfix">
                                                    <button v-disabled="isReadOnlyForUser" class="btn btn-default pull-right" ngf-select="" ngf-accept="'.pdf,image/*,video/*,audio/*'" ngf-max-size="100MB" ngf-change="fileSelected(attachment, $file);$event.stopPropagation()" type="file">
                                                        <span v-i18next>Update</span>
                                                    </button>
                                                    <a href="{{downloadLookupFileBaseUrl + '/' + questionnaire.questionnaireId + '/' + attachment.attachmentId}}" class="btn btn-default pull-right" target="_blank" v-i18next>Download</a>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </v-form>
                        </div>
                    </form>
                </perfect-scrollbar>
            </div>
        </div>
    </div>
    <div class="left-side-panel translations" v-class="{unfolded: isFolded }" v-controller="TranslationsCtrl">
        <div class="foldback-region" v-click="foldback();$event.stopPropagation()"></div>
        <div class="left-side-panel-content translations-panel">
            <div class="foldback-button-region" v-click="foldback();$event.stopPropagation()">
                <div class="foldback-button"></div>
            </div>
            <div class="translations">
                <perfect-scrollbar class="scroller">
                    <h3 v-i18next="[i18next]({count: translations.length })SideBarTranslationsCounter"></h3>

                    <div class="empty-list" v-show="translations.length == 1">
                        <p v-i18next="SideBarTranslationsEmptyLine1"></p>
                        <p v-i18next="SideBarTranslationsEmptyLine2"></p>
                        <p v-i18next="SideBarTranslationsEmptyLine3"></p>
                    </div>
                    <form role="form" name="translationsForm" novalidate>
                        <div class="translation-list">
                            <v-form name="translation.form" v-repeat="translation in translations">
                                <div class="translations-panel-item" v-class="{'has-error': translation.form.name.$error.pattern}"
                                     ngf-drop="" ngf-max-size="4MB" ngf-change="fileSelected(translation, $file)"
                                     ngf-drag-over-class="{accept:'dragover', reject:'dragover-err'}">
                                    <a href
                                       v-click="deleteTranslation($index)" 
                                       v-disabled="questionnaire.isReadOnlyForUser" 
                                       class="btn delete-btn" 
                                       tabindex="-1"
                                       v-show="!translation.isOriginalTranslation && !questionnaire.isReadOnlyForUser"></a>
                                    <div class="translation-content">
                                        <input focus-on-out="focusTranslation{{translation.translationId}}"
                                               required=""
                                               v-i18next="[placeholder]SideBarTranslationName"
                                               maxlength="32"
                                               spellcheck="false"
                                               v-model="translation.name"
                                               name="name"
                                               class="form-control table-name"
                                               type="text" />
                                        <div class="drop-box" v-i18next="SideBarLookupTableDropFile">
                                            Drop File here
                                        </div>
                                        <div class="actions" v-class="{dirty: translation.form.$dirty }">
                                            <div v-show="translation.form.$dirty" class="pull-left">
                                                <button type="submit" v-disabled="questionnaire.isReadOnlyForUser || translation.form.$invalid" class="btn lighter-hover" v-click="onSave($event, translation)"
                                                        v-i18next>
                                                    Save
                                                </button>
                                                <button type="button" class="btn lighter-hover" v-click="onCancel($event, translation)" v-i18next>Cancel</button>
                                            </div>

                                            <button type="button" class="btn btn-default"
                                                    v-show="translation.isDefault && !translation.isOriginalTranslation"
                                                    v-click="setDefaultTranslation($index, false);$event.stopPropagation()" v-i18next>
                                                UnMarkAsDefault
                                            </button>

                                            <div class="permanent-actions pull-right">
                                                <button type="button" class="btn lighter-hover"
                                                        v-disabled="isReadOnlyForUser"
                                                        v-show="!translation.isDefault"
                                                        v-click="setDefaultTranslation($index, true);$event.stopPropagation()" v-i18next>
                                                    MarkAsDefault
                                                </button>

                                                <a v-if="translation.downloadUrl" href="{{translation.downloadUrl}}" class="btn btn-default"
                                                   target="_blank" v-i18next>SideBarTranslationDownloadXlsx</a>

                                                <button v-hide="translation.form.$dirty || translation.isOriginalTranslation" 
                                                        v-disabled="isReadOnlyForUser" 
                                                        class="btn btn-default" 
                                                        ngf-select="" 
                                                        ngf-max-size="10MB" 
                                                        accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel"
                                                        ngf-change="fileSelected(translation, $file);$event.stopPropagation()" 
                                                        type="button">
                                                    <span v-i18next>Update</span>
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </v-form>
                        </div>
                    </form>
                    <div class="button-holder">
                        <p>
                            <span v-i18next="SideBarTranslationGetTemplate"></span>
                            <a class="btn btn-default" href="{{downloadBaseUrl + '/' + questionnaire.questionnaireId + '/template' }}" target="_blank" rel="noopener" v-i18next>
                                SideBarTranslationGetTemplateLinkTextXlsx
                            </a>
                            <a class="btn btn-default" href="{{downloadBaseUrl + '/' + questionnaire.questionnaireId + '/templateCsv' }}" target="_blank" rel="noopener" v-i18next>
                                SideBarTranslationGetTemplateLinkTextCsv
                            </a>
                        </p>
                        <p>
                            <input type="button" v-i18next="[value]SideBarTranslationsUploadNew" value="Upload new translation"
                                   v-disabled="isReadOnlyForUser"
                                   class="btn lighter-hover" 
                                   ngf-select
                                   ngf-change="createAndUploadFile($file);$event.stopPropagation()"
                                   ngf-max-size="10MB"
                                   accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel"
                                   ngf-select-disabled="isReadOnlyForUser"
                                   ngf-drop-disabled="isReadOnlyForUser" />
                        </p>
                    </div>
                </perfect-scrollbar>
            </div>
        </div>
    </div>


    <div class="left-side-panel categories" v-class="{unfolded: isFolded }" v-controller="CategoriesCtrl">
        <div class="foldback-region" v-click="foldback();$event.stopPropagation()"></div>
        <div class="left-side-panel-content categories-panel">
            <div class="foldback-button-region" v-click="foldback();$event.stopPropagation()">
                <div class="foldback-button"></div>
            </div>
            <div class="categories">
                <div id="show-reload-details-promt" class="v-cloak" v-show="shouldUserSeeReloadPromt">
                    <div class="inner">{{'QuestionToUpdateOptions' | i18next}} <a href="#" onclick="window.location.reload(true);" v-i18next="QuestionClickReload"></a></div>
                </div>

                <perfect-scrollbar class="scroller">
                    <h3 v-i18next="[i18next]({count: categoriesList.length })SideBarCategoriesCounter"></h3>

                    <div class="empty-list" v-show="categoriesList.length == 0">
                        <p v-i18next="SideBarCategoriesEmptyLine1"></p>
                        <p v-i18next="SideBarCategoriesEmptyLine2"></p>
                        <p>
                            <span class="variable-name" v-i18next="VariableName"></span>
                            {{'SideBarCategoriesEmptyLine3' | i18next}}
                        </p>
                    </div>
                    <form role="form" name="categoriesForm" novalidate>
                        <div class="categories-list">
                            <v-form name="categories.form" v-repeat="categories in categoriesList">
                                <div class="categories-panel-item" v-class="{'has-error': categories.form.name.$error.pattern}"
                                     ngf-drop="" ngf-change="fileSelected(categories, $file)"
                                     ngf-drag-over-class="{accept:'dragover', reject:'dragover-err'}">
                                    <a href v-click="deleteCategories($index)" class="btn delete-btn" tabindex="-1" v-if="!questionnaire.isReadOnlyForUser"></a>
                                    <div class="categories-content">
                                        <input focus-on-out="focusCategories{{categories.categoriesId}}"
                                               required=""
                                               v-i18next="[placeholder]SideBarCategoriesName"
                                               maxlength="32"
                                               spellcheck="false"
                                               v-model="categories.name"
                                               name="name"
                                               class="form-control table-name"
                                               type="text"/>
                                        <div class="drop-box" v-i18next="SideBarLookupTableDropFile">
                                            Drop File here
                                        </div>
                                        <div class="actions" v-class="{dirty: categories.form.$dirty }">
                                            <div v-show="categories.form.$dirty" class="pull-left">
                                                <button type="submit" v-disabled="categories.form.$invalid" class="btn lighter-hover"
                                                        v-click="saveCategories(categories);$event.stopPropagation()"
                                                        v-i18next>
                                                    Save
                                                </button>
                                                <button type="button" class="btn lighter-hover" v-click="cancel(categories);$event.stopPropagation()" v-i18next>Cancel</button>
                                            </div>

                                            <div class="permanent-actions pull-right">
                                                <a
                                                    href="javascript:void(0);"
                                                    class="btn btn-link"
                                                    v-click="editCategories(questionnaire.questionnaireId, categories.categoriesId)" v-i18next="SideBarEditCategories">
                                                </a>

                                                <button v-hide="categories.form.$dirty" v-disabled="isReadOnlyForUser"
                                                        class="btn btn-default" ngf-select="" ngf-max-size="10MB"
                                                        accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel,.txt,.tsv,.tab"
                                                        ngf-change="fileSelected(categories, $file);$event.stopPropagation()" type="button">
                                                    <span v-i18next>Update</span>
                                                </button>

                                                {{'SideBarDownload' | i18next}}
                                                <a href="{{'/questionnaire/ExportOptions/' + questionnaire.questionnaireId  + '?type=xlsx&isCategory=true&entityId=' + categories.categoriesId}}" class="btn btn-default"
                                                   target="_blank" rel="noopener" v-i18next>SideBarXlsx</a>
                                                <a href="{{'/questionnaire/ExportOptions/' + questionnaire.questionnaireId  + '?type=csv&isCategory=true&entityId=' + categories.categoriesId}}" class="btn btn-default"
                                                   target="_blank" rel="noopener" v-i18next>SideBarTab</a>

                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </v-form>
                        </div>
                    </form>
                    <div class="button-holder">
                        <p>
                            <span v-i18next="SideBarTranslationGetTemplate"></span>

                            <a class="btn btn-default" href="{{downloadBaseUrl + '/template' }}" target="_blank" rel="noopener" v-i18next>
                                SideBarXlsx
                            </a>
                            <a class="btn btn-default" href="{{downloadBaseUrl + '/templateTab' }}" target="_blank" rel="noopener" v-i18next>
                                SideBarTab
                            </a>
                        </p>
                        <p>
                            <input type="button" v-i18next="[value]SideBarCategoriesAddNew" value="ADD new category"
                                   class="btn lighter-hover" 
                                   v-click="addNewCategory();$event.stopPropagation()"
                                   v-disabled="isReadOnlyForUser" />
                        </p>
                        <p>
                            <input type="button" v-i18next="[value]SideBarCategoriesUploadNew" value="Upload new categories"
                                   class="btn lighter-hover" ngf-select
                                   ngf-change="createAndUploadFile($file);$event.stopPropagation()"
                                   ngf-max-size="10MB"
                                   accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel,.txt,.tsv,.tab"
                                   ngf-select-disabled="isReadOnlyForUser"
                                   ngf-drop-disabled="isReadOnlyForUser"
                                   v-disabled="isReadOnlyForUser" />
                        </p>
                    </div>
                </perfect-scrollbar>
            </div>
        </div>
    </div>

    <div class="left-side-panel metadata" v-class="{unfolded: isFolded }" v-controller="MetadataCtrl">
        <div class="foldback-region" v-click="foldback();$event.stopPropagation()"></div>
        <div class="left-side-panel-content metadata-panel">
            <div class="foldback-button-region" v-click="foldback();$event.stopPropagation()">
                <div class="foldback-button"></div>
            </div>

            <div class="metadata">
                <perfect-scrollbar class="scroller">
                    <h3 v-i18next="SideBarMetadataHeader"></h3>

                    <form role="form" name="metadataForm" novalidate>
                        <v-form name="metadata.form">

                            <ul class="list-unstyled metadata-blocks">
                                <li>
                                    <h4>{{'SideBarMetadataBasicInfo' | i18next}}</h4>
                                    <div class="field-wrapper">
                                        <span class="label-title">{{'SideBarMetadataTitle' | i18next}}</span>
                                        <div class="form-group">
                                            <input type="text" class="form-control" name="title" required="" minlength="1" v-model="metadata.title" />
                                        </div>
                                    </div>
                                    <div class="field-wrapper">
                                        <span class="label-title">{{'SideBarMetadataSubtitle' | i18next}}</span>
                                        <div class="form-group">
                                            <textarea class="form-control msd-elastic" name="subTitle" v-model="metadata.subTitle"></textarea>
                                        </div>
                                    </div>
                                    <div class="field-wrapper">
                                        <span class="label-title">{{'SideBarMetadataVersionIdentificator' | i18next}}</span>
                                        <div class="form-group">
                                            <input type="text" class="form-control" name="version" v-model="metadata.version">
                                        </div>
                                    </div>
                                    <div class="field-wrapper">
                                        <span class="label-title">{{'SideBarMetadataVersionNotes' | i18next}}</span>
                                        <div class="form-group">
                                            <textarea class="form-control msd-elastic" name="versionNotes" v-model="metadata.versionNotes"></textarea>
                                        </div>
                                    </div>
                                </li>
                                <li>
                                    <h4>{{'SideBarMetadataSurveyDataInfo' | i18next}}</h4>
                                    <div class="field-wrapper">
                                        <span class="label-title">{{'SideBarMetadataStudyTypes' | i18next}}</span>
                                        <div class="form-group">
                                            <div class="btn-group dropdown" v-class="{'has-value': metadata.studyType }">
                                                <button class="btn btn-default dropdown-toggle" type="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="true">
                                                    {{ metadata.studyType ? (questionnaire.studyTypes | filter : {'code':metadata.studyType})[0].title : 'SelectStudyType' | i18next }}
                                                    <span class="dropdown-arrow" aria-labelledby="dropdownMenu12"></span>
                                                </button>
                                                <button type="button" class="btn btn-link btn-clear" v-click="metadata.studyType = null; metadata.form.$setDirty();">
                                                    <span></span>
                                                </button>
                                                <div class="dropdown-menu" aria-labelledby="dropdownMenu12">
                                                    <perfect-scrollbar class="scroller">
                                                        <ul class="list-unstyled">
                                                            <li v-repeat="studyType in questionnaire.studyTypes">
                                                                <a v-click="metadata.studyType = studyType.code; metadata.form.$setDirty();" value="{{studyType.code}}" href="#">{{studyType.title}}</a>
                                                            </li>
                                                        </ul>
                                                    </perfect-scrollbar>
                                                </div>
                                            </div>
                                            <input type="hidden" class="form-control" name="studyType" v-model="metadata.studyType" />
                                        </div>
                                    </div>
                                    <div class="field-wrapper">
                                        <span class="label-title">{{'SideBarMetadataKindOfData' | i18next}}</span>
                                        <div class="form-group">
                                            <div class="btn-group dropdown" v-class="{'has-value': metadata.kindOfData }">
                                                <button class="btn btn-default dropdown-toggle" type="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="true">
                                                    {{ metadata.kindOfData ? (questionnaire.kindsOfData | filter : {'code':metadata.kindOfData})[0].title  : 'SelectKindOfData' | i18next}}
                                                    <span class="dropdown-arrow" aria-labelledby="dropdownMenu12"></span>
                                                </button>
                                                <button type="button" class="btn btn-link btn-clear" v-click="metadata.kindOfData = null; metadata.form.$setDirty();">
                                                    <span></span>
                                                </button>
                                                <div class="dropdown-menu " aria-labelledby="dropdownMenu12">
                                                    <ul class="scroller list-unstyled">
                                                        <li v-repeat="kindOfData in questionnaire.kindsOfData">
                                                            <a v-click="metadata.kindOfData = kindOfData.code; metadata.form.$setDirty();" value="{{kindOfData.code}}" href="#">{{kindOfData.title}}</a>
                                                        </li>
                                                    </ul>
                                                </div>
                                            </div>
                                        </div>
                                        <input type="hidden" class="form-control" name="kindOfData" v-model="metadata.kindOfData" />
                                    </div>
                                    <div class="field-wrapper">
                                        <span class="label-title">{{'SideBarMetadataModeOfDataCollection' | i18next}}</span>

                                        <div class="form-group">
                                            <div class="btn-group dropdown" v-class="{'has-value': metadata.modeOfDataCollection }">
                                                <button class="btn btn-default dropdown-toggle" type="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="true">
                                                    {{ metadata.modeOfDataCollection ? (questionnaire.modesOfDataCollection | filter : {'code':metadata.modeOfDataCollection})[0].title  : 'SelectModeOfDataCollection' | i18next}}
                                                    <span class="dropdown-arrow" aria-labelledby="dropdownMenu12"></span>
                                                </button>
                                                <button type="button" class="btn btn-link btn-clear" v-click="metadata.modeOfDataCollection = null; metadata.form.$setDirty();">
                                                    <span></span>
                                                </button>
                                                <div class="dropdown-menu " aria-labelledby="dropdownMenu12">
                                                    <ul class="scroller list-unstyled">
                                                        <li v-repeat="modeOfData in questionnaire.modesOfDataCollection">
                                                            <a v-click="metadata.modeOfDataCollection = modeOfData.code; metadata.form.$setDirty();" value="{{modeOfData.code}}" href="#">{{modeOfData.title}}</a>
                                                        </li>
                                                    </ul>
                                                </div>
                                            </div>
                                        </div>
                                        <input type="hidden" class="form-control" id="modeOfDataCollection" name="modeOfDataCollection" v-model="metadata.modeOfDataCollection" />
                                    </div>
                                </li>
                                <li>
                                    <h4>{{'SideBarMetadataSurveyInfo' | i18next}}</h4>
                                    <div class="field-wrapper">
                                        <span class="label-title">{{'SideBarMetadataCountry' | i18next}}</span>
                                        <div class="form-group">
                                            <div class="btn-group dropdown" v-class="{'has-value': metadata.country }">
                                                <button class="btn btn-default dropdown-toggle" type="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="true">
                                                    {{ metadata.country ? (questionnaire.countries | filter : {'code':metadata.country})[0].title  : 'SelectCountry' | i18next}}
                                                    <span class="dropdown-arrow" aria-labelledby="dropdownMenu12"></span>
                                                </button>
                                                <button type="button" class="btn btn-link btn-clear" v-click="metadata.country = null; metadata.form.$setDirty();">
                                                    <span></span>
                                                </button>
                                                <div class="dropdown-menu" aria-labelledby="dropdownMenu12">
                                                    <perfect-scrollbar class="scroller">
                                                        <ul class="list-unstyled">
                                                            <li v-repeat="country in questionnaire.countries">
                                                                <a v-click="metadata.country = country.code; metadata.form.$setDirty();" value="{{country.code}}" href="#">{{country.title}}</a>
                                                            </li>
                                                        </ul>
                                                    </perfect-scrollbar>
                                                </div>
                                            </div>
                                        </div>
                                        <input type="hidden" class="form-control" name="country" v-model="metadata.country" />
                                    </div>
                                    <div class="field-wrapper">
                                        <span class="label-title">{{'SideBarMetadataYear' | i18next}}</span>
                                        <div class="form-group">
                                            <input type="text" min="0" max="9999" pattern="\d*" onkeypress='return event.charCode >= 48 && event.charCode <= 57' maxlength="4" v-pattern="/^\d+$/" class="form-control date-field" name="year" v-model="metadata.year" />
                                        </div>
                                        <p class="help-block v-cloak" v-show="metadata.form.year.$error.pattern" v-i18next="QuestionOnlyInts">
                                        </p>
                                    </div>
                                    <div class="field-wrapper">
                                        <span class="label-title">{{'SideBarMetadataLanguages' | i18next}}</span>
                                        <div class="form-group">
                                            <input type="text" class="form-control" name="language" v-model="metadata.language" />
                                        </div>
                                    </div>
                                    <div class="field-wrapper">
                                        <span class="label-title">{{'SideBarMetadataUnitOfAlalysis' | i18next}}</span>
                                        <div class="form-group">
                                            <textarea class="form-control msd-elastic" name="unitOfAnalysis" v-model="metadata.unitOfAnalysis"></textarea>
                                        </div>
                                    </div>
                                    <div class="field-wrapper">
                                        <span class="label-title">{{'SideBarMetadataCoverage' | i18next}}</span>
                                        <div class="form-group">
                                            <textarea class="form-control msd-elastic" name="coverage" v-model="metadata.coverage"></textarea>
                                        </div>
                                    </div>
                                    <div class="field-wrapper">
                                        <span class="label-title">{{'SideBarMetadataUniverse' | i18next}}</span>
                                        <div class="form-group">
                                            <textarea class="form-control msd-elastic" name="universe" v-model="metadata.universe"></textarea>
                                        </div>
                                    </div>
                                    <div class="field-wrapper">
                                        <span class="label-title">{{'SideBarMetadataPrimaryInvestigator' | i18next}}</span>
                                        <div class="form-group">
                                            <textarea class="form-control msd-elastic" name="primaryInvestigator" v-model="metadata.primaryInvestigator"></textarea>
                                        </div>
                                    </div>
                                    <div class="field-wrapper">
                                        <span class="label-title">{{'SideBarMetadataConsultants' | i18next}}</span>
                                        <div class="form-group">
                                            <textarea class="form-control msd-elastic" name="consultant" v-model="metadata.consultant">
                                            </textarea>
                                        </div>
                                    </div>
                                    <div class="field-wrapper">
                                        <span class="label-title">{{'SideBarMetadataFunding' | i18next}}</span>
                                        <div class="form-group">
                                            <textarea class="form-control msd-elastic" name="funding" v-model="metadata.funding">
                                            </textarea>
                                        </div>
                                    </div>
                                </li>
                                <li>
                                    <h4>{{'SideBarMetadataAdditionalInfo' | i18next}}</h4>
                                    <div class="field-wrapper">
                                        <span class="label-title">{{'SideBarMetadataNotes' | i18next}}</span>
                                        <div class="form-group">
                                            <textarea class="form-control msd-elastic" name="notes" v-model="metadata.notes"></textarea>
                                        </div>
                                    </div>
                                    <div class="field-wrapper">
                                        <span class="label-title">{{'SideBarMetadataKeywords' | i18next}}</span>
                                        <div class="form-group">
                                            <textarea class="form-control msd-elastic" name="keywords" v-model="metadata.keywords"></textarea>
                                        </div>
                                    </div>
                                </li>
                                <li>
                                    <h4>{{'SideBarMetadataQuestionnaireAccess' | i18next}}</h4>
                                    <div class="checkbox">
                                        <input id="agreeToMakeThisQuestionnairePublic" name="agreeToMakeThisQuestionnairePublic" v-model="metadata.agreeToMakeThisQuestionnairePublic" type="checkbox" class="checkbox-filter" checked>
                                        <label for="agreeToMakeThisQuestionnairePublic" class=""><span class="tick"></span>{{'SideBarMetadataAgreeToMakeThisQuestionnairePublic' | i18next}}</label>
                                    </div>
                                </li>
                            </ul>
                            <div class="form-buttons-holder" v-class="{dirty: metadata.form.$dirty }">
                                <button type="submit" class="btn btn-lg v-isolate-scope" v-disabled="questionnaire.isReadOnlyForUser || metadata.form.$invalid" v-class="{ 'btn-primary': metadata.form.$dirty }" 
                                        v-click="saveMetadata();$event.stopPropagation()" v-i18next>Save</button>
                                <button type="button" class="btn btn-lg btn-link v-isolate-scope" v-click="cancelMetadata();$event.stopPropagation()" v-i18next>Cancel</button>
                            </div>
                        </v-form>
                    </form>
                </perfect-scrollbar>

            </div>
        </div>
    </div>
    <div class="left-side-panel comments" v-class="{unfolded: isFolded }" v-controller="CommentsCtrl" data-empty-place-holder-enabled="false">
        <div class="foldback-region" v-click="foldback();$event.stopPropagation()"></div>
        <div class="left-side-panel-content comments-panel">
            <div class="foldback-button-region" v-click="foldback();$event.stopPropagation()">
                <div class="foldback-button"></div>
            </div>
            <div class="comments">
                <perfect-scrollbar class="scroller">
                    <h3>
                        <span v-i18next="[i18next]({count: commentThreads.length})SideBarCommentsCounter"></span>
                    </h3>
                    <div class="empty-list" v-show="commentThreads.length == 0">
                        <p v-i18next>SideBarEmptyCommentsLine</p>
                    </div>
                    <ul v-model="commentThreads">
                        <li class="comment-thread" v-repeat="commentThread in commentThreads">
                            <a class="reference-item" href="javascript:void(0);" v-click="showCommentsAndNavigateTo(commentThread.entity)">
                                <span v-if="commentThread.entity.type == 'Question'" class="icon {{commentThread.entity.questionType}} "></span>
                                <span v-if="commentThread.entity.type !== 'Question' && commentThread.entity.type !== 'Group' && commentThread.entity.type !== 'Roster'" class="icon icon-{{commentThread.entity.type.toLowerCase()}}"></span>
                                <span class="title">{{commentThread.entity.title | escape}}</span>
                                <span class="variable" v-bind-html="commentThread.entity.variable || '&nbsp;'">&nbsp;</span>
                            </a>
                            <div class="comments-in-thread">
                                <ul>
                                    <li class="comment" v-class="{resolved: comment.isResolved }" v-repeat="comment in commentThread.comments">
                                        <span class="author">{{comment.userEmail}}</span>
                                        <span class="date">{{comment.date}}</span>
                                        <p class="comment-text">{{comment.comment}}</p>
                                    </li>
                                </ul>
                                <div v-if="commentThread.resolvedComments.length > 0">
                                    <a href="javascript:void(0);" class="show-more" v-click="commentThread.toggleResolvedComments()">
                                        <span v-hide="commentThread.resolvedAreExpanded" v-i18next="[i18next]({count: commentThread.resolvedComments.length})ViewResolvedCommentsCounter"></span>
                                        <span v-show="commentThread.resolvedAreExpanded" v-i18next>HideResolvedComments</span>
                                    </a>
                                    <ul v-show="commentThread.resolvedAreExpanded">
                                        <li class="comment" v-class="{resolved: comment.isResolved }" v-repeat="comment in commentThread.resolvedComments">
                                            <span class="author">{{comment.userEmail}}</span>
                                            <span class="date">{{comment.date}}</span>
                                            <p class="comment-text">{{comment.comment}}</p>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </li>
                    </ul>
                </perfect-scrollbar>
            </div>
        </div>
    </div>
    <div id="left-menu" v-controller="LeftMenuCtrl">
        <ul>
            <li><a class="left-menu-chapters" v-class="{unfolded: isUnfoldedChapters }" v-click="unfoldChapters()" v-i18next="[title]SideBarSectionsTitle"></a></li>
            <li><a class="left-menu-metadata" v-class="{unfolded: isUnfoldedMetadata }" v-click="unfoldMetadata()" v-i18next="[title]SideBarMetadataTitle"></a></li>
            <li><a class="left-menu-translations" v-class="{unfolded: isUnfoldedTranslations }" v-click="unfoldTranslations()" v-i18next="[title]SideBarTranslationsTitle"></a></li>
            <li><a class="left-menu-categories" v-class="{unfolded: isUnfoldedCategories }" v-click="unfoldCategories()" v-i18next="[title]SideBarCategoriesTitle"></a></li>
            <li><a class="left-menu-scenarios" v-if="questionnaire.questionnaireRevision === null" v-class="{unfolded: isUnfoldedScenarios }" v-click="unfoldScenarios()" v-i18next="[title]SideBarScenarioTitle"></a></li>
            <li><a class="left-menu-macroses" v-class="{unfolded: isUnfoldedMacros }" v-click="unfoldMacros()" v-i18next="[title]SideBarMacroTitle"></a></li>
            <li><a class="left-menu-lookupTables" v-class="{unfolded: isUnfoldedLookupTables }" v-click="unfoldLookupTables()" v-i18next="[title]SideBarLookupTitle"></a></li>
            <li><a class="left-menu-attachments" v-class="{unfolded: isUnfoldedAttachments }" v-click="unfoldAttachments()" v-i18next="[title]SideBarAttachmentsTitle"></a></li>
            <li><a class="left-menu-comments" v-if="!questionnaire.isReadOnlyForUser" v-class="{unfolded: isUnfoldedComments }" v-click="unfoldComments()" v-i18next="[title]SideBarCommentsTitle"></a></li>
        </ul>
    </div>
    <div class="questionnaire-tree" ui-view></div>
  </section>

</template>

<script>
import { questionnaireService } from '../../services';

export default {
    name: 'Main',
    data() {
      return {         
        questionnaire = {
                questionsCount: 0,
                groupsCount: 0,
                rostersCount: 0,
                chapters = []
            },
        currentUserIsAuthenticated = false,
        isReadOnlyForUser = true,
        verificationStatus = {
                errors: null,
                warnings: null,
                visible: false,
                time: new Date()
            },
        lookupTables = [],
        scenarios = [],
        commentThreads =[],

        
        isUnfoldedChapters = false,
        isUnfoldedScenarios = false,
        isUnfoldedMacros = false,
        isUnfoldedLookupTables = false,
        isUnfoldedAttachments = false,
        isUnfoldedTranslations = false,
        isUnfoldedMetadata = false,
        isUnfoldedComments = false,
        isUnfoldedCategories = false,

      };
    },
    methods: {
        showDownloadPdf(){
    //     $uibModal.open({
    //                 templateUrl: 'views/pdf.html',
    //                 controller: 'pdfCtrl',
    //                 windowClass: 'share-window',
    //                 resolve:
    //                 {
    //                     currentUser: function() {
    //                         return {
    //                             name: $scope.currentUserName,
    //                             email: $scope.currentUserEmail,
    //                             isAuthenticated: $scope.currentUserIsAuthenticated
    //                         }
    //                     },
    //                     questionnaire: function() {
    //                         return $scope.questionnaire;
    //                     }
    //                 }
    //             });
        },
        exportQuestionnaire(){},
        showShareInfo(){},
        verify(){},
        showVerificationErrors(){},
        showVerificationWarnings(){},
        addNewCategory() {
                if (this.isReadOnlyForUser) {
                    notificationService.notice($i18next.t('NoPermissions'));
                    return;
                }

                var categories = { categoriesId: utilityService.guid() };
                
                commandService.updateCategories($state.params.questionnaireId, categories)
                    .then(function (response) {
                        if (response.status !== 200) return;

                        categories.checkpoint = categories.checkpoint || {};

                        dataBind(categories.checkpoint, categories);
                        $scope.categoriesList.push(categories);
                        updateQuestionnaireCategories();

                        setTimeout(function() {
                            utilityService.focus("focusCategories" + categories.categoriesId);
                        }, 500);
                    }).catch(function() { });
            },
        unfoldMetadata(){},
        unfoldChapters(){},


        getQuestionnaire() {
                questionnaireService.getQuestionnaireById($state.params.questionnaireId).then(function (result) {
                    this.questionnaire = result.data;
                    if (!$state.params.chapterId && result.data.chapters.length > 0) {
                        var defaultChapter = _.first(result.data.chapters);
                        var itemId = defaultChapter.itemId;
                        $scope.currentChapter = defaultChapter;
                        $state.go('questionnaire.chapter.group', { chapterId: itemId, itemId: itemId });
                    }

                    $rootScope.$emit('questionnaireLoaded');
                });
                },
    },
    mounted() {
        this.getQuestionnaire();
    }
}
</script>

