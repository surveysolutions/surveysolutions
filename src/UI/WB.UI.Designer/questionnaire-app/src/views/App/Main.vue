<template>
    <div id="designer-editor" class="container" ui-view>
        <section id="header" class="row">
            <a class="designer-logo" href="../../"></a>
            <div class="header-line">
                <div class="header-menu">
                    <div class="buttons">
                        <a
                            class="btn"
                            href="http://support.mysurvey.solutions/designer"
                            target="_blank"
                            >{{ $t('QuestionnaireEditor.Help') }}</a
                        >
                        <a
                            class="btn"
                            href="https://forum.mysurvey.solutions"
                            target="_blank"
                            >{{ $t('QuestionnaireEditor.Forum') }}
                        </a>

                        <a
                            class="btn"
                            href="../../questionnaire/questionnairehistory/{{questionnaire.questionnaireId}}"
                            target="_blank"
                            v-if="
                                questionnaire.hasViewerAdminRights ||
                                    questionnaire.isSharedWithUser
                            "
                            >{{ $t('QuestionnaireEditor.History') }}</a
                        >
                        <button
                            class="btn"
                            type="button"
                            @click="showDownloadPdf()"
                        >
                            {{ $t('QuestionnaireEditor.DownloadPdf') }}
                        </button>
                        <a
                            class="btn"
                            type="button"
                            v-if="questionnaire.hasViewerAdminRights"
                            @click="ortQuestionnaire()"
                            >{{ $t('QuestionnaireEditor.SaveAs') }}</a
                        >

                        <a
                            class="btn"
                            v-if="
                                questionnaire.questionnaireRevision ||
                                    questionnaire.isReadOnlyForUser
                            "
                            href="../../questionnaire/clone/{{questionnaire.questionnaireId}}{{questionnaire.questionnaireRevision ? '$' + questionnaire.questionnaireRevision : ''}}"
                            target="_blank"
                            >{{ $t('QuestionnaireEditor.CopyTo') }}</a
                        >
                        <button
                            class="btn"
                            type="button"
                            v-disabled="
                                questionnaire.isReadOnlyForUser &&
                                    !questionnaire.hasViewerAdminRights &&
                                    !questionnaire.isSharedWithUser
                            "
                            @click="showShareInfo()"
                        >
                            {{ $t('QuestionnaireEditor.Settings') }}
                        </button>

                        <div
                            class="btn-group"
                            v-if="currentUserIsAuthenticated"
                        >
                            <!-- <a class="btn btn-default">{{ $t('QuestionnaireEditor.HellowMessageBtn', {currentUserName:currentUserName}) }}</a> -->
                            <a
                                class="btn btn-default dropdown-toggle"
                                data-toggle="dropdown"
                                aria-haspopup="true"
                                aria-expanded="false"
                            >
                                <span class="caret"></span>
                                <span class="sr-only">{{
                                    $t('QuestionnaireEditor.ToggleDropdown')
                                }}</span>
                            </a>
                            <ul class="dropdown-menu dropdown-menu-right">
                                <li>
                                    <a href="../../identity/account/manage">{{
                                        $t('QuestionnaireEditor.ManageAccount')
                                    }}</a>
                                </li>
                                <li>
                                    <a href="../../identity/account/logout">{{
                                        $t('QuestionnaireEditor.LogOut')
                                    }}</a>
                                </li>
                            </ul>
                        </div>
                        <a
                            class="btn"
                            href="/"
                            type="button"
                            v-if="!currentUserIsAuthenticated"
                            >{{ $t('QuestionnaireEditor.Login') }}</a
                        >
                        <a
                            class="btn"
                            href="/identity/account/register"
                            type="button"
                            v-if="!currentUserIsAuthenticated"
                            >{{ $t('QuestionnaireEditor.Register') }}</a
                        >
                    </div>
                </div>
                <div class="questionnarie-title">
                    <div class="title">
                        <div class="questionnarie-title-text">
                            {{ questionnaire.title }}
                        </div>
                        <div class="questionnarie-title-buttons">
                            <!-- <span class="text-muted">{{ $t('QuestionnaireEditor.QuestionnaireSummary', { questionsCount: questionnaire.questionsCount, groupsCount: questionnaire.groupsCount, rostersCount: questionnaire.rostersCount}) }} 

                    </span>-->
                            <button
                                id="verification-btn"
                                type="button"
                                class="btn"
                                @click="verify()"
                                v-if="
                                    questionnaire.questionnaireRevision === null
                                "
                            >
                                {{ $t('QuestionnaireEditor.Compile') }}
                            </button>
                            <span
                                v-if="
                                    verificationStatus.warnings != null &&
                                        verificationStatus.errors != null
                                "
                            >
                                <span
                                    data-toggle="modal"
                                    v-if="
                                        verificationStatus.warnings.length +
                                            verificationStatus.errors.length >
                                            0
                                    "
                                    class="error-message v-hide"
                                    v-class="{
                                        'no-errors':
                                            verificationStatus.errors.length ==
                                            0
                                    }"
                                >
                                    <!-- <a href="javascript:void(0);"
                               @click="showVerificationErrors()">{{ $t('QuestionnaireEditor.ErrorsCounter',{ count: verificationStatus.errors.length}) }}
                            </a> -->
                                </span>
                                <span
                                    data-toggle="modal"
                                    v-if="
                                        verificationStatus.warnings.length > 0
                                    "
                                    class="warniv-message v-hide"
                                >
                                    <a
                                        href="javascript:void(0);"
                                        @click="showVerificationWarnings()"
                                    >
                                        <!-- {{ $t('QuestionnaireEditor.WarningsCounter',{ count: verificationStatus.warnings.length}) }} -->
                                    </a>
                                </span>
                                <span
                                    class="text-success"
                                    v-if="
                                        verificationStatus.warnings.length +
                                            verificationStatus.errors.length ===
                                            0
                                    "
                                    >{{ $t('QuestionnaireEditor.Ok') }}</span
                                >

                                <span class="text-success">
                                    <!-- <em>{{ $t('QuestionnaireEditor.SavedAtTimestamp',{ dateTime: verificationStatus.time}) }}</em> -->
                                </span>
                            </span>
                            <span
                                class="error-message strong"
                                v-if="questionnaire.isReadOnlyForUser"
                                >{{ $t('QuestionnaireEditor.ReadOnly') }}</span
                            >
                            <button
                                id="webtest-btn"
                                type="button"
                                class="btn"
                                v-if="
                                    questionnaire.webTestAvailable &&
                                        questionnaire.questionnaireRevision ===
                                            null
                                "
                                @click="webTest()"
                            >
                                {{ $t('QuestionnaireEditor.Test') }}
                            </button>
                            <!-- <span class="error-message strong" v-if="questionnaire.previewRevision !== null && questionnaire.previewRevision !== undefined">
                        {{ $t('QuestionnaireEditor.Preview',{ revision: questionnaire.previewRevision}) }}
                    </span> -->
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
            <div
                class="left-side-panel chapters"
                v-class="{ unfolded: isFolded }"
                data-empty-place-holder-enabled="false"
            >
                <div
                    class="foldback-region"
                    @click="
                        $event => {
                            foldback();
                            $event.stopPropagation();
                        }
                    "
                ></div>
                <div
                    class="left-side-panel-content chapter-panel"
                    ui-tree="chaptersTree"
                >
                    <div
                        class="foldback-button"
                        @click="
                            $event => {
                                foldback();
                                $event.stopPropagation();
                            }
                        "
                    ></div>
                    <div class="ul-holder">
                        <div class="chapters">
                            <perfect-scrollbar class="scroller">
                                <h3>
                                    <!-- <span>{{ $t('QuestionnaireEditor.SideBarSectionsCounter',{ count: questionnaire.chapters.length}) }}</span>-->
                                </h3>
                                <!-- v-bind="questionnaire.chapters" -->
                                <ul ui-tree-nodes class="chapters-list">
                                    <li
                                        class="chapter-panel-item"
                                        v-for="chapter in questionnaire.chapters"
                                        ui-tree-node
                                        data-nodrag="{{ chapter.isCover }}"
                                        v-class="{
                                            current: isCurrentChapter(chapter)
                                        }"
                                        context-menu
                                        data-target="chapter-context-menu-{{ chapter.itemId }}"
                                        context-menu-hide-on-mouse-leave="true"
                                    >
                                        <div
                                            class="holder"
                                            @click="
                                                $event => {
                                                    editChapter(chapter);
                                                    $event.stopPropagation();
                                                }
                                            "
                                        >
                                            <div class="inner">
                                                <a
                                                    class="handler"
                                                    ui-tree-handle
                                                    v-if="
                                                        !questionnaire.isReadOnlyForUser &&
                                                            !chapter.isCover
                                                    "
                                                    ><span></span
                                                ></a>
                                                <a
                                                    class="chapter-panel-item-body"
                                                    ui-sref="questionnaire.chapter.group({ chapterId: chapter.itemId, itemId: chapter.itemId})"
                                                >
                                                    <span
                                                        v-html="chapter.title"
                                                    ></span>
                                                    <help
                                                        key="coverPage"
                                                        v-if="chapter.isCover"
                                                    />
                                                </a>
                                                <div
                                                    class="qname-block chapter-panel-item-condition"
                                                >
                                                    <div
                                                        class="conditions-block"
                                                    >
                                                        <div
                                                            class="enabliv-group-marker"
                                                            v-class="{
                                                                'hide-if-disabled':
                                                                    chapter.hideIfDisabled
                                                            }"
                                                            v-if="
                                                                chapter.hasCondition
                                                            "
                                                        ></div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        <div
                                            class="dropdown position-fixed"
                                            id="chapter-context-menu-{{ chapter.itemId }}"
                                        >
                                            <ul
                                                class="dropdown-menu"
                                                role="menu"
                                            >
                                                <li>
                                                    <a
                                                        @click="
                                                            $event => {
                                                                editChapter(
                                                                    chapter
                                                                );
                                                            }
                                                        "
                                                        >{{
                                                            $t(
                                                                'QuestionnaireEditor.Open'
                                                            )
                                                        }}</a
                                                    >
                                                </li>
                                                <li>
                                                    <a
                                                        @click="
                                                            $event => {
                                                                copyRef(
                                                                    chapter
                                                                );
                                                            }
                                                        "
                                                        >{{
                                                            $t(
                                                                'QuestionnaireEditor.Copy'
                                                            )
                                                        }}</a
                                                    >
                                                </li>
                                                <li>
                                                    <a
                                                        v-disabled="
                                                            !readyToPaste
                                                        "
                                                        @click="
                                                            $event => {
                                                                pasteAfterChapter(
                                                                    chapter
                                                                );
                                                                $event.stopPropagation();
                                                            }
                                                        "
                                                        v-if="
                                                            !questionnaire.isReadOnlyForUser &&
                                                                !chapter.isReadOnly
                                                        "
                                                    >
                                                        {{
                                                            $t(
                                                                'QuestionnaireEditor.PasteAfter'
                                                            )
                                                        }}</a
                                                    >
                                                </li>
                                                <li>
                                                    <a
                                                        @click="
                                                            $event => {
                                                                deleteChapter(
                                                                    chapter
                                                                );
                                                                $event.stopPropagation();
                                                            }
                                                        "
                                                        v-if="
                                                            !questionnaire.isReadOnlyForUser &&
                                                                !chapter.isCover
                                                        "
                                                        >{{
                                                            $t(
                                                                'QuestionnaireEditor.Delete'
                                                            )
                                                        }}</a
                                                    >
                                                </li>
                                            </ul>
                                        </div>
                                    </li>
                                </ul>
                                <div class="button-holder">
                                    <button
                                        type="button"
                                        class="btn lighter-hover"
                                        @click="addNewChapter()"
                                        v-if="!questionnaire.isReadOnlyForUser"
                                    >
                                        {{
                                            $t(
                                                'QuestionnaireEditor.AddNewSection'
                                            )
                                        }}
                                    </button>
                                </div>
                            </perfect-scrollbar>
                        </div>
                    </div>
                </div>
            </div>

            <div
                class="left-side-panel scenarios"
                v-class="{ unfolded: isFolded }"
                v-controller="ScenariosCtrl"
            >
                <div
                    class="foldback-region"
                    @click="
                        $event => {
                            foldback();
                            $event.stopPropagation();
                        }
                    "
                ></div>
                <div class="left-side-panel-content macros-panel">
                    <div
                        class="foldback-button-region"
                        @click="
                            $event => {
                                foldback();
                                $event.stopPropagation();
                            }
                        "
                    >
                        <div class="foldback-button"></div>
                    </div>
                    <div class="macroses">
                        <perfect-scrollbar class="scroller">
                            <h3>
                                <!--<span>{{ $t('QuestionnaireEditor.SideBarScenarioCounter',{ count: scenarios.length}) }}</span>-->
                            </h3>
                            <div
                                class="empty-list"
                                v-if="scenarios.length == 0"
                            >
                                <p>
                                    {{
                                        $t(
                                            'QuestionnaireEditor.SideBarScenarioEmptyLine1'
                                        )
                                    }}
                                </p>
                                <p>
                                    {{
                                        $t(
                                            'QuestionnaireEditor.SideBarScenarioEmptyLine2'
                                        )
                                    }}
                                </p>
                            </div>
                            <form role="form" name="scenariosForm" novalidate>
                                <ul v-bind="scenarios">
                                    <li
                                        class="macroses-panel-item"
                                        v-for="scenario in scenarios"
                                    >
                                        <v-form name="scenario.form">
                                            <a
                                                href="javascript:void(0)"
                                                v-if="
                                                    !questionnaire.isReadOnlyForUser
                                                "
                                                @click="deleteScenario($index)"
                                                class="btn delete-btn"
                                                tabindex="-1"
                                            ></a>
                                            <div
                                                class="input-group macroses-name"
                                            >
                                                <input
                                                    focus-on-out="focusScenario{{scenario.id}}"
                                                    :placeholder="
                                                        $t(
                                                            'QuestionnaireEditor.SideBarScenarioName'
                                                        )
                                                    "
                                                    maxlength="32"
                                                    spellcheck="false"
                                                    v-model="scenario.title"
                                                    name="name"
                                                    class="form-control"
                                                    type="text"
                                                />
                                            </div>
                                            <div class="divider"></div>
                                            <button
                                                type="button"
                                                class="btn lighter-hover"
                                                @click="runScenario(scenario)"
                                            >
                                                {{
                                                    $t(
                                                        'QuestionnaireEditor.Run'
                                                    )
                                                }}
                                            </button>
                                            <button
                                                type="button"
                                                class="btn lighter-hover"
                                                @click="
                                                    showScenarioEditor(
                                                        scenario.id,
                                                        scenario.title
                                                    )
                                                "
                                            >
                                                {{
                                                    $t(
                                                        'QuestionnaireEditor.View'
                                                    )
                                                }}
                                            </button>
                                            <div
                                                class="actions"
                                                v-if="
                                                    !questionnaire.isReadOnlyForUser &&
                                                        scenario.form.$dirty
                                                "
                                            >
                                                <button
                                                    type="submit"
                                                    v-disabled="
                                                        questionnaire.isReadOnlyForUser ||
                                                            scenario.form
                                                                .$invalid
                                                    "
                                                    class="btn lighter-hover"
                                                    @click="
                                                        saveScenario(scenario)
                                                    "
                                                >
                                                    {{
                                                        $t(
                                                            'QuestionnaireEditor.Save'
                                                        )
                                                    }}
                                                </button>
                                                <button
                                                    type="button"
                                                    class="btn lighter-hover"
                                                    @click="cancel(scenario)"
                                                >
                                                    {{
                                                        $t(
                                                            'QuestionnaireEditor.Cancel'
                                                        )
                                                    }}
                                                </button>
                                            </div>
                                        </v-form>
                                    </li>
                                </ul>
                            </form>
                        </perfect-scrollbar>
                    </div>
                </div>
            </div>

            <div
                class="left-side-panel macroses"
                v-class="{ unfolded: isFolded }"
                v-controller="MacrosCtrl"
            >
                <div
                    class="foldback-region"
                    @click="
                        $event => {
                            foldback();
                            $event.stopPropagation();
                        }
                    "
                ></div>
                <div class="left-side-panel-content macros-panel">
                    <div
                        class="foldback-button-region"
                        @click="
                            $event => {
                                foldback();
                                $event.stopPropagation();
                            }
                        "
                    >
                        <div class="foldback-button"></div>
                    </div>
                    <div class="macroses">
                        <perfect-scrollbar class="scroller">
                            <h3>
                                <!--<span>{{ $t('QuestionnaireEditor.SideBarMacroCounter',{ count: macros.length}) }}</span>-->
                            </h3>
                            <div class="empty-list" v-if="macros.length == 0">
                                <p>
                                    {{
                                        $t(
                                            'QuestionnaireEditor.SideBarMacroEmptyLine1'
                                        )
                                    }}
                                </p>
                                <p>
                                    {{
                                        $t(
                                            'QuestionnaireEditor.SideBarMacroEmptyLine2'
                                        )
                                    }}
                                </p>
                                <p>
                                    <span class="variable-name">{{
                                        $t('QuestionnaireEditor.VariableName')
                                    }}</span>
                                    {{
                                        $t(
                                            'QuestionnaireEditor.SideBarMacroEmptyLine3'
                                        )
                                    }}
                                </p>
                            </div>
                            <form role="form" name="macrosForm" novalidate>
                                <ul v-bind="macros">
                                    <li
                                        class="macros-panel-item"
                                        v-for="macro in macros"
                                    >
                                        <v-form name="macro.form">
                                            <a
                                                href
                                                @click="deleteMacro($index)"
                                                v-disabled="
                                                    questionnaire.isReadOnlyForUser
                                                "
                                                v-if="
                                                    !questionnaire.isReadOnlyForUser
                                                "
                                                class="btn delete-btn"
                                                tabindex="-1"
                                            ></a>
                                            <div
                                                class="input-group macros-name"
                                            >
                                                <span class="input-group-addon"
                                                    >$</span
                                                >
                                                <input
                                                    focus-on-out="focusMacro{{macro.itemId}}"
                                                    :placeholder="
                                                        $t(
                                                            'QuestionnaireEditor.SideBarMacroName'
                                                        )
                                                    "
                                                    maxlength="32"
                                                    spellcheck="false"
                                                    v-model="macro.name"
                                                    name="name"
                                                    class="form-control"
                                                    type="text"
                                                />
                                            </div>
                                            <div class="divider"></div>
                                            <div
                                                v-bind="macro.content"
                                                ui-ace="{ onLoad : aceLoaded, require: ['ace/ext/language_tools'] }"
                                                type="text"
                                            ></div>
                                            <div
                                                v-if="
                                                    macro.isDescriptionVisible
                                                "
                                            >
                                                <div class="divider"></div>
                                                <textarea
                                                    :placeholder="
                                                        $t(
                                                            'QuestionnaireEditor.SideBarMacroDescription'
                                                        )
                                                    "
                                                    type="text"
                                                    v-model="macro.description"
                                                    class="form-control macros-description"
                                                    msd-elastic
                                                ></textarea>
                                            </div>
                                            <div
                                                class="actions"
                                                v-if="macro.form.$dirty"
                                            >
                                                <button
                                                    type="submit"
                                                    v-disabled="
                                                        questionnaire.isReadOnlyForUser ||
                                                            macro.form.$invalid
                                                    "
                                                    class="btn lighter-hover"
                                                    @click="saveMacro(macro)"
                                                >
                                                    {{
                                                        $t(
                                                            'QuestionnaireEditor.Save'
                                                        )
                                                    }}
                                                </button>
                                                <button
                                                    type="button"
                                                    class="btn lighter-hover"
                                                    @click="cancel(macro)"
                                                >
                                                    {{
                                                        $t(
                                                            'QuestionnaireEditor.Cancel'
                                                        )
                                                    }}
                                                </button>
                                                <button
                                                    class="btn btn-default pull-right"
                                                    v-if="
                                                        isDescriptionEmpty(
                                                            macro
                                                        )
                                                    "
                                                    type="button"
                                                    @click="
                                                        toggleDescription(macro)
                                                    "
                                                >
                                                    {{
                                                        macro.isDescriptionVisible
                                                            ? $t(
                                                                  'QuestionnaireEditor.SideBarMacroHideDescription'
                                                              )
                                                            : $t(
                                                                  'QuestionnaireEditor.SideBarMacroShowDescription'
                                                              )
                                                    }}
                                                </button>
                                            </div>
                                        </v-form>
                                    </li>
                                </ul>
                            </form>
                            <div class="button-holder">
                                <button
                                    type="button"
                                    class="btn lighter-hover"
                                    v-disabled="questionnaire.isReadOnlyForUser"
                                    @click="addNewMacro()"
                                >
                                    {{
                                        $t(
                                            'QuestionnaireEditor.SideBarAddMacro'
                                        )
                                    }}
                                </button>
                            </div>
                        </perfect-scrollbar>
                    </div>
                </div>
            </div>

            <div
                class="left-side-panel lookup-tables"
                v-class="{ unfolded: isFolded }"
                v-controller="LookupTablesCtrl"
            >
                <div
                    class="foldback-region"
                    @click="
                        $event => {
                            foldback();
                            $event.stopPropagation();
                        }
                    "
                ></div>
                <div class="left-side-panel-content lookup-tables-panel">
                    <div
                        class="foldback-button-region"
                        @click="
                            $event => {
                                foldback();
                                $event.stopPropagation();
                            }
                        "
                    >
                        <div class="foldback-button"></div>
                    </div>
                    <div class="lookup-tables">
                        <perfect-scrollbar class="scroller">
                            <h3>
                                <!--<span>{{ $t('QuestionnaireEditor.SideBarLookupTablesCounter',{ count: lookupTables.length}) }}</span>-->
                            </h3>
                            <div
                                class="empty-list"
                                v-if="lookupTables.length === 0"
                            >
                                <p>
                                    {{
                                        $t(
                                            'QuestionnaireEditor.SideBarLookupEmptyLine1'
                                        )
                                    }}
                                </p>
                                <p>
                                    {{
                                        $t(
                                            'QuestionnaireEditor.SideBarLookupEmptyLine2'
                                        )
                                    }}
                                </p>
                                <p>
                                    {{
                                        $t(
                                            'QuestionnaireEditor.SideBarLookupEmptyLine3'
                                        )
                                    }}
                                </p>
                            </div>
                            <form
                                role="form"
                                name="lookupTablesForm"
                                novalidate
                            >
                                <ul v-bind="lookupTables">
                                    <li
                                        class="lookup-table-panel-item"
                                        v-for="table in lookupTables"
                                        ngf-drop=""
                                        ngf-change="fileSelected(table, $file)"
                                        ngf-max-size="1MB"
                                        accept=".tab,.txt"
                                        ngf-drag-over-class="{accept:'dragover', reject:'dragover-err'}"
                                    >
                                        <v-form name="table.form">
                                            <a
                                                href="javascript:void(0);"
                                                @click="
                                                    deleteLookupTable($index)
                                                "
                                                v-disabled="
                                                    questionnaire.isReadOnlyForUser
                                                "
                                                v-if="
                                                    !questionnaire.isReadOnlyForUser
                                                "
                                                class="btn delete-btn"
                                                tabindex="-1"
                                            ></a>
                                            <input
                                                focus-on-out="focusLookupTable{{table.itemId}}"
                                                required=""
                                                :placeholder="
                                                    $t(
                                                        'QuestionnaireEditor.SideBarLookupTableName'
                                                    )
                                                "
                                                maxlength="32"
                                                spellcheck="false"
                                                autocomplete="off"
                                                v-model="table.name"
                                                name="name"
                                                class="form-control table-name"
                                                type="text"
                                            />
                                            <div class="divider"></div>
                                            <input
                                                :placeholder="
                                                    $t(
                                                        'QuestionnaireEditor.SideBarLookupTableFileName'
                                                    )
                                                "
                                                required=""
                                                spellcheck="false"
                                                v-model="table.fileName"
                                                name="fileName"
                                                class="form-control"
                                                disabled=""
                                                type="text"
                                            />

                                            <div class="drop-box">
                                                {{
                                                    $t(
                                                        'QuestionnaireEditor.SideBarLookupTableDropFile'
                                                    )
                                                }}
                                            </div>

                                            <div
                                                class="actions clearfix"
                                                v-class="{
                                                    dirty: table.form.$dirty
                                                }"
                                            >
                                                <div
                                                    v-if="table.form.$dirty"
                                                    class="pull-left"
                                                >
                                                    <button
                                                        type="submit"
                                                        v-disabled="
                                                            questionnaire.isReadOnlyForUser ||
                                                                table.form
                                                                    .$invalid
                                                        "
                                                        class="btn lighter-hover"
                                                        @click="
                                                            $event => {
                                                                saveLookupTable(
                                                                    table
                                                                );
                                                                $event.stopPropagation();
                                                            }
                                                        "
                                                    >
                                                        {{
                                                            $t(
                                                                'QuestionnaireEditor.Save'
                                                            )
                                                        }}
                                                    </button>
                                                    <button
                                                        type="button"
                                                        class="btn lighter-hover"
                                                        @click="
                                                            $event => {
                                                                cancel(table);
                                                                $event.stopPropagation();
                                                            }
                                                        "
                                                    >
                                                        {{
                                                            $t(
                                                                'QuestionnaireEditor.Cancel'
                                                            )
                                                        }}
                                                    </button>
                                                </div>
                                                <div
                                                    class="permanent-actions clearfix"
                                                >
                                                    <a
                                                        href="{{downloadLookupFileBaseUrl + questionnaire.questionnaireId +'?lookupTableId='+ table.itemId}}"
                                                        v-if="
                                                            table.hasUploadedFile
                                                        "
                                                        class="btn btn-default pull-right"
                                                        target="_blank"
                                                    >
                                                        {{
                                                            $t(
                                                                'QuestionnaireEditor.Download'
                                                            )
                                                        }}</a
                                                    >
                                                    <button
                                                        class="btn btn-default pull-right"
                                                        ngf-select=""
                                                        ngf-change="fileSelected(table, $file);$event.stopPropagation()"
                                                        accept=".tab,.txt"
                                                        ngf-max-size="2MB"
                                                        type="file"
                                                    >
                                                        <span
                                                            v-hide="
                                                                table.hasUploadedFile
                                                            "
                                                            >{{
                                                                $t(
                                                                    'QuestionnaireEditor.SideBarLookupTableSelectFile'
                                                                )
                                                            }}</span
                                                        >
                                                        <span
                                                            v-if="
                                                                table.hasUploadedFile
                                                            "
                                                            >{{
                                                                $t(
                                                                    'QuestionnaireEditor.SideBarLookupTableUpdateFile'
                                                                )
                                                            }}</span
                                                        >
                                                    </button>
                                                </div>
                                            </div>
                                        </v-form>
                                    </li>
                                </ul>
                            </form>
                            <div class="button-holder">
                                <button
                                    type="button"
                                    class="btn lighter-hover"
                                    v-disabled="questionnaire.isReadOnlyForUser"
                                    @click="addNewLookupTable()"
                                >
                                    {{
                                        $t(
                                            'QuestionnaireEditor.SideBarLookupTableAdd'
                                        )
                                    }}
                                </button>
                            </div>
                        </perfect-scrollbar>
                    </div>
                </div>
            </div>

            <div
                class="left-side-panel attachments"
                v-class="{ unfolded: isFolded }"
                v-controller="AttachmentsCtrl"
            >
                <div
                    class="foldback-region"
                    @click="
                        $event => {
                            foldback();
                            $event.stopPropagation();
                        }
                    "
                ></div>
                <div class="left-side-panel-content attachments-panel">
                    <div
                        class="foldback-button-region"
                        @click="
                            $event => {
                                foldback();
                                $event.stopPropagation();
                            }
                        "
                    >
                        <div class="foldback-button"></div>
                    </div>
                    <div class="attachments">
                        <perfect-scrollbar class="scroller">
                            <div class="panel-header clearfix">
                                <div class="title pull-left">
                                    <h3>
                                        <!--<span>{{ $t('QuestionnaireEditor.SideBarAttachmentsCounter',{ count: attachments.length, bytes: formatBytes(totalSize()) }) }}</span>-->
                                    </h3>
                                    <p class="estimated-download-time">
                                        <!-- {{ $t( 'QuestionnaireEditor.SideBarAttachmentsEstimate', { timeString: formatSeconds( estimatedLoadingTime())}) }} -->
                                    </p>
                                </div>
                                <button
                                    class="btn btn-default btn-lg pull-left"
                                    v-class="{
                                        'btn-primary': !isReadOnlyForUser
                                    }"
                                    ngf-select
                                    ngf-change="createAndUploadFile($file);$event.stopPropagation()"
                                    ngf-accept="'.pdf,image/*,video/*,audio/*'"
                                    ngf-max-size="100MB"
                                    type="file"
                                    ngf-select-disabled="isReadOnlyForUser"
                                    ngf-drop-disabled="isReadOnlyForUser"
                                    v-disabled="isReadOnlyForUser"
                                >
                                    {{
                                        $t(
                                            'QuestionnaireEditor.SideBarAttachmentsUpload'
                                        )
                                    }}
                                </button>
                            </div>
                            <div
                                class="empty-list"
                                v-if="attachments.length == 0"
                            >
                                <p>
                                    {{
                                        $t(
                                            'QuestionnaireEditor.SideBarAttachmentsEmptyLine1'
                                        )
                                    }}
                                </p>
                                <p>
                                    <span>{{
                                        $t(
                                            'QuestionnaireEditor.SideBarAttachmentsEmptyLine2'
                                        )
                                    }}</span>
                                    <a
                                        href="https://support.mysurvey.solutions/questionnaire-designer/limits/multimedia-reference"
                                        target="_blank"
                                    >
                                        {{
                                            $t('QuestionnaireEditor.ClickHere')
                                        }}
                                    </a>
                                </p>
                                <p v-bind-html="emptyAttachmentsDescription" />
                            </div>
                            <form role="form" name="attachmentsForm" novalidate>
                                <div class="attachment-list">
                                    <v-form
                                        name="attachment.form"
                                        v-for="attachment in attachments"
                                    >
                                        <div
                                            class="attachments-panel-item"
                                            v-class="{
                                                'has-error':
                                                    attachment.form.name.$error
                                                        .pattern
                                            }"
                                            ngf-drop=""
                                            ngf-max-size="100MB"
                                            ngf-change="fileSelected(attachment, $file)"
                                            ngf-drag-over-class="{accept:'dragover', reject:'dragover-err'}"
                                        >
                                            <a
                                                href
                                                @click="
                                                    deleteAttachment($index)
                                                "
                                                v-disabled="
                                                    questionnaire.isReadOnlyForUser
                                                "
                                                v-if="
                                                    !questionnaire.isReadOnlyForUser
                                                "
                                                class="btn delete-btn"
                                                tabindex="-1"
                                            ></a>
                                            <div class="attachment">
                                                <div class="attachment-preview">
                                                    <div
                                                        class="attachment-preview-cover clearfix"
                                                    >
                                                        <img
                                                            class="pull-right"
                                                            @click="
                                                                previewAttachment(
                                                                    attachment
                                                                )
                                                            "
                                                            ngf-size="{width: 156, height: 140}"
                                                            v-src="
                                                                downloadLookupFileBaseUrl +
                                                                    '/' +
                                                                    questionnaire.questionnaireId +
                                                                    '/thumbnail/' +
                                                                    attachment.attachmentId
                                                            "
                                                        />
                                                    </div>
                                                </div>
                                                <div class="attachment-content">
                                                    <input
                                                        focus-on-out="focusAttachment{{attachment.attachmentId}}"
                                                        required=""
                                                        :placeholder="
                                                            $t(
                                                                'QuestionnaireEditor.SideBarAttachmentName'
                                                            )
                                                        "
                                                        maxlength="32"
                                                        spellcheck="false"
                                                        v-model="
                                                            attachment.name
                                                        "
                                                        name="name"
                                                        class="form-control table-name"
                                                        type="text"
                                                    />
                                                    <div class="divider"></div>
                                                    <div class="drop-box">
                                                        {{
                                                            $t(
                                                                'QuestionnaireEditor.SideBarLookupTableDropFile'
                                                            )
                                                        }}
                                                    </div>
                                                    <div
                                                        class="attachment-meta"
                                                        v-include="
                                                            'Image-attachment-info-template.html'
                                                        "
                                                    ></div>
                                                    <div
                                                        class="actions clearfix"
                                                        v-class="{
                                                            dirty:
                                                                attachment.form
                                                                    .$dirty
                                                        }"
                                                    >
                                                        <div
                                                            v-if="
                                                                attachment.form
                                                                    .$dirty
                                                            "
                                                            class="pull-left"
                                                        >
                                                            <button
                                                                type="submit"
                                                                v-disabled="
                                                                    questionnaire.isReadOnlyForUser ||
                                                                        attachment
                                                                            .form
                                                                            .$invalid
                                                                "
                                                                class="btn lighter-hover"
                                                                @click="
                                                                    $event => {
                                                                        saveAttachment(
                                                                            attachment
                                                                        );
                                                                        $event.stopPropagation();
                                                                    }
                                                                "
                                                            >
                                                                {{
                                                                    $t(
                                                                        'QuestionnaireEditor.Save'
                                                                    )
                                                                }}
                                                            </button>
                                                            <button
                                                                type="button"
                                                                class="btn lighter-hover"
                                                                @click="
                                                                    $event => {
                                                                        cancel(
                                                                            attachment
                                                                        );
                                                                        $event.stopPropagation();
                                                                    }
                                                                "
                                                            >
                                                                {{
                                                                    $t(
                                                                        'QuestionnaireEditor.Cancel'
                                                                    )
                                                                }}
                                                            </button>
                                                        </div>
                                                        <div
                                                            class="permanent-actions pull-right clearfix"
                                                        >
                                                            <button
                                                                v-disabled="
                                                                    isReadOnlyForUser
                                                                "
                                                                class="btn btn-default pull-right"
                                                                ngf-select=""
                                                                ngf-accept="'.pdf,image/*,video/*,audio/*'"
                                                                ngf-max-size="100MB"
                                                                ngf-change="fileSelected(attachment, $file);$event.stopPropagation()"
                                                                type="file"
                                                            >
                                                                <span>{{
                                                                    $t(
                                                                        'QuestionnaireEditor.Update'
                                                                    )
                                                                }}</span>
                                                            </button>
                                                            <a
                                                                href="{{downloadLookupFileBaseUrl + '/' + questionnaire.questionnaireId + '/' + attachment.attachmentId}}"
                                                                class="btn btn-default pull-right"
                                                                target="_blank"
                                                            >
                                                                {{
                                                                    $t(
                                                                        'QuestionnaireEditor.Download'
                                                                    )
                                                                }}</a
                                                            >
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

            <div
                class="left-side-panel translations"
                v-class="{ unfolded: isFolded }"
                v-controller="TranslationsCtrl"
            >
                <div
                    class="foldback-region"
                    @click="
                        $event => {
                            foldback();
                            $event.stopPropagation();
                        }
                    "
                ></div>
                <div class="left-side-panel-content translations-panel">
                    <div
                        class="foldback-button-region"
                        @click="
                            $event => {
                                foldback();
                                $event.stopPropagation();
                            }
                        "
                    >
                        <div class="foldback-button"></div>
                    </div>
                    <div class="translations">
                        <perfect-scrollbar class="scroller">
                            <h3>
                                <!--<span>{{ $t('QuestionnaireEditor.SideBarTranslationsCounter',{ count: translations.length}) }}</span>-->
                            </h3>
                            <div
                                class="empty-list"
                                v-if="translations.length == 1"
                            >
                                <p>
                                    {{
                                        $t(
                                            'QuestionnaireEditor.SideBarTranslationsEmptyLine1'
                                        )
                                    }}
                                </p>
                                <p>
                                    {{
                                        $t(
                                            'QuestionnaireEditor.SideBarTranslationsEmptyLine2'
                                        )
                                    }}
                                </p>
                                <p>
                                    {{
                                        $t(
                                            'QuestionnaireEditor.SideBarTranslationsEmptyLine3'
                                        )
                                    }}
                                </p>
                            </div>
                            <form
                                role="form"
                                name="translationsForm"
                                novalidate
                            >
                                <div class="translation-list">
                                    <v-form
                                        name="translation.form"
                                        v-for="translation in translations"
                                    >
                                        <div
                                            class="translations-panel-item"
                                            v-class="{
                                                'has-error':
                                                    translation.form.name.$error
                                                        .pattern
                                            }"
                                            ngf-drop=""
                                            ngf-max-size="4MB"
                                            ngf-change="fileSelected(translation, $file)"
                                            ngf-drag-over-class="{accept:'dragover', reject:'dragover-err'}"
                                        >
                                            <a
                                                href
                                                @click="
                                                    deleteTranslation($index)
                                                "
                                                v-disabled="
                                                    questionnaire.isReadOnlyForUser
                                                "
                                                class="btn delete-btn"
                                                tabindex="-1"
                                                v-if="
                                                    !translation.isOriginalTranslation &&
                                                        !questionnaire.isReadOnlyForUser
                                                "
                                            ></a>
                                            <div class="translation-content">
                                                <input
                                                    focus-on-out="focusTranslation{{translation.translationId}}"
                                                    required=""
                                                    :placeholder="
                                                        $t(
                                                            'QuestionnaireEditor.SideBarTranslationName'
                                                        )
                                                    "
                                                    maxlength="32"
                                                    spellcheck="false"
                                                    v-model="translation.name"
                                                    name="name"
                                                    class="form-control table-name"
                                                    type="text"
                                                />
                                                <div class="drop-box">
                                                    {{
                                                        $t(
                                                            'QuestionnaireEditor.SideBarLookupTableDropFile'
                                                        )
                                                    }}
                                                </div>
                                                <div
                                                    class="actions"
                                                    v-class="{
                                                        dirty:
                                                            translation.form
                                                                .$dirty
                                                    }"
                                                >
                                                    <div
                                                        v-if="
                                                            translation.form
                                                                .$dirty
                                                        "
                                                        class="pull-left"
                                                    >
                                                        <button
                                                            type="submit"
                                                            v-disabled="
                                                                questionnaire.isReadOnlyForUser ||
                                                                    translation
                                                                        .form
                                                                        .$invalid
                                                            "
                                                            class="btn lighter-hover"
                                                            @click="
                                                                onSave(
                                                                    $event,
                                                                    translation
                                                                )
                                                            "
                                                        >
                                                            {{
                                                                $t(
                                                                    'QuestionnaireEditor.Save'
                                                                )
                                                            }}
                                                        </button>
                                                        <button
                                                            type="button"
                                                            class="btn lighter-hover"
                                                            @click="
                                                                onCancel(
                                                                    $event,
                                                                    translation
                                                                )
                                                            "
                                                        >
                                                            {{
                                                                $t(
                                                                    'QuestionnaireEditor.Cancel'
                                                                )
                                                            }}
                                                        </button>
                                                    </div>

                                                    <button
                                                        type="button"
                                                        class="btn btn-default"
                                                        v-if="
                                                            translation.isDefault &&
                                                                !translation.isOriginalTranslation
                                                        "
                                                        @click="
                                                            $event => {
                                                                setDefaultTranslation(
                                                                    $index,
                                                                    false
                                                                );
                                                                $event.stopPropagation();
                                                            }
                                                        "
                                                    >
                                                        {{
                                                            $t(
                                                                'QuestionnaireEditor.UnMarkAsDefault'
                                                            )
                                                        }}
                                                    </button>

                                                    <div
                                                        class="permanent-actions pull-right"
                                                    >
                                                        <button
                                                            type="button"
                                                            class="btn lighter-hover"
                                                            v-disabled="
                                                                isReadOnlyForUser
                                                            "
                                                            v-if="
                                                                !translation.isDefault
                                                            "
                                                            @click="
                                                                $event => {
                                                                    setDefaultTranslation(
                                                                        $index,
                                                                        true
                                                                    );
                                                                    $event.stopPropagation();
                                                                }
                                                            "
                                                        >
                                                            {{
                                                                $t(
                                                                    'QuestionnaireEditor.MarkAsDefault'
                                                                )
                                                            }}
                                                        </button>

                                                        <a
                                                            v-if="
                                                                translation.downloadUrl
                                                            "
                                                            href="{{translation.downloadUrl}}"
                                                            class="btn btn-default"
                                                            target="_blank"
                                                            >{{
                                                                $t(
                                                                    'QuestionnaireEditor.SideBarTranslationDownloadXlsx'
                                                                )
                                                            }}</a
                                                        >

                                                        <button
                                                            v-hide="
                                                                translation.form
                                                                    .$dirty ||
                                                                    translation.isOriginalTranslation
                                                            "
                                                            v-disabled="
                                                                isReadOnlyForUser
                                                            "
                                                            class="btn btn-default"
                                                            ngf-select=""
                                                            ngf-max-size="10MB"
                                                            accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel"
                                                            ngf-change="fileSelected(translation, $file);$event.stopPropagation()"
                                                            type="button"
                                                        >
                                                            <span>{{
                                                                $t(
                                                                    'QuestionnaireEditor.Update'
                                                                )
                                                            }}</span>
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
                                    <span>{{
                                        $t(
                                            'QuestionnaireEditor.SideBarTranslationGetTemplate'
                                        )
                                    }}</span>
                                    <a
                                        class="btn btn-default"
                                        href="{{downloadBaseUrl + '/' + questionnaire.questionnaireId + '/template' }}"
                                        target="_blank"
                                        rel="noopener"
                                    >
                                        {{
                                            $t(
                                                'QuestionnaireEditor.SideBarTranslationGetTemplateLinkTextXlsx'
                                            )
                                        }}
                                    </a>
                                    <a
                                        class="btn btn-default"
                                        href="{{downloadBaseUrl + '/' + questionnaire.questionnaireId + '/templateCsv' }}"
                                        target="_blank"
                                        rel="noopener"
                                    >
                                        {{
                                            $t(
                                                'QuestionnaireEditor.SideBarTranslationGetTemplateLinkTextCsv'
                                            )
                                        }}
                                    </a>
                                </p>
                                <p>
                                    <button
                                        type="button"
                                        v-disabled="isReadOnlyForUser"
                                        class="btn lighter-hover"
                                        ngf-select
                                        ngf-change="createAndUploadFile($file);$event.stopPropagation()"
                                        ngf-max-size="10MB"
                                        accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel"
                                        ngf-select-disabled="isReadOnlyForUser"
                                        ngf-drop-disabled="isReadOnlyForUser"
                                    >
                                        {{
                                            $t(
                                                'QuestionnaireEditor.SideBarTranslationsUploadNew'
                                            )
                                        }}
                                    </button>
                                </p>
                            </div>
                        </perfect-scrollbar>
                    </div>
                </div>
            </div>

            <div
                class="left-side-panel categories"
                v-class="{ unfolded: isFolded }"
                v-controller="CategoriesCtrl"
            >
                <div
                    class="foldback-region"
                    @click="
                        $event => {
                            foldback();
                            $event.stopPropagation();
                        }
                    "
                ></div>
                <div class="left-side-panel-content categories-panel">
                    <div
                        class="foldback-button-region"
                        @click="
                            $event => {
                                foldback();
                                $event.stopPropagation();
                            }
                        "
                    >
                        <div class="foldback-button"></div>
                    </div>
                    <div class="categories">
                        <div
                            id="show-reload-details-promt"
                            class="v-cloak"
                            v-if="shouldUserSeeReloadPromt"
                        >
                            <div class="inner">
                                {{
                                    $t(
                                        'QuestionnaireEditor.QuestionToUpdateOptions'
                                    )
                                }}
                                <a
                                    href="#"
                                    onclick="window.location.reload(true);"
                                    >{{
                                        $t(
                                            'QuestionnaireEditor.QuestionClickReload'
                                        )
                                    }}</a
                                >
                            </div>
                        </div>

                        <perfect-scrollbar class="scroller">
                            <h3>
                                <!-- {{ $t('QuestionnaireEditor.SideBarCategoriesCounter', {count: categoriesList.length}) }} -->
                            </h3>

                            <div
                                class="empty-list"
                                v-if="categoriesList.length == 0"
                            >
                                <p>
                                    {{
                                        $t(
                                            'QuestionnaireEditor.SideBarCategoriesEmptyLine1'
                                        )
                                    }}
                                </p>
                                <p>
                                    {{
                                        $t(
                                            'QuestionnaireEditor.SideBarCategoriesEmptyLine2'
                                        )
                                    }}
                                </p>
                                <p>
                                    <span class="variable-name">{{
                                        $t('QuestionnaireEditor.VariableName')
                                    }}</span>
                                    {{
                                        $t(
                                            'QuestionnaireEditor.SideBarCategoriesEmptyLine3'
                                        )
                                    }}
                                </p>
                            </div>
                            <form role="form" name="categoriesForm" novalidate>
                                <div class="categories-list">
                                    <v-form
                                        name="categories.form"
                                        v-for="categories in categoriesList"
                                    >
                                        <div
                                            class="categories-panel-item"
                                            v-class="{
                                                'has-error':
                                                    categories.form.name.$error
                                                        .pattern
                                            }"
                                            ngf-drop=""
                                            ngf-change="fileSelected(categories, $file)"
                                            ngf-drag-over-class="{accept:'dragover', reject:'dragover-err'}"
                                        >
                                            <a
                                                href
                                                @click="
                                                    deleteCategories($index)
                                                "
                                                class="btn delete-btn"
                                                tabindex="-1"
                                                v-if="
                                                    !questionnaire.isReadOnlyForUser
                                                "
                                            ></a>
                                            <div class="categories-content">
                                                <input
                                                    focus-on-out="focusCategories{{categories.categoriesId}}"
                                                    required=""
                                                    :placeholder="
                                                        $t(
                                                            'QuestionnaireEditor.SideBarCategoriesName'
                                                        )
                                                    "
                                                    maxlength="32"
                                                    spellcheck="false"
                                                    v-model="categories.name"
                                                    name="name"
                                                    class="form-control table-name"
                                                    type="text"
                                                />
                                                <div class="drop-box">
                                                    {{
                                                        $t(
                                                            'QuestionnaireEditor.SideBarLookupTableDropFile'
                                                        )
                                                    }}
                                                </div>
                                                <div
                                                    class="actions"
                                                    v-class="{
                                                        dirty:
                                                            categories.form
                                                                .$dirty
                                                    }"
                                                >
                                                    <div
                                                        v-if="
                                                            categories.form
                                                                .$dirty
                                                        "
                                                        class="pull-left"
                                                    >
                                                        <button
                                                            type="submit"
                                                            v-disabled="
                                                                categories.form
                                                                    .$invalid
                                                            "
                                                            class="btn lighter-hover"
                                                            @click="
                                                                $event => {
                                                                    saveCategories(
                                                                        categories
                                                                    );
                                                                    $event.stopPropagation();
                                                                }
                                                            "
                                                        >
                                                            {{
                                                                $t(
                                                                    'QuestionnaireEditor.Save'
                                                                )
                                                            }}
                                                        </button>
                                                        <button
                                                            type="button"
                                                            class="btn lighter-hover"
                                                            @click="
                                                                $event => {
                                                                    cancel(
                                                                        categories
                                                                    );
                                                                    $event.stopPropagation();
                                                                }
                                                            "
                                                        >
                                                            {{
                                                                $t(
                                                                    'QuestionnaireEditor.Cancel'
                                                                )
                                                            }}
                                                        </button>
                                                    </div>

                                                    <div
                                                        class="permanent-actions pull-right"
                                                    >
                                                        <a
                                                            href="javascript:void(0);"
                                                            class="btn btn-link"
                                                            @click="
                                                                editCategories(
                                                                    questionnaire.questionnaireId,
                                                                    categories.categoriesId
                                                                )
                                                            "
                                                        >
                                                            {{
                                                                $t(
                                                                    'QuestionnaireEditor.SideBarEditCategories'
                                                                )
                                                            }}
                                                        </a>
                                                        <button
                                                            v-hide="
                                                                categories.form
                                                                    .$dirty
                                                            "
                                                            v-disabled="
                                                                isReadOnlyForUser
                                                            "
                                                            class="btn btn-default"
                                                            ngf-select=""
                                                            ngf-max-size="10MB"
                                                            accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel,.txt,.tsv,.tab"
                                                            ngf-change="fileSelected(categories, $file);$event.stopPropagation()"
                                                            type="button"
                                                        >
                                                            <span>{{
                                                                $t(
                                                                    'QuestionnaireEditor.Update'
                                                                )
                                                            }}</span>
                                                        </button>
                                                        {{
                                                            $t(
                                                                'QuestionnaireEditor.SideBarDownload'
                                                            )
                                                        }}
                                                        <a
                                                            href="{{'/questionnaire/ExportOptions/' + questionnaire.questionnaireId  + '?type=xlsx&isCategory=true&entityId=' + categories.categoriesId}}"
                                                            class="btn btn-default"
                                                            target="_blank"
                                                            rel="noopener"
                                                            >{{
                                                                $t(
                                                                    'QuestionnaireEditor.SideBarXlsx'
                                                                )
                                                            }}</a
                                                        >
                                                        <a
                                                            href="{{'/questionnaire/ExportOptions/' + questionnaire.questionnaireId  + '?type=csv&isCategory=true&entityId=' + categories.categoriesId}}"
                                                            class="btn btn-default"
                                                            target="_blank"
                                                            rel="noopener"
                                                            >{{
                                                                $t(
                                                                    'QuestionnaireEditor.SideBarTab'
                                                                )
                                                            }}</a
                                                        >
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </v-form>
                                </div>
                            </form>
                            <div class="button-holder">
                                <p>
                                    <span>{{
                                        $t(
                                            'QuestionnaireEditor.SideBarTranslationGetTemplate'
                                        )
                                    }}</span>

                                    <a
                                        class="btn btn-default"
                                        href="{{downloadBaseUrl + '/template' }}"
                                        target="_blank"
                                        rel="noopener"
                                    >
                                        {{
                                            $t(
                                                'QuestionnaireEditor.SideBarXlsx'
                                            )
                                        }}
                                    </a>
                                    <a
                                        class="btn btn-default"
                                        href="{{downloadBaseUrl + '/templateTab' }}"
                                        target="_blank"
                                        rel="noopener"
                                    >
                                        {{
                                            $t('QuestionnaireEditor.SideBarTab')
                                        }}
                                    </a>
                                </p>
                                <p>
                                    <button
                                        type="button"
                                        class="btn lighter-hover"
                                        @click="
                                            $event => {
                                                addNewCategory();
                                                $event.stopPropagation();
                                            }
                                        "
                                        v-disabled="isReadOnlyForUser"
                                    >
                                        {{
                                            $t(
                                                'QuestionnaireEditor.SideBarCategoriesAddNew'
                                            )
                                        }}
                                    </button>
                                </p>
                                <p>
                                    <button
                                        type="button"
                                        class="btn lighter-hover"
                                        ngf-select
                                        ngf-change="createAndUploadFile($file);$event.stopPropagation()"
                                        ngf-max-size="10MB"
                                        accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel,.txt,.tsv,.tab"
                                        ngf-select-disabled="isReadOnlyForUser"
                                        ngf-drop-disabled="isReadOnlyForUser"
                                        v-disabled="isReadOnlyForUser"
                                    >
                                        {{
                                            $t(
                                                'QuestionnaireEditor.SideBarCategoriesUploadNew'
                                            )
                                        }}
                                    </button>
                                </p>
                            </div>
                        </perfect-scrollbar>
                    </div>
                </div>
            </div>

            <div
                class="left-side-panel metadata"
                v-class="{ unfolded: isFolded }"
                v-controller="MetadataCtrl"
            >
                <div
                    class="foldback-region"
                    @click="
                        $event => {
                            foldback();
                            $event.stopPropagation();
                        }
                    "
                ></div>
                <div class="left-side-panel-content metadata-panel">
                    <div
                        class="foldback-button-region"
                        @click="
                            $event => {
                                foldback();
                                $event.stopPropagation();
                            }
                        "
                    >
                        <div class="foldback-button"></div>
                    </div>

                    <div class="metadata">
                        <perfect-scrollbar class="scroller">
                            <h3>
                                {{
                                    $t(
                                        'QuestionnaireEditor.SideBarMetadataHeader'
                                    )
                                }}
                            </h3>

                            <form role="form" name="metadataForm" novalidate>
                                <v-form name="metadata.form">
                                    <ul class="list-unstyled metadata-blocks">
                                        <li>
                                            <h4>
                                                {{
                                                    $t(
                                                        'QuestionnaireEditor.SideBarMetadataBasicInfo'
                                                    )
                                                }}
                                            </h4>
                                            <div class="field-wrapper">
                                                <span class="label-title">
                                                    {{
                                                        $t(
                                                            'QuestionnaireEditor.SideBarMetadataTitle'
                                                        )
                                                    }}
                                                </span>
                                                <div class="form-group">
                                                    <input
                                                        type="text"
                                                        class="form-control"
                                                        name="title"
                                                        required=""
                                                        minlength="1"
                                                        v-model="metadata.title"
                                                    />
                                                </div>
                                            </div>
                                            <div class="field-wrapper">
                                                <span class="label-title">
                                                    {{
                                                        $t(
                                                            'QuestionnaireEditor.SideBarMetadataSubtitle'
                                                        )
                                                    }}
                                                </span>
                                                <div class="form-group">
                                                    <textarea
                                                        class="form-control msd-elastic"
                                                        name="subTitle"
                                                        v-model="
                                                            metadata.subTitle
                                                        "
                                                    ></textarea>
                                                </div>
                                            </div>
                                            <div class="field-wrapper">
                                                <span class="label-title">
                                                    {{
                                                        $t(
                                                            'QuestionnaireEditor.SideBarMetadataVersionIdentificator'
                                                        )
                                                    }}
                                                </span>
                                                <div class="form-group">
                                                    <input
                                                        type="text"
                                                        class="form-control"
                                                        name="version"
                                                        v-model="
                                                            metadata.version
                                                        "
                                                    />
                                                </div>
                                            </div>
                                            <div class="field-wrapper">
                                                <span class="label-title">{{
                                                    $t(
                                                        'QuestionnaireEditor.SideBarMetadataVersionNotes'
                                                    )
                                                }}</span>
                                                <div class="form-group">
                                                    <textarea
                                                        class="form-control msd-elastic"
                                                        name="versionNotes"
                                                        v-model="
                                                            metadata.versionNotes
                                                        "
                                                    ></textarea>
                                                </div>
                                            </div>
                                        </li>
                                        <li>
                                            <h4>
                                                {{
                                                    $t(
                                                        'QuestionnaireEditor.SideBarMetadataSurveyDataInfo'
                                                    )
                                                }}
                                            </h4>
                                            <div class="field-wrapper">
                                                <span class="label-title">
                                                    {{
                                                        $t(
                                                            'QuestionnaireEditor.SideBarMetadataStudyTypes'
                                                        )
                                                    }}
                                                </span>
                                                <div class="form-group">
                                                    <div
                                                        class="btn-group dropdown"
                                                        v-class="{
                                                            'has-value':
                                                                metadata.studyType
                                                        }"
                                                    >
                                                        <button
                                                            class="btn btn-default dropdown-toggle"
                                                            type="button"
                                                            data-toggle="dropdown"
                                                            aria-haspopup="true"
                                                            aria-expanded="true"
                                                        >
                                                            <!-- {{ metadata.studyType
                                                            ? (questionnaire.studyTypes | filter : { 'code': metadata.studyType})[0].title
                                                            : $t('QuestionnaireEditor.SelectStudyType')
                                                        }} -->
                                                            <span
                                                                class="dropdown-arrow"
                                                                aria-labelledby="dropdownMenu12"
                                                            ></span>
                                                        </button>
                                                        <button
                                                            type="button"
                                                            class="btn btn-link btn-clear"
                                                            @click="
                                                                v => {
                                                                    metadata.studyType = null;
                                                                    metadata.form.$setDirty();
                                                                }
                                                            "
                                                        >
                                                            <span></span>
                                                        </button>
                                                        <div
                                                            class="dropdown-menu"
                                                            aria-labelledby="dropdownMenu12"
                                                        >
                                                            <perfect-scrollbar
                                                                class="scroller"
                                                            >
                                                                <ul
                                                                    class="list-unstyled"
                                                                >
                                                                    <li
                                                                        v-for="studyType in questionnaire.studyTypes"
                                                                    >
                                                                        <a
                                                                            @click="
                                                                                v => {
                                                                                    metadata.studyType =
                                                                                        studyType.code;
                                                                                    metadata.form.$setDirty();
                                                                                }
                                                                            "
                                                                            value="{{studyType.code}}"
                                                                            href="#"
                                                                            >{{
                                                                                studyType.title
                                                                            }}</a
                                                                        >
                                                                    </li>
                                                                </ul>
                                                            </perfect-scrollbar>
                                                        </div>
                                                    </div>
                                                    <input
                                                        type="hidden"
                                                        class="form-control"
                                                        name="studyType"
                                                        v-model="
                                                            metadata.studyType
                                                        "
                                                    />
                                                </div>
                                            </div>
                                            <div class="field-wrapper">
                                                <span class="label-title">
                                                    {{
                                                        $t(
                                                            'QuestionnaireEditor.SideBarMetadataKindOfData'
                                                        )
                                                    }}</span
                                                >
                                                <div class="form-group">
                                                    <div
                                                        class="btn-group dropdown"
                                                        v-class="{
                                                            'has-value':
                                                                metadata.kindOfData
                                                        }"
                                                    >
                                                        <button
                                                            class="btn btn-default dropdown-toggle"
                                                            type="button"
                                                            data-toggle="dropdown"
                                                            aria-haspopup="true"
                                                            aria-expanded="true"
                                                        >
                                                            <!-- {{ metadata.kindOfData
                                                            ? (questionnaire.kindsOfData | filter : {
                                                                'code':
                                                                    metadata.kindOfData
                                                            })[0].title
                                                                                                                : $t('QuestionnaireEditor.SelectKindOfData')
                                                        }} -->
                                                            <span
                                                                class="dropdown-arrow"
                                                                aria-labelledby="dropdownMenu12"
                                                            ></span>
                                                        </button>
                                                        <button
                                                            type="button"
                                                            class="btn btn-link btn-clear"
                                                            @click="
                                                                v => {
                                                                    metadata.kindOfData = null;
                                                                    metadata.form.$setDirty();
                                                                }
                                                            "
                                                        >
                                                            <span></span>
                                                        </button>
                                                        <div
                                                            class="dropdown-menu "
                                                            aria-labelledby="dropdownMenu12"
                                                        >
                                                            <ul
                                                                class="scroller list-unstyled"
                                                            >
                                                                <li
                                                                    v-for="kindOfData in questionnaire.kindsOfData"
                                                                >
                                                                    <a
                                                                        @click="
                                                                            v => {
                                                                                metadata.kindOfData =
                                                                                    kindOfData.code;
                                                                                metadata.form.$setDirty();
                                                                            }
                                                                        "
                                                                        value="{{kindOfData.code}}"
                                                                        href="#"
                                                                        >{{
                                                                            kindOfData.title
                                                                        }}</a
                                                                    >
                                                                </li>
                                                            </ul>
                                                        </div>
                                                    </div>
                                                </div>
                                                <input
                                                    type="hidden"
                                                    class="form-control"
                                                    name="kindOfData"
                                                    v-model="
                                                        metadata.kindOfData
                                                    "
                                                />
                                            </div>
                                            <div class="field-wrapper">
                                                <span class="label-title">
                                                    {{
                                                        $t(
                                                            'QuestionnaireEditor.SideBarMetadataModeOfDataCollection'
                                                        )
                                                    }}
                                                </span>
                                                <div class="form-group">
                                                    <div
                                                        class="btn-group dropdown"
                                                        v-class="{
                                                            'has-value':
                                                                metadata.modeOfDataCollection
                                                        }"
                                                    >
                                                        <button
                                                            class="btn btn-default dropdown-toggle"
                                                            type="button"
                                                            data-toggle="dropdown"
                                                            aria-haspopup="true"
                                                            aria-expanded="true"
                                                        >
                                                            <!-- {{ metadata.modeOfDataCollection ?
                                                            (questionnaire.modesOfDataCollection | filter :
                                                            { 'code': metadata.modeOfDataCollection })[0].title :
                                                        $t('QuestionnaireEditor.SelectModeOfDataCollection')
                                                        }} -->
                                                            <span
                                                                class="dropdown-arrow"
                                                                aria-labelledby="dropdownMenu12"
                                                            ></span>
                                                        </button>
                                                        <button
                                                            type="button"
                                                            class="btn btn-link btn-clear"
                                                            @click="
                                                                v => {
                                                                    metadata.modeOfDataCollection = null;
                                                                    metadata.form.$setDirty();
                                                                }
                                                            "
                                                        >
                                                            <span></span>
                                                        </button>
                                                        <div
                                                            class="dropdown-menu "
                                                            aria-labelledby="dropdownMenu12"
                                                        >
                                                            <ul
                                                                class="scroller list-unstyled"
                                                            >
                                                                <li
                                                                    v-for="modeOfData in questionnaire.modesOfDataCollection"
                                                                >
                                                                    <a
                                                                        @click="
                                                                            v => {
                                                                                metadata.modeOfDataCollection =
                                                                                    modeOfData.code;
                                                                                metadata.form.$setDirty();
                                                                            }
                                                                        "
                                                                        value="{{modeOfData.code}}"
                                                                        href="#"
                                                                        >{{
                                                                            modeOfData.title
                                                                        }}</a
                                                                    >
                                                                </li>
                                                            </ul>
                                                        </div>
                                                    </div>
                                                </div>
                                                <input
                                                    type="hidden"
                                                    class="form-control"
                                                    id="modeOfDataCollection"
                                                    name="modeOfDataCollection"
                                                    v-model="
                                                        metadata.modeOfDataCollection
                                                    "
                                                />
                                            </div>
                                        </li>
                                        <li>
                                            <h4>
                                                {{
                                                    $t(
                                                        'QuestionnaireEditor.SideBarMetadataSurveyInfo'
                                                    )
                                                }}
                                            </h4>
                                            <div class="field-wrapper">
                                                <span class="label-title">{{
                                                    $t(
                                                        'QuestionnaireEditor.SideBarMetadataSurveyInfo'
                                                    )
                                                }}</span>
                                                <div class="form-group">
                                                    <div
                                                        class="btn-group dropdown"
                                                        v-class="{
                                                            'has-value':
                                                                metadata.country
                                                        }"
                                                    >
                                                        <button
                                                            class="btn btn-default dropdown-toggle"
                                                            type="button"
                                                            data-toggle="dropdown"
                                                            aria-haspopup="true"
                                                            aria-expanded="true"
                                                        >
                                                            <!-- {{ metadata.country
                                                            ? (questionnaire.countries | filter : {
                                                                'code': metadata.country
                                                            })[0].title
                                                                                                                : $t('QuestionnaireEditor.SelectCountry')
                                                        }} -->
                                                            <span
                                                                class="dropdown-arrow"
                                                                aria-labelledby="dropdownMenu12"
                                                            ></span>
                                                        </button>
                                                        <button
                                                            type="button"
                                                            class="btn btn-link btn-clear"
                                                            @click="
                                                                v => {
                                                                    metadata.country = null;
                                                                    metadata.form.$setDirty();
                                                                }
                                                            "
                                                        >
                                                            <span></span>
                                                        </button>
                                                        <div
                                                            class="dropdown-menu"
                                                            aria-labelledby="dropdownMenu12"
                                                        >
                                                            <perfect-scrollbar
                                                                class="scroller"
                                                            >
                                                                <ul
                                                                    class="list-unstyled"
                                                                >
                                                                    <li
                                                                        v-for="country in questionnaire.countries"
                                                                    >
                                                                        <a
                                                                            @click="
                                                                                v => {
                                                                                    metadata.country =
                                                                                        country.code;
                                                                                    metadata.form.$setDirty();
                                                                                }
                                                                            "
                                                                            value="{{country.code}}"
                                                                            href="#"
                                                                            >{{
                                                                                country.title
                                                                            }}</a
                                                                        >
                                                                    </li>
                                                                </ul>
                                                            </perfect-scrollbar>
                                                        </div>
                                                    </div>
                                                </div>
                                                <input
                                                    type="hidden"
                                                    class="form-control"
                                                    name="country"
                                                    v-model="metadata.country"
                                                />
                                            </div>
                                            <div class="field-wrapper">
                                                <span class="label-title">{{
                                                    $t(
                                                        'QuestionnaireEditor.SideBarMetadataYear'
                                                    )
                                                }}</span>
                                                <div class="form-group">
                                                    <input
                                                        type="text"
                                                        min="0"
                                                        max="9999"
                                                        pattern="\d*"
                                                        onkeypress="return event.charCode >= 48 && event.charCode <= 57"
                                                        maxlength="4"
                                                        v-pattern="/^\d+$/"
                                                        class="form-control date-field"
                                                        name="year"
                                                        v-model="metadata.year"
                                                    />
                                                </div>
                                                <p class="help-block v-cloak">
                                                    <!-- v-if="metadata.form.year.$error.pattern" -->
                                                    {{
                                                        $t(
                                                            'QuestionnaireEditor.QuestionOnlyInts'
                                                        )
                                                    }}
                                                </p>
                                            </div>
                                            <div class="field-wrapper">
                                                <span class="label-title">{{
                                                    $t(
                                                        'QuestionnaireEditor.SideBarMetadataLanguages'
                                                    )
                                                }}</span>
                                                <div class="form-group">
                                                    <input
                                                        type="text"
                                                        class="form-control"
                                                        name="language"
                                                        v-model="
                                                            metadata.language
                                                        "
                                                    />
                                                </div>
                                            </div>
                                            <div class="field-wrapper">
                                                <span class="label-title">{{
                                                    $t(
                                                        'QuestionnaireEditor.SideBarMetadataUnitOfAlalysis'
                                                    )
                                                }}</span>
                                                <div class="form-group">
                                                    <textarea
                                                        class="form-control msd-elastic"
                                                        name="unitOfAnalysis"
                                                        v-model="
                                                            metadata.unitOfAnalysis
                                                        "
                                                    ></textarea>
                                                </div>
                                            </div>
                                            <div class="field-wrapper">
                                                <span class="label-title">{{
                                                    $t(
                                                        'QuestionnaireEditor.SideBarMetadataCoverage'
                                                    )
                                                }}</span>
                                                <div class="form-group">
                                                    <textarea
                                                        class="form-control msd-elastic"
                                                        name="coverage"
                                                        v-model="
                                                            metadata.coverage
                                                        "
                                                    ></textarea>
                                                </div>
                                            </div>
                                            <div class="field-wrapper">
                                                <span class="label-title">{{
                                                    $t(
                                                        'QuestionnaireEditor.SideBarMetadataUniverse'
                                                    )
                                                }}</span>
                                                <div class="form-group">
                                                    <textarea
                                                        class="form-control msd-elastic"
                                                        name="universe"
                                                        v-model="
                                                            metadata.universe
                                                        "
                                                    ></textarea>
                                                </div>
                                            </div>
                                            <div class="field-wrapper">
                                                <span class="label-title">
                                                    {{
                                                        $t(
                                                            'QuestionnaireEditor.SideBarMetadataPrimaryInvestigator'
                                                        )
                                                    }}
                                                </span>
                                                <div class="form-group">
                                                    <textarea
                                                        class="form-control msd-elastic"
                                                        name="primaryInvestigator"
                                                        v-model="
                                                            metadata.primaryInvestigator
                                                        "
                                                    ></textarea>
                                                </div>
                                            </div>
                                            <div class="field-wrapper">
                                                <span class="label-title">{{
                                                    $t(
                                                        'QuestionnaireEditor.SideBarMetadataConsultants'
                                                    )
                                                }}</span>
                                                <div class="form-group">
                                                    <textarea
                                                        class="form-control msd-elastic"
                                                        name="consultant"
                                                        v-model="
                                                            metadata.consultant
                                                        "
                                                    >
                                                    </textarea>
                                                </div>
                                            </div>
                                            <div class="field-wrapper">
                                                <span class="label-title">
                                                    {{
                                                        $t(
                                                            'QuestionnaireEditor.SideBarMetadataFunding'
                                                        )
                                                    }}
                                                </span>
                                                <div class="form-group">
                                                    <textarea
                                                        class="form-control msd-elastic"
                                                        name="funding"
                                                        v-model="
                                                            metadata.funding
                                                        "
                                                    >
                                                    </textarea>
                                                </div>
                                            </div>
                                        </li>
                                        <li>
                                            <h4>
                                                {{
                                                    $t(
                                                        'QuestionnaireEditor.SideBarMetadataAdditionalInfo'
                                                    )
                                                }}
                                            </h4>
                                            <div class="field-wrapper">
                                                <span class="label-title">
                                                    {{
                                                        $t(
                                                            'QuestionnaireEditor.SideBarMetadataNotes'
                                                        )
                                                    }}
                                                </span>
                                                <div class="form-group">
                                                    <textarea
                                                        class="form-control msd-elastic"
                                                        name="notes"
                                                        v-model="metadata.notes"
                                                    ></textarea>
                                                </div>
                                            </div>
                                            <div class="field-wrapper">
                                                <span class="label-title">
                                                    {{
                                                        $t(
                                                            'QuestionnaireEditor.SideBarMetadataKeywords'
                                                        )
                                                    }}
                                                </span>
                                                <div class="form-group">
                                                    <textarea
                                                        class="form-control msd-elastic"
                                                        name="keywords"
                                                        v-model="
                                                            metadata.keywords
                                                        "
                                                    ></textarea>
                                                </div>
                                            </div>
                                        </li>
                                        <li>
                                            <h4>
                                                {{
                                                    $t(
                                                        'QuestionnaireEditor.SideBarMetadataQuestionnaireAccess'
                                                    )
                                                }}
                                            </h4>
                                            <div class="checkbox">
                                                <input
                                                    id="agreeToMakeThisQuestionnairePublic"
                                                    name="agreeToMakeThisQuestionnairePublic"
                                                    v-model="
                                                        metadata.agreeToMakeThisQuestionnairePublic
                                                    "
                                                    type="checkbox"
                                                    class="checkbox-filter"
                                                    checked
                                                />
                                                <label
                                                    for="agreeToMakeThisQuestionnairePublic"
                                                    class=""
                                                    ><span class="tick"></span>
                                                    {{
                                                        $t(
                                                            'QuestionnaireEditor.SideBarMetadataAgreeToMakeThisQuestionnairePublic'
                                                        )
                                                    }}
                                                </label>
                                            </div>
                                        </li>
                                    </ul>
                                    <!-- v-class="{ dirty: metadata.form.$dirty }" -->
                                    <div class="form-buttons-holder">
                                        <!-- v-disabled="questionnaire.isReadOnlyForUser || metadata.form.$invalid" -->
                                        <!-- v-class="{ 'btn-primary': metadata.form.$dirty }" -->
                                        <button
                                            type="submit"
                                            class="btn btn-lg v-isolate-scope"
                                            @click="
                                                $event => {
                                                    saveMetadata();
                                                    $event.stopPropagation();
                                                }
                                            "
                                        >
                                            {{ $t('QuestionnaireEditor.Save') }}
                                        </button>
                                        <button
                                            type="button"
                                            class="btn btn-lg btn-link v-isolate-scope"
                                            @click="
                                                $event => {
                                                    cancelMetadata();
                                                    $event.stopPropagation();
                                                }
                                            "
                                        >
                                            {{
                                                $t('QuestionnaireEditor.Cancel')
                                            }}
                                        </button>
                                    </div>
                                </v-form>
                            </form>
                        </perfect-scrollbar>
                    </div>
                </div>
            </div>

            <div
                class="left-side-panel comments"
                v-class="{ unfolded: isFolded }"
                v-controller="CommentsCtrl"
                data-empty-place-holder-enabled="false"
            >
                <div
                    class="foldback-region"
                    @click="
                        $event => {
                            foldback();
                            $event.stopPropagation();
                        }
                    "
                ></div>
                <div class="left-side-panel-content comments-panel">
                    <div
                        class="foldback-button-region"
                        @click="
                            $event => {
                                foldback();
                                $event.stopPropagation();
                            }
                        "
                    >
                        <div class="foldback-button"></div>
                    </div>
                    <div class="comments">
                        <perfect-scrollbar class="scroller">
                            <h3>
                                <span>
                                    <!-- {{
                                    $t(
                                        'QuestionnaireEditor.SideBarCommentsCounter', { count: commentThreads.length }
                                    )
                                }} -->
                                </span>
                            </h3>
                            <div
                                class="empty-list"
                                v-if="commentThreads.length == 0"
                            >
                                <p>
                                    {{
                                        $t(
                                            'QuestionnaireEditor.SideBarEmptyCommentsLine'
                                        )
                                    }}
                                </p>
                            </div>
                            <ul v-bind="commentThreads">
                                <li
                                    class="comment-thread"
                                    v-for="commentThread in commentThreads"
                                >
                                    <a
                                        class="reference-item"
                                        href="javascript:void(0);"
                                        @click="
                                            showCommentsAndNavigateTo(
                                                commentThread.entity
                                            )
                                        "
                                    >
                                        <span
                                            v-if="
                                                commentThread.entity.type ==
                                                    'Question'
                                            "
                                            class="icon {{commentThread.entity.questionType}} "
                                        ></span>
                                        <span
                                            v-if="
                                                commentThread.entity.type !==
                                                    'Question' &&
                                                    commentThread.entity
                                                        .type !== 'Group' &&
                                                    commentThread.entity
                                                        .type !== 'Roster'
                                            "
                                            class="icon icon-{{commentThread.entity.type.toLowerCase()}}"
                                        ></span>
                                        <span class="title">{{
                                            commentThread.entity.title | escape
                                        }}</span>
                                        <span
                                            class="variable"
                                            v-bind-html="commentThread.entity.variable || '&nbsp;'">
                                            &nbsp;
                                        </span>
                                    </a>
                                    <div class="comments-in-thread">
                                        <ul>
                                            <li
                                                class="comment"
                                                v-class="{
                                                    resolved: comment.isResolved
                                                }"
                                                v-for="comment in commentThread.comments"
                                            >
                                                <span class="author">{{
                                                    comment.userEmail
                                                }}</span>
                                                <span class="date">{{
                                                    comment.date
                                                }}</span>
                                                <p class="comment-text">
                                                    {{ comment.comment }}
                                                </p>
                                            </li>
                                        </ul>
                                        <div
                                            v-if="
                                                commentThread.resolvedComments
                                                    .length > 0
                                            "
                                        >
                                            <a
                                                href="javascript:void(0);"
                                                class="show-more"
                                                @click="
                                                    commentThread.toggleResolvedComments()
                                                "
                                            >
                                                <span
                                                    v-hide="
                                                        commentThread.resolvedAreExpanded
                                                    "
                                                >
                                                    <!-- {{
    $t(
        'QuestionnaireEditor.ViewResolvedCommentsCounter', { count: commentThread.resolvedComments.length }
    )
}} -->
                                                </span>
                                                <span
                                                    v-if="
                                                        commentThread.resolvedAreExpanded
                                                    "
                                                    >{{
                                                        $t(
                                                            'QuestionnaireEditor.HideResolvedComments'
                                                        )
                                                    }}</span
                                                >
                                            </a>
                                            <ul
                                                v-if="
                                                    commentThread.resolvedAreExpanded
                                                "
                                            >
                                                <li
                                                    class="comment"
                                                    v-class="{
                                                        resolved:
                                                            comment.isResolved
                                                    }"
                                                    v-for="comment in commentThread.resolvedComments"
                                                >
                                                    <span class="author">{{
                                                        comment.userEmail
                                                    }}</span>
                                                    <span class="date">{{
                                                        comment.date
                                                    }}</span>
                                                    <p class="comment-text">
                                                        {{ comment.comment }}
                                                    </p>
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
                    <li>
                        <a
                            class="left-menu-chapters"
                            v-class="{ unfolded: isUnfoldedChapters }"
                            @click="unfoldChapters()"
                            :title="
                                $t('QuestionnaireEditor.SideBarSectionsTitle')
                            "
                        ></a>
                    </li>
                    <li>
                        <a
                            class="left-menu-metadata"
                            v-class="{ unfolded: isUnfoldedMetadata }"
                            @click="unfoldMetadata()"
                            :title="
                                $t('QuestionnaireEditor.SideBarMetadataTitle')
                            "
                        ></a>
                    </li>
                    <li>
                        <a
                            class="left-menu-translations"
                            v-class="{ unfolded: isUnfoldedTranslations }"
                            @click="unfoldTranslations()"
                            :title="
                                $t(
                                    'QuestionnaireEditor.SideBarTranslationsTitle'
                                )
                            "
                        ></a>
                    </li>
                    <li>
                        <a
                            class="left-menu-categories"
                            v-class="{ unfolded: isUnfoldedCategories }"
                            @click="unfoldCategories()"
                            :title="
                                $t('QuestionnaireEditor.SideBarCategoriesTitle')
                            "
                        ></a>
                    </li>
                    <li>
                        <a
                            class="left-menu-scenarios"
                            v-if="questionnaire.questionnaireRevision === null"
                            v-class="{ unfolded: isUnfoldedScenarios }"
                            @click="unfoldScenarios()"
                            :title="
                                $t('QuestionnaireEditor.SideBarScenarioTitle')
                            "
                        ></a>
                    </li>
                    <li>
                        <a
                            class="left-menu-macroses"
                            v-class="{ unfolded: isUnfoldedMacros }"
                            @click="unfoldMacros()"
                            :title="$t('QuestionnaireEditor.SideBarMacroTitle')"
                        ></a>
                    </li>
                    <li>
                        <a
                            class="left-menu-lookupTables"
                            v-class="{ unfolded: isUnfoldedLookupTables }"
                            @click="unfoldLookupTables()"
                            :title="
                                $t('QuestionnaireEditor.SideBarLookupTitle')
                            "
                        ></a>
                    </li>
                    <li>
                        <a
                            class="left-menu-attachments"
                            v-class="{ unfolded: isUnfoldedAttachments }"
                            @click="unfoldAttachments()"
                            :title="
                                $t(
                                    'QuestionnaireEditor.SideBarAttachmentsTitle'
                                )
                            "
                        ></a>
                    </li>
                    <li>
                        <a
                            class="left-menu-comments"
                            v-if="!questionnaire.isReadOnlyForUser"
                            v-class="{ unfolded: isUnfoldedComments }"
                            @click="unfoldComments()"
                            :title="
                                $t('QuestionnaireEditor.SideBarCommentsTitle')
                            "
                        ></a>
                    </li>
                </ul>
            </div>

            <div class="questionnaire-tree" ui-view></div>
        </section>
    </div>
