<template>
    <div role="tabpanel" class="tab-pane active page-preview-block" id="welcome">
        <Form v-slot="{ errors, meta }" ref="welcomePage" class="" :data-vv-scope="'welcomePage'" v-on:submit="dummy">
            <div class="d-flex f-row">
                <div class="costomization-block">
                    <div class="row-element mb-30">
                        <div class="h5">
                            {{ $t('WebInterviewSettings.Title') }}
                        </div>
                        <div class="form-group" :class="{ 'has-error': errors.welcomeTextTitle }">
                            <div class="field" :class="{ 'answered': webInterviewPageMessages['welcomeText'].text }">
                                <Field as="textarea" v-autosize name="welcomeTextTitle"
                                    v-model="webInterviewPageMessages['welcomeText'].text" rules="required"
                                    data-vv-name="welcomeTextTitle" ref="welcomeTextTitle" :min-height="77"
                                    maxlength="200" class="form-control js-elasticArea font-bold"
                                    placeholder="Please enter the main text">
                                </Field>
                                <button type="button" @click="webInterviewPageMessages['welcomeText'].text = ''"
                                    class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                                <span class="help-block" v-if="errors.welcomeTextTitle">
                                    {{ $t('WebInterviewSettings.FieldRequired') }}
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="row-element mb-30">
                        <div class="h5">
                            {{ $t('WebInterviewSettings.Description') }}
                        </div>
                        <Field v-slot="{ field }" name="welcomeTextDescription"
                            :value="webInterviewPageMessages['invitation'].text">

                            <md-editor ref="welcomeTextDescription" v-bind="field" data-vv-name="invitation"
                                v-model="webInterviewPageMessages['invitation'].text">
                            </md-editor>
                        </Field>
                    </div>

                    <div class="row-element mb-30">
                        <div class="h5">
                            {{ $t('WebInterviewSettings.StartNew') }}
                        </div>
                        <div class="form-group" :class="{ 'has-error': errors.startNewButton }">
                            <div class="field" :class="{ 'answered': webInterviewPageMessages['startNewButton'].text }">
                                <Field type="text" v-model="webInterviewPageMessages['startNewButton'].text"
                                    name="startNewButton" rules="required" data-vv-name="startNewButton"
                                    ref="startNewButton" maxlength="200" class="form-control" />
                                <button type="button" @click="webInterviewPageMessages['startNewButton'].text = ''"
                                    class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                                <span class="help-block" v-if="errors.startNewButton">{{
                                    $t('WebInterviewSettings.FieldRequired') }}</span>
                            </div>
                        </div>
                    </div>

                    <div class="">
                        <button type="submit" :disabled="!meta.dirty ? 'disabled' : null"
                            @click="savePageTextEditMode($refs.welcomePage, 'welcomeText', 'invitation', 'startNewButton')"
                            class="btn btn-md btn-success">
                            {{ $t('WebInterviewSettings.Save') }}
                        </button>
                        <button type="submit" :disabled="!meta.dirty ? 'disabled' : null"
                            @click="cancelPageTextEditMode($refs.welcomePage, 'welcomeText', 'invitation', 'startNewButton')"
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
        this.$refs.welcomePage.resetForm({ values: this.$refs.welcomePage.values })
    }
}
</script>