<template>
    <div role="tabpanel" class="tab-pane page-preview-block" id="resume">
        <form class="" :data-vv-scope="'resumePage'" v-on:submit.prevent="dummy">
            <div class="d-flex f-row">
                <div class="costomization-block">
                    <div class="row-element mb-30">
                        <div class="h5">
                            {{ $t('WebInterviewSettings.Title') }}
                        </div>
                        <div class="form-group" :class="{ 'has-error': errors.has('resumePage.resumeWelcome') }">
                            <div class="field" :class="{ 'answered': webInterviewPageMessages['resumeWelcome'].text }">
                                <textarea-autosize v-model="webInterviewPageMessages['resumeWelcome'].text"
                                    v-validate="'required'" data-vv-name="resumeWelcome" ref="resumeWelcome"
                                    :min-height="77" maxlength="200" class="form-control js-elasticArea font-bold"
                                    placeholder="Please enter the main text">
                                </textarea-autosize>
                                <button type="button" @click="webInterviewPageMessages['resumeWelcome'].text = ''"
                                    class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                                <span class="help-block"
                                    v-if="errors.first('resumePage.resumeWelcome')">{{ $t('WebInterviewSettings.FieldRequired') }}</span>
                            </div>
                        </div>
                    </div>
                    <div class="row-element mb-30">
                        <div class="h5">
                            {{ $t('WebInterviewSettings.Description') }}
                        </div>
                        <md-editor ref="resumeInvitation" v-validate="" data-vv-name="resumeInvitation"
                            v-model="webInterviewPageMessages['resumeInvitation'].text">
                        </md-editor>

                    </div>

                    <div class="row-element mb-30">
                        <div class="h5">
                            {{ $t('WebInterviewSettings.Resume') }}
                        </div>
                        <div class="form-group" :class="{ 'has-error': errors.has('resumePage.resumeButton') }">
                            <div class="field" :class="{ 'answered': webInterviewPageMessages['resumeButton'].text }">
                                <input type="text" v-model="webInterviewPageMessages['resumeButton'].text"
                                    v-validate="'required'" data-vv-name="resumeButton" ref="resumeButton" maxlength="200"
                                    class="form-control" />
                                <button type="button" @click="webInterviewPageMessages['resumeButton'].text = ''"
                                    class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                                <span class="help-block"
                                    v-if="errors.first('resumePage.resumeButton')">{{ $t('WebInterviewSettings.FieldRequired') }}</span>
                            </div>
                        </div>
                    </div>

                    <div class="">
                        <button type="submit" :disabled="!isDirty('$resumePage')"
                            @click="savePageTextEditMode('resumePage', 'resumeWelcome', 'resumeInvitation', 'resumeButton')"
                            class="btn btn-md btn-success">
                            {{ $t('WebInterviewSettings.Save') }}
                        </button>
                        <button type="submit" :disabled="!isDirty('$resumePage')"
                            @click="cancelPageTextEditMode('resumePage', 'resumeWelcome', 'resumeInvitation', 'resumeButton')"
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
                                        {{ previewText(webInterviewPageMessages['resumeWelcome'].text) }}
                                    </div>
                                </div>
                                <div class="row-element mb-40">
                                    <p v-html="previewHtml(webInterviewPageMessages['resumeInvitation'].text)"></p>
                                </div>
                                <div class="row-element">
                                    <a href="javascript:void(0);" class="btn btn-success btn-lg mb-1">
                                        {{ previewText(webInterviewPageMessages['resumeButton'].text) }}
                                    </a>
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
        this.$validator.reset('resumePage')
    }
}
</script>