</template>

<script>
import questionnaireService from '../../services/questionnaireService.js';
import userService from '../../services/userService.js';
import moment from 'moment';
import _ from 'lodash';

export default {
    name: 'Main',
    props: {
        questionnaireRev: { type: String, required: true }
    },
    data() {
        return {
            questionnaire: {
                questionsCount: 0,
                groupsCount: 0,
                rostersCount: 0,
                chapters: []
            },
            currentUserIsAuthenticated: false,
            isReadOnlyForUser: true,
            verificationStatus: {
                errors: null,
                warnings: null,
                visible: false,
                time: new Date()
            },
            lookupTables: [],
            scenarios: [],
            macros: [],
            commentThreads: [],
            attachments: [],
            translations: [],
            categoriesList: [],
            metadata: {},

            isUnfoldedChapters: false,
            isUnfoldedScenarios: false,
            isUnfoldedMacros: false,
            isUnfoldedLookupTables: false,
            isUnfoldedAttachments: false,
            isUnfoldedTranslations: false,
            isUnfoldedMetadata: false,
            isUnfoldedComments: false,
            isUnfoldedCategories: false,

            currentUserName: '',
            currentUserEmail: '',
            currentUserIsAuthenticated: false,

            benchmarkDownloadSpeed: 20
        };
    },
    methods: {
        showDownloadPdf() {
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
        exportQuestionnaire() {},
        showShareInfo() {},
        verify() {},
        showVerificationErrors() {},
        showVerificationWarnings() {},
        addNewCategory() {
            if (this.isReadOnlyForUser) {
                notificationService.notice($i18next.t('NoPermissions'));
                return;
            }

            var categories = { categoriesId: utilityService.guid() };

            commandService
                .updateCategories($state.params.questionnaireId, categories)
                .then(function(response) {
                    if (response.status !== 200) return;

                    categories.checkpoint = categories.checkpoint || {};

                    dataBind(categories.checkpoint, categories);
                    $scope.categoriesList.push(categories);
                    updateQuestionnaireCategories();

                    setTimeout(function() {
                        utilityService.focus(
                            'focusCategories' + categories.categoriesId
                        );
                    }, 500);
                })
                .catch(function() {});
        },
        unfoldMetadata() {},
        unfoldChapters() {},
        unfoldCategories() {},
        unfoldComments() {},
        unfoldTranslations() {},
        unfoldAttachments() {},
        webTest() {},

        foldback() {},

        addNewChapter() {
            // var newId = utilityService.guid();
            // var newChapter = {
            //     title: $i18next.t('DefaultNewSection'),
            //     itemId: newId,
            //     itemType: "Chapter",
            //     isCover: false
            // };
            // commandService.addChapter($state.params.questionnaireId, newChapter).then(function () {
            //     $scope.questionnaire.chapters.push(newChapter);
            //     $state.go('questionnaire.chapter.group', { chapterId: newChapter.itemId, itemId: newChapter.itemId });
            //     $rootScope.$emit('groupAdded');
            // });
        },
        isCurrentChapter(chapter) {
            return false;
            //return chapter.itemId === $state.params.chapterId;
        },

        formatSeconds(seconds) {
            return moment.duration(seconds).humanize();
        },

        loadMacros() {
            if (
                this.questionnaire === null ||
                this.questionnaire.macros === null
            )
                return;

            _.each(this.questionnaire.macros, function(macroDto) {
                var macro = { checkpoint: {} };
                if (
                    !_.any(this.macros, function(elem) {
                        return elem.itemId === macroDto.itemId;
                    })
                ) {
                    //dataBind(macro, macroDto);
                    //dataBind(macro.checkpoint, macroDto);
                    this.macros.push(macro);
                }
            });
        },

        totalSize() {
            // return _.reduce(this.attachments, function (sum, attachment) {
            //     return sum + (attachment.content.size || 0);
            // }, 0);
        },

        estimatedLoadingTime() {
            return 0;
            //return Math.floor(this.totalSize() / this.benchmarkDownloadSpeed);
        },

        dataBindMetadata(metadata, metadataDto) {
            metadata.title = metadataDto.title;
            metadata.subTitle = metadataDto.subTitle;
            metadata.studyType = metadataDto.studyType;
            metadata.version = metadataDto.version;
            metadata.versionNotes = metadataDto.versionNotes;
            metadata.kindOfData = metadataDto.kindOfData;
            metadata.country = metadataDto.country;
            metadata.year = metadataDto.year;
            metadata.language = metadataDto.language;
            metadata.coverage = metadataDto.coverage;
            metadata.universe = metadataDto.universe;
            metadata.unitOfAnalysis = metadataDto.unitOfAnalysis;
            metadata.primaryInvestigator = metadataDto.primaryInvestigator;
            metadata.funding = metadataDto.funding;
            metadata.consultant = metadataDto.consultant;
            metadata.modeOfDataCollection = metadataDto.modeOfDataCollection;
            metadata.notes = metadataDto.notes;
            metadata.keywords = metadataDto.keywords;
            metadata.agreeToMakeThisQuestionnairePublic =
                metadataDto.agreeToMakeThisQuestionnairePublic;
        },

        loadMetadata() {
            if (
                this.questionnaire === null ||
                this.questionnaire.metadata === null
            )
                return;

            this.dataBindMetadata(this.metadata, this.questionnaire.metadata);
            this.metadata.checkpoint = {};
            this.dataBindMetadata(
                this.metadata.checkpoint,
                this.questionnaire.metadata
            );
        },

        async getCurrentUserName() {
            const result = await userService.getCurrentUserName();
            this.currentUserName = result.userName;
        },

        async getQuestionnaire() {
            const data = await questionnaireService.getQuestionnaireById(
                this.questionnaireRev
            );
            this.questionnaire = data;

            /* questionnaireService.getQuestionnaireById($state.params.questionnaireId).then(function (result) {
                    this.questionnaire = result.data;
                    if (!$state.params.chapterId && result.data.chapters.length > 0) {
                        var defaultChapter = _.first(result.data.chapters);
                        var itemId = defaultChapter.itemId;
                        $scope.currentChapter = defaultChapter;
                        $state.go('questionnaire.chapter.group', { chapterId: itemId, itemId: itemId });
                    }

                    $rootScope.$emit('questionnaireLoaded');
                });*/

            this.loadMacros();

            this.loadMetadata();
        }
    },
    async mounted() {
        await this.getCurrentUserName();
        await this.getQuestionnaire();
    }
};
</script>
