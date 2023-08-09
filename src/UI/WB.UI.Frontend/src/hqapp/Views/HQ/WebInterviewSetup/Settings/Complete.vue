<template>
    <div role="tabpanel" class="tab-pane page-preview-block" id="complete">
        <form class="" :data-vv-scope="'completePage'" v-on:submit.prevent="dummy">
            <div class="d-flex f-row">
                <div class="costomization-block">
                    <div class="row-element mb-30">
                        <div class="h5">
                            {{ $t('WebInterviewSettings.NoteToSupervisor') }}
                        </div>
                        <div class="form-group" :class="{ 'has-error': errors.has('completePage.noteToSupervisor') }">
                            <div class="field"
                                :class="{ 'answered': webInterviewPageMessages['completeNoteToSupervisor'].text }">
                                <textarea-autosize v-model="webInterviewPageMessages['completeNoteToSupervisor'].text"
                                    v-validate="'required'" data-vv-name="completeNoteToSupervisor"
                                    ref="completeNoteToSupervisor" :min-height="77" maxlength="200"
                                    class="form-control js-elasticArea font-bold">
                                </textarea-autosize>
                                <button type="button"
                                    @click="webInterviewPageMessages['completeNoteToSupervisor'].text = ''"
                                    class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                                <span class="help-block" v-if="errors.first('completePage.noteToSupervisor')">{{
                                    $t('WebInterviewSettings.FieldRequired') }}</span>
                            </div>
                        </div>
                    </div>
                    <div class="row-element mb-30">
                        <div class="h5">
                            {{ $t('WebInterviewSettings.Complete') }}
                        </div>
                        <div class="form-group" :class="{ 'has-error': errors.has('completePage.completeButton') }">
                            <div class="field" :class="{ 'answered': webInterviewPageMessages['completeButton'].text }">
                                <input type="text" v-model="webInterviewPageMessages['completeButton'].text"
                                    v-validate="'required'" data-vv-name="completeButton" ref="completeButton"
                                    maxlength="200" class="form-control" />
                                <button type="button" @click="webInterviewPageMessages['completeButton'].text = ''"
                                    class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                                <span class="help-block" v-if="errors.first('completePage.completeButton')">{{
                                    $t('WebInterviewSettings.FieldRequired') }}</span>
                            </div>
                        </div>
                    </div>
                    <div class="">
                        <button type="submit" :disabled="!isDirty('$completePage')"
                            @click="savePageTextEditMode('completePage', 'completeButton', 'completeNoteToSupervisor')"
                            class="btn btn-md btn-success">
                            {{ $t('WebInterviewSettings.Save') }}
                        </button>
                        <button type="submit" :disabled="!isDirty('$completePage')"
                            @click="cancelPageTextEditMode('completePage', 'completeButton', 'completeNoteToSupervisor')"
                            class="btn btn-md btn-link">
                            {{ $t('WebInterviewSettings.Cancel') }}
                        </button>
                    </div>
                </div>
                <div class="preview">
                    <div class="browser-mockup mb-30">
                        <div class="mockup-block d-flex f-row">
                            <div class="text-example">
                                <div class="row-element mb-20">
                                    <div class="h2">
                                        {{ $t('WebInterviewUI.CompleteAbout') }}
                                    </div>
                                </div>
                                <div class="row-element mb-40">
                                    <div class="h2 gray-uppercase">
                                        {{ previewText(webInterviewPageMessages['completeNoteToSupervisor'].text) }}
                                    </div>
                                    <input type="text" :placeholder="$t('WebInterviewUI.TextEnter')" class="form-control" />
                                </div>
                                <div class="row-element mb-40">
                                    <a href="javascript:void(0);" class="btn btn-success btn-lg">
                                        {{ previewText(webInterviewPageMessages['completeButton'].text) }}
                                    </a>
                                </div>
                                <div class="row-element">
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </form>
    </div>
</template>

<script>
import settings from './settingsMixin'

export default {
    mixins: [settings],
    mounted() {
        this.$validator.reset('completePage')
    }
}
</script>