<template>
    <div role="tabpanel"
        class="tab-pane page-preview-block"
        id="link">
        <form class=""
            :data-vv-scope="'linkPage'"
            v-on:submit.prevent="dummy">
            <div class="d-flex f-row">
                <div class="costomization-block">
                    <div class="row-element mb-30">
                        <div class="h5">
                            {{$t('WebInterviewSettings.Title')}}
                        </div>
                        <div class="form-group"
                            :class="{ 'has-error': errors.has('linkPage.linkWelcome') }">
                            <div class="field"
                                :class="{ 'answered': webInterviewPageMessages['linkWelcome'].text }">
                                <textarea-autosize
                                    v-model="webInterviewPageMessages['linkWelcome'].text"
                                    v-validate="'required'"
                                    data-vv-name="linkWelcome"
                                    ref="linkWelcome"
                                    :min-height="77"
                                    maxlength="200"
                                    class="form-control js-elasticArea font-bold"
                                    placeholder="Please enter the main text">
                                </textarea-autosize>
                                <button type="button"
                                    @click="webInterviewPageMessages['linkWelcome'].text=''"
                                    class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                                <span class="help-block"
                                    v-if="errors.first('linkPage.linkWelcome')">{{$t('WebInterviewSettings.FieldRequired')}}</span>
                            </div>
                        </div>
                    </div>
                    <div class="row-element mb-30">
                        <div class="h5">
                            {{$t('WebInterviewSettings.Description')}}
                        </div>
                        <md-editor
                            ref="linkInvitation"
                            v-validate=""
                            data-vv-name="linkInvitation"
                            v-model="webInterviewPageMessages['linkInvitation'].text">
                        </md-editor>

                    </div>

                    <div class="">
                        <button type="submit"
                            :disabled="!isDirty('$linkPage')"
                            @click="savePageTextEditMode('linkPage', 'linkWelcome', 'linkInvitation', 'linkButton')"
                            class="btn btn-md btn-success">
                            {{$t('WebInterviewSettings.Save')}}
                        </button>
                        <button type="submit"
                            :disabled="!isDirty('$linkPage')"
                            @click="cancelPageTextEditMode('linkPage', 'linkWelcome', 'linkInvitation', 'linkButton')"
                            class="btn btn-md btn-link">
                            {{$t('WebInterviewSettings.Cancel')}}
                        </button>
                    </div>
                </div>
                <div class="preview">
                    <div class="browser-mockup mb-30">
                        <div class="mockup-block d-flex f-row">
                            <div class="icon" />

                            <div class="text-example">
                                <div class="row-element mb-30">
                                    <Logo :hasLogo="hasLogo"
                                        :logoUrl="logoUrl" />
                                </div>
                                <div class="row-element mb-20">
                                    <div class="h2">
                                        {{previewText(webInterviewPageMessages['linkWelcome'].text)}}
                                    </div>
                                </div>
                                <div class="row-element mb-40">
                                    <p v-html="previewHtml(webInterviewPageMessages['linkInvitation'].text)"></p>
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
}
</script>