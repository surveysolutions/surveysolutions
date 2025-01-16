<template>
    <div role="tabpanel" class="tab-pane page-preview-block" id="link">
        <Form v-slot="{ errors, meta }" ref="linkPage" class="" :data-vv-scope="'linkPage'" @submit="dummy">
            <div class="d-flex f-row">
                <div class="costomization-block">
                    <div class="row-element mb-30">
                        <div class="h5">
                            {{ $t('WebInterviewSettings.Title') }}
                        </div>
                        <div class="form-group" :class="{ 'has-error': errors.linkWelcome }">
                            <div class="field" :class="{ 'answered': webInterviewPageMessages['linkWelcome'].text }">
                                <Field as="textarea" v-autosize v-model="webInterviewPageMessages['linkWelcome'].text"
                                    rules="required" name="linkWelcome" data-vv-name="linkWelcome" ref="linkWelcome"
                                    :min-height="77" maxlength="200" class="form-control js-elasticArea font-bold"
                                    placeholder="Please enter the main text">
                                </Field>
                                <button type="button" @click="webInterviewPageMessages['linkWelcome'].text = ''"
                                    class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                                <span class="help-block" v-if="errors.linkWelcome">{{
                                    $t('WebInterviewSettings.FieldRequired') }}</span>
                            </div>
                        </div>
                    </div>
                    <div class="row-element mb-30">
                        <div class="h5">
                            {{ $t('WebInterviewSettings.Description') }}
                        </div>
                        <Field v-slot="{ field }" name="linkInvitationDescription"
                            :value="webInterviewPageMessages['linkInvitation'].text">

                            <md-editor ref="linkInvitation" v-bind="field" data-vv-name="linkInvitation"
                                v-model="webInterviewPageMessages['linkInvitation'].text">
                            </md-editor>

                        </Field>

                    </div>

                    <div class="">
                        <button type="submit" :disabled="!meta.dirty ? 'disabled' : null"
                            @click="savePageTextEditMode($refs.linkPage, 'linkWelcome', 'linkInvitation')"
                            class="btn btn-md btn-success">
                            {{ $t('WebInterviewSettings.Save') }}
                        </button>
                        <button type="submit" :disabled="!meta.dirty ? 'disabled' : null"
                            @click="cancelPageTextEditMode($refs.linkPage, 'linkWelcome', 'linkInvitation')"
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
                                        {{ previewText(webInterviewPageMessages['linkWelcome'].text) }}
                                    </div>
                                </div>
                                <div class="row-element mb-40">
                                    <p v-dompurify-html="previewHtml(webInterviewPageMessages['linkInvitation'].text)">
                                    </p>
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
import { Form, Field, ErrorMessage } from 'vee-validate'

export default {
    components: {
        Form,
        Field,
        ErrorMessage,
    },
    mixins: [settings],
    mounted() {
        this.$refs.linkPage.resetForm({ values: this.$refs.linkPage.values })
    }
}
</script>