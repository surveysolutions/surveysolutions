<template>
    <div role="tabpanel" class="tab-pane page-preview-block" id="finish">
        <form class="" :data-vv-scope="'finishPage'" v-on:submit.prevent="dummy">
            <div class="d-flex f-row">
                <div class="costomization-block">
                    <div class="row-element mb-30">
                        <div class="h5">
                            {{ $t('WebInterviewSettings.Title') }}
                        </div>
                        <div class="form-group" :class="{ 'has-error': errors.has('finishPage.webSurveyHeader') }">
                            <div class="field" :class="{ 'answered': webInterviewPageMessages['webSurveyHeader'].text }">
                                <textarea-autosize v-model="webInterviewPageMessages['webSurveyHeader'].text"
                                    v-validate="'required'" data-vv-name="webSurveyHeader" ref="webSurveyHeader"
                                    :min-height="77" maxlength="200" class="form-control js-elasticArea font-bold"
                                    placeholder="Please enter the main text">
                                </textarea-autosize>
                                <button type="button" @click="webInterviewPageMessages['webSurveyHeader'].text = ''"
                                    class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                                <span class="help-block"
                                    v-if="errors.first('finishPage.webSurveyHeader')">{{ $t('WebInterviewSettings.FieldRequired') }}</span>
                            </div>
                        </div>
                    </div>
                    <div class="row-element mb-30">
                        <div class="h5">
                            {{ $t('WebInterviewSettings.Description') }}
                        </div>
                        <md-editor ref="finishInterview" v-validate="" data-vv-name="finishInterview"
                            v-model="webInterviewPageMessages['finishInterview'].text">
                        </md-editor>

                    </div>
                    <div class="">
                        <button type="submit" :disabled="!isDirty('$finishPage')"
                            @click="savePageTextEditMode('finishPage', 'webSurveyHeader', 'finishInterview')"
                            class="btn btn-md btn-success">
                            {{ $t('WebInterviewSettings.Save') }}
                        </button>
                        <button type="submit" :disabled="!isDirty('$finishPage')"
                            @click="cancelPageTextEditMode('finishPage', 'webSurveyHeader', 'finishInterview')"
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
                                    <p v-html="previewHtml(webInterviewPageMessages['finishInterview'].text)"></p>
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
        this.$validator.reset('finishPage')
    }
}
</script>