<template>
    <div role="tabpanel" class="tab-pane page-preview-block" id="finish">
        <Form v-slot="{ errors, meta }" ref="finishPage" class="" :data-vv-scope="'finishPage'" @submit="dummy">
            <div class="d-flex f-row">
                <div class="costomization-block">
                    <div class="row-element mb-30">
                        <div class="h5">
                            {{ $t('WebInterviewSettings.Title') }}
                        </div>
                        <div class="form-group" :class="{ 'has-error': errors.webSurveyHeader }">
                            <div class="field"
                                :class="{ 'answered': webInterviewPageMessages['webSurveyHeader'].text }">
                                <Field as="textarea" v-autosize
                                    v-model="webInterviewPageMessages['webSurveyHeader'].text" rules="required"
                                    name="webSurveyHeader" data-vv-name="webSurveyHeader" ref="webSurveyHeader"
                                    :min-height="77" maxlength="200" class="form-control js-elasticArea font-bold"
                                    placeholder="Please enter the main text">
                                </Field>
                                <button type="button" @click="webInterviewPageMessages['webSurveyHeader'].text = ''"
                                    class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                                <span class="help-block" v-if="errors.webSurveyHeader">{{
                                    $t('WebInterviewSettings.FieldRequired') }}</span>
                            </div>
                        </div>
                    </div>
                    <div class="row-element mb-30">
                        <div class="h5">
                            {{ $t('WebInterviewSettings.Description') }}
                        </div>

                        <Field v-slot="{ field }" name="finishInterviewDescription"
                            :value="webInterviewPageMessages['finishInterview'].text">

                            <md-editor ref="finishInterview" v-bind="field" data-vv-name="finishInterview"
                                v-model="webInterviewPageMessages['finishInterview'].text">
                            </md-editor>

                        </Field>

                    </div>
                    <div class="">
                        <button type="submit" :disabled="!meta.dirty ? 'disabled' : null"
                            @click="savePageTextEditMode($refs.finishPage, 'webSurveyHeader', 'finishInterview')"
                            class="btn btn-md btn-success">
                            {{ $t('WebInterviewSettings.Save') }}
                        </button>
                        <button type="submit" :disabled="!meta.dirty ? 'disabled' : null"
                            @click="cancelPageTextEditMode($refs.finishPage, 'webSurveyHeader', 'finishInterview')"
                            class="btn btn-md btn-link">
                            {{ $t('WebInterviewSettings.Cancel') }}
                        </button>
                    </div>
                </div>
                <div class="preview">
                    <div class="browser-mockup mb-30">
                        <div class="mockup-block d-flex f-row">
                            <div class="icon" />
                            <div class="text-example">
                                <div class="row-element mb-30">
                                    <Logo :hasLogo="hasLogo" :logoUrl="logoUrl" />
                                </div>
                                <div class="row-element mb-20">
                                    <div class="h2">
                                        {{ previewText(webInterviewPageMessages['webSurveyHeader'].text) }}
                                    </div>
                                </div>
                                <div class="row-element mb-40">
                                    <p v-dompurify-html="previewHtml(webInterviewPageMessages['finishInterview'].text)">
                                    </p>
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
import { Form, Field, ErrorMessage } from 'vee-validate'

export default {
    components: {
        Form,
        Field,
        ErrorMessage,
    },
    mixins: [settings],
    mounted() {
        this.$refs.finishPage.resetForm({ values: this.$refs.finishPage.values })
    }
}
</script>