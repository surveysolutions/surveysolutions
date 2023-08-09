<template>
    <div role="tabpanel" class="tab-pane active page-preview-block" id="welcome">
        <form class="" :data-vv-scope="'welcomePage'" v-on:submit.prevent="dummy">
            <div class="d-flex f-row">
                <div class="costomization-block">
                    <div class="row-element mb-30">
                        <div class="h5">
                            {{ $t('WebInterviewSettings.Title') }}
                        </div>
                        <div class="form-group" :class="{ 'has-error': errors.has('welcomePage.welcomeTextTitle') }">
                            <div class="field" :class="{ 'answered': webInterviewPageMessages['welcomeText'].text }">
                                <textarea-autosize v-model="webInterviewPageMessages['welcomeText'].text"
                                    v-validate="'required'" data-vv-name="welcomeTextTitle" ref="welcomeTextTitle"
                                    :min-height="77" maxlength="200" class="form-control js-elasticArea font-bold"
                                    placeholder="Please enter the main text">
                                </textarea-autosize>
                                <button type="button" @click="webInterviewPageMessages['welcomeText'].text = ''"
                                    class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                                <span class="help-block" v-if="errors.first('welcomePage.welcomeTextTitle')">{{
                                    $t('WebInterviewSettings.FieldRequired') }}</span>
                            </div>
                        </div>
                    </div>
                    <div class="row-element mb-30">
                        <div class="h5">
                            {{ $t('WebInterviewSettings.Description') }}
                        </div>
                        <md-editor ref="welcomeTextDescription" v-validate="" data-vv-name="invitation"
                            v-model="webInterviewPageMessages['invitation'].text">
                        </md-editor>
                    </div>

                    <div class="row-element mb-30">
                        <div class="h5">
                            {{ $t('WebInterviewSettings.StartNew') }}
                        </div>
                        <div class="form-group" :class="{ 'has-error': errors.has('welcomePage.startNewButton') }">
                            <div class="field" :class="{ 'answered': webInterviewPageMessages['startNewButton'].text }">
                                <input type="text" v-model="webInterviewPageMessages['startNewButton'].text"
                                    v-validate="'required'" data-vv-name="startNewButton" ref="startNewButton"
                                    maxlength="200" class="form-control" />
                                <button type="button" @click="webInterviewPageMessages['startNewButton'].text = ''"
                                    class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                                <span class="help-block" v-if="errors.first('welcomePage.startNewButton')">{{
                                    $t('WebInterviewSettings.FieldRequired') }}</span>
                            </div>
                        </div>
                    </div>

                    <div class="">
                        <button type="submit" :disabled="!isDirty('$welcomePage')"
                            @click="savePageTextEditMode('welcomePage', 'welcomeText', 'invitation', 'startNewButton')"
                            class="btn btn-md btn-success">
                            {{ $t('WebInterviewSettings.Save') }}
                        </button>
                        <button type="submit" :disabled="!isDirty('$welcomePage')"
                            @click="cancelPageTextEditMode('welcomePage', 'welcomeText', 'invitation', 'startNewButton')"
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
                                        {{ previewText(webInterviewPageMessages['welcomeText'].text) }}
                                    </div>
                                </div>
                                <div class="row-element mb-40">
                                    <p v-html="previewHtml(webInterviewPageMessages['invitation'].text)"></p>
                                </div>
                                <div class="row-element mb-40">
                                    <a href="javascript:void(0);" class="btn btn-success btn-lg">
                                        {{ previewText(webInterviewPageMessages['startNewButton'].text) }}
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
        this.$validator.reset('welcomePage')
    }
}
</script>