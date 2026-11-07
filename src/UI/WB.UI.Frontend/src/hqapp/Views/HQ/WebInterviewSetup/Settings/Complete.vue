<template>
    <div role="tabpanel" class="tab-pane page-preview-block" id="complete">
        <Form v-slot="{ errors, meta }" ref="completePage" class="" :data-vv-scope="'completePage'" @submit="dummy">
            <div class="d-flex f-row">
                <div class="costomization-block">
                    <div class="row-element mb-30">
                        <div class="h5">
                            {{ $t('WebInterviewSettings.CompleteScreenText') }}
                        </div>
                        <div class="form-group" :class="{ 'has-error': errors.completeScreenText }">
                            <div class="field"
                                :class="{ 'answered': webInterviewPageMessages['completeScreenText'].text }">
                                <Field v-slot="{ handleChange }"
                                    v-model="completeScreenTextMarkdown"
                                    name="completeScreenText"
                                    rules="max:2000">
                                    <md-editor
                                        ref="completeScreenText"
                                        :model-value="webInterviewPageMessages['completeScreenText'].text"
                                        @update:model-value="updateCompleteScreenText($event, handleChange)"
                                        data-vv-name="completeScreenText"
                                        :aria-label="$t('WebInterviewSettings.CompleteScreenText')">
                                    </md-editor>
                                </Field>
                                <button type="button"
                                    @click="webInterviewPageMessages['completeScreenText'].text = ''"
                                    :aria-label="$t('Common.Clear')"
                                    class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                            </div>
                            <span class="help-block" v-if="errors.completeScreenText">{{ errors.completeScreenText }}</span>
                        </div>
                    </div>
                    <div class="row-element mb-30">
                        <div class="h5">
                            {{ $t('WebInterviewSettings.NoteToSupervisor') }}
                        </div>
                        <div class="form-group" :class="{ 'has-error': errors.completeNoteToSupervisor }">
                            <div class="field"
                                :class="{ 'answered': webInterviewPageMessages['completeNoteToSupervisor'].text }">
                                <Field as="textarea" v-autosize
                                    v-model="webInterviewPageMessages['completeNoteToSupervisor'].text" rules="required"
                                    name="completeNoteToSupervisor" data-vv-name="completeNoteToSupervisor"
                                    ref="completeNoteToSupervisor" :min-height="77" maxlength="200"
                                    class="form-control js-elasticArea font-bold">
                                </Field>
                                <button type="button"
                                    @click="webInterviewPageMessages['completeNoteToSupervisor'].text = ''"
                                    class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                                <span class="help-block" v-if="errors.completeNoteToSupervisor">{{
                                    $t('WebInterviewSettings.FieldRequired') }}</span>
                            </div>
                        </div>
                    </div>
                    <div class="row-element mb-30">
                        <div class="h5">
                            {{ $t('WebInterviewSettings.Complete') }}
                        </div>
                        <div class="form-group" :class="{ 'has-error': errors.completeButton }">
                            <div class="field" :class="{ 'answered': webInterviewPageMessages['completeButton'].text }">
                                <Field type="text" v-model="webInterviewPageMessages['completeButton'].text"
                                    rules="required" name="completeButton" data-vv-name="completeButton"
                                    ref="completeButton" maxlength="200" class="form-control" />
                                <button type="button" @click="webInterviewPageMessages['completeButton'].text = ''"
                                    class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                                <span class="help-block" v-if="errors.completeButton">{{
                                    $t('WebInterviewSettings.FieldRequired') }}</span>
                            </div>
                        </div>
                    </div>
                    <div class="">
                        <button type="submit" :disabled="!meta.dirty ? 'disabled' : null"
                            @click="savePageTextEditMode($refs.completePage, 'completeButton', 'completeScreenText', 'completeNoteToSupervisor')"
                            class="btn btn-md btn-success">
                            {{ $t('WebInterviewSettings.Save') }}
                        </button>
                        <button type="submit" :disabled="!meta.dirty ? 'disabled' : null"
                            @click="cancelPageTextEditMode($refs.completePage, 'completeButton', 'completeScreenText', 'completeNoteToSupervisor')"
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
                                        {{ $t('WebInterviewUI.CompleteReviewSubmit') }}
                                    </div>
                                </div>
                                <div class="row-element mb-20" v-if="webInterviewPageMessages['completeScreenText'].text"
                                    v-dompurify-html="previewCompleteScreenText(webInterviewPageMessages['completeScreenText'].text)">
                                </div>
                                <div class="row-element mb-40">
                                    <div class="h2 info-block gray-uppercase">
                                        {{ previewText(webInterviewPageMessages['completeNoteToSupervisor'].text) }}
                                    </div>
                                    <input type="text" :placeholder="$t('WebInterviewUI.TextEnter')"
                                        class="form-control" />
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
        </Form>
    </div>
</template>

<script>
import settings from './settingsMixin'
import { Form, Field } from 'vee-validate'
import { escape, unescape } from 'lodash-es'

export default {
    components: {
        Form,
        Field,
    },
    mixins: [settings],
    computed: {
        completeScreenTextMarkdown: {
            get() {
                return unescape(this.webInterviewPageMessages['completeScreenText'].text)
            },
            set(value) {
                this.webInterviewPageMessages['completeScreenText'].text = escape(value)
            },
        },
    },
    methods: {
        previewCompleteScreenText(text) {
            return this.previewHtml(unescape(text))
        },
        updateCompleteScreenText(value, handleChange) {
            handleChange(unescape(value))
        },
    },
    mounted() {
        this.$refs.completePage.resetForm({ values: this.$refs.completePage.values })
    },
}
</script>