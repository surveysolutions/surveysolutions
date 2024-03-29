<template>
    <div id="search-modal" ref="searchModal" class="modal findReplaceModal fade in" v-if="visible"
        style="display:block; z-index:1050;" v-dragAndDrop>
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header" style="cursor: default;">
                    <button type="button" class="close" aria-hidden="true" @click="close()"> </button>
                    <h3 class="modal-title"> {{ $t('QuestionnaireEditor.FindReplaceTitle') }} </h3>
                </div>
                <div class="modal-body">
                    <div>
                        <form class="form-horizontal" v-if="step == 'search'">
                            <div class="form-group">
                                <label for="searchFor" class="col-sm-3 control-label wb-label">
                                    {{ $t('QuestionnaireEditor.FindReplaceFindWhat') }}
                                </label>
                                <div class="col-sm-9">
                                    <input type="text" maxlength="500" class="form-control" id="searchFor"
                                        v-model="searchForm.searchFor" on-enter="findAll()" autocomplete="off">
                                </div>
                            </div>
                            <div class="form-group">
                                <label for="relaceWith" class="col-sm-3 control-label wb-label">
                                    {{ $t('QuestionnaireEditor.FindReplaceReplaceWith') }}
                                </label>
                                <div class="col-sm-9">
                                    <input type="text" maxlength="500" class="form-control" id="relaceWith"
                                        v-model="searchForm.replaceWith" autocomplete="off">
                                </div>
                            </div>
                            <div class="form-group">
                                <div class="checkbox">
                                    <input id="cb-match-case" type="checkbox" class="wb-checkbox"
                                        v-model="searchForm.matchCase" />
                                    <label for="cb-match-case" class="wb-label">
                                        <span></span>
                                        {{ $t('QuestionnaireEditor.FindReplaceMatchCase') }}
                                    </label>
                                </div>
                                <div class="checkbox">
                                    <input id="cb-match-wholeword" type="checkbox" class="wb-checkbox"
                                        v-model="searchForm.matchWholeWord" />
                                    <label for="cb-match-wholeword" class="wb-label">
                                        <span></span>
                                        {{ $t('QuestionnaireEditor.FindReplaceMatchWord') }}
                                    </label>
                                </div>
                                <div class="checkbox">
                                    <input id="cb-use-regexp" type="checkbox" class="wb-checkbox"
                                        v-model="searchForm.useRegex" />
                                    <label for="cb-use-regexp" class="wb-label">
                                        <span></span>
                                        {{ $t('QuestionnaireEditor.FindReplaceUseRegex') }}
                                    </label>
                                </div>
                            </div>
                        </form>

                        <div v-if="step == 'confirm'">
                            <p>{{ $t('QuestionnaireEditor.FindReplaceСonfirm') }}</p>
                            <p>
                                {{ $t('QuestionnaireEditor.FindReplaceReplaceAllConfirm') }}
                                "<strong>{{ searchForm.searchFor }}</strong>"
                                {{ $t('QuestionnaireEditor.FindReplaceReplaceAllConfirmWith') }}
                                "<strong>{{ searchForm.replaceWith }}</strong>"
                            </p>
                            <p> {{ $t('QuestionnaireEditor.FindReplaceInAllFound', { count: foundReferences.length }) }}
                            </p>
                        </div>
                        <div v-if="step == 'done'">
                            {{ $t('QuestionnaireEditor.FindReplaceInAll') }}
                            "<strong>{{ searchForm.searchFor }}</strong>"
                            {{ $t('QuestionnaireEditor.FindReplaceWereReplaced') }}
                            "<strong>{{ searchForm.replaceWith }}</strong>"
                        </div>
                    </div>

                </div>
                <div class="modal-footer">
                    <div>
                        <div v-if="step == 'search'">
                            <p class="wb-label">
                                {{ $t('QuestionnaireEditor.FindReplaceMatchingLines', { count: foundReferences.length })
                                }}
                            </p>
                            <button type="button" class="btn btn-lg btn-primary"
                                :disabled="searchForm.searchFor.length === 0" @click="findAll()">{{
        $t('QuestionnaireEditor.FindReplaceFindAll') }}</button>
                            <button type="button" class="btn btn-lg btn-primary" v-if="!isReadOnlyForUser"
                                :disabled="foundReferences.length === 0" @click="confirmReplaceAll()">
                                {{ $t('QuestionnaireEditor.FindReplaceReplaceAll', { count: foundReferences.length }) }}
                            </button>

                            <div class="pull-right" v-if="foundReferences.length">
                                <button type="button" class="btn btn-lg btn-link" @click="navigatePrev()"> {{
        $t('QuestionnaireEditor.FindReplacePrevious') }}
                                </button>
                                <button type="button" class="btn btn-lg btn-link" @click="navigateNext()">{{
        $t('QuestionnaireEditor.FindReplaceNext') }}</button>
                            </div>
                        </div>
                        <div v-if="step == 'confirm'">
                            <button type="button" class="btn btn-lg btn-primary"
                                :disabled="foundReferences.length === 0" @click="replaceAll()">
                                {{ $t('QuestionnaireEditor.FindReplaceReplaceAll', { count: foundReferences.length }) }}
                            </button>
                            <button type="button" class="btn btn-lg btn-link" @click="backToSearch()"> {{
        $t('QuestionnaireEditor.FindReplaceBackToSearch') }}</button>
                        </div>
                        <div v-if="step == 'done'">
                            <button type="button" class="btn btn-lg btn-primary" @click="onDone()">{{
                                $t('QuestionnaireEditor.FindReplaceDone') }}</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import { replaceAll, findAll } from '../../../services/searchService'
import { setFocusIn } from '../../../services/utilityService'

export default {
    name: 'SearchDialog',
    props: {
        questionnaireId: { type: String, required: true }
    },
    data() {
        return {
            searchForm: {
                searchFor: '',
                replaceWith: '',
                matchCase: false,
                matchWholeWord: false,
                useRegex: false,
            },
            foundReferences: [],
            indexOfCurrentReference: -1,
            visible: false,
            step: 'search',
        };
    },
    expose: ['open', 'close'],
    mounted() {
        this.$emitter.on('openMacrosList', this.openLeftPanel);
    },
    unmounted() {
        this.$emitter.off('openMacrosList', this.openLeftPanel);
    },
    methods: {
        close() {
            this.visible = false;
        },
        open() {

            this.searchForm.searchFor = '';
            this.searchForm.replaceWith = '';
            this.searchForm.matchCase = false;
            this.searchForm.matchWholeWord = false;
            this.searchForm.useRegex = false;

            this.step = 'search';
            this.foundReferences = [];

            this.visible = true;

            setFocusIn('searchFor');
        },
        onDone() {
            this.close();
            window.location.reload();
        },
        backToSearch() {
            this.step = 'search';
            this.foundReferences.splice(0, this.foundReferences.length);
        },
        confirmReplaceAll() {
            this.step = 'confirm';
        },
        replaceAll() {
            replaceAll(this.questionnaireId, this.searchForm).then(() => {
                this.step = 'done';
            });
        },

        navigateNext() {
            this.indexOfCurrentReference++;
            if (this.indexOfCurrentReference >= this.foundReferences.length)
                this.indexOfCurrentReference = 0;

            const reference = this.foundReferences[this.indexOfCurrentReference];

            const name = reference.type.toLowerCase();
            if (name === "macro") {
                this.$emitter.emit("openMacrosList", { focusOn: reference.itemId });
            }
            else {
                this.$router.push({
                    name: name,
                    params: {
                        chapterId: reference.chapterId,
                        entityId: reference.itemId,
                    },
                    force: true,
                    state: {
                        indexOfEntityInProperty: reference.indexOfEntityInProperty,
                        property: reference.property
                    }
                });
            }

        },
        navigatePrev() {
            this.indexOfCurrentReference--;
            if (this.indexOfCurrentReference < 0) {
                this.indexOfCurrentReference = this.foundReferences.length - 1;
            }

            const reference = this.foundReferences[this.indexOfCurrentReference];
            const name = reference.type.toLowerCase();

            if (name === "macro") {
                this.$emitter.emit("openMacrosList", { focusOn: reference.itemId });
            }
            else {
                this.$router.push({
                    name: name,
                    params: {
                        chapterId: reference.chapterId,
                        entityId: reference.itemId,
                    },
                    force: true,
                    state: {
                        indexOfEntityInProperty: reference.indexOfEntityInProperty,
                        property: reference.property
                    }
                });
            }

        },

        async findAll() {
            if (this.searchForm.searchFor.length > 0) {
                const seachResults = await findAll(this.questionnaireId, this.searchForm);
                this.foundReferences = seachResults;
                this.indexOfCurrentReference = -1;
            }
        },

        openLeftPanel() {
            const modal = this.$refs.searchModal;
            if (modal) {
                const rect = modal.getBoundingClientRect();
                if (rect.left < 700)
                    modal.style.left = '700px'
            }
        }
    }
};
</script>
