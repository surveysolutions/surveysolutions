<template>
    <div>
        <p v-if="model.serverUnderLoad">
            <strong>{{ $t('WebInterview.ServerUnderLoad') }}</strong>
        </p>

        <form method="post">
            <input name="__RequestVerificationToken" type="hidden" :value="this.$hq.Util.getCsrfCookie()" />

            <div v-if="model.useCaptcha && !model.serverUnderLoad && model.captchaErrors.length > 0"
                class="form-group has-error">
                <p>
                    <span class="help-block" v-for="captchaError in model.captchaErrors" v-bind:key="captchaError"
                        v-text="captchaError"></span>
                </p>
            </div>

            <div v-if="model.useCaptcha && model.recaptchaSiteKey && !model.useRecaptchaV3 && !model.serverUnderLoad" class="form-group">
                <vue-recaptcha v-if="model.useCaptcha" :sitekey="model.recaptchaSiteKey"
                    :loadRecaptchaScript="true"></vue-recaptcha>
            </div>
            <input v-if="model.useCaptcha && model.useRecaptchaV3 && !model.serverUnderLoad"
                name="g-recaptcha-response" type="hidden" :value="recaptchaV3Token" />
            <div v-if="model.useCaptcha && model.hostedCaptchaHtml && !model.serverUnderLoad"
                v-dompurify-html="model.hostedCaptchaHtml" class="form-group">
            </div>

            <div v-if="model.hasPassword" class="form-group" :class="{ 'has-error': model.isPasswordInvalid }">
                <label class="font-bold primary-text">
                    {{ $t('WebInterview.EnterPasswordText') }}
                </label>
                <input class="form-control" type="password" id="Password" name="password"
                    :placeholder="$t('WebInterviewUI.EnterPassword')" />
                <span v-if="model.isPasswordInvalid" class="help-block">{{ $t('WebInterview.InvalidPassword') }}</span>
                <input id="ShowPassword" type="checkbox"
                    onclick="var pass = document.getElementById('Password');pass.type = (pass.type === 'text' ? 'password' : 'text');">
                <label for="ShowPassword"
                    style="padding-left:5px; margin-top: 10px; font-size: 13px; font-weight: normal;">
                    <span></span>{{ $t('Pages.ShowPassword') }}
                </label>

            </div>
            <div v-if="model.serverUnderLoad" class="row-element">
                <button class="btn btn-success btn-lg" onclick="window.location.reload(); return false;">{{
                    $t('WebInterview.RefreshPage') }}</button>
            </div>
            <div v-else class="row-element mb-20">
                <button class="btn btn-success btn-lg" type="submit">
                    {{ buttonTitle }}
                </button>
            </div>

            <input v-if="model.hasPendingInterviewId" name="resume" class="btn btn-success btn-lg" type="submit"
                :value="resumeButtonTitle" />
        </form>
    </div>
</template>

<script>

import { VueRecaptcha } from 'vue-recaptcha'

export default {
    components: {
        VueRecaptcha,
    },
    props: {
        buttonTitle: null,
        resumeButtonTitle: null,
    },
    data() {
        return {
            recaptchaV3Token: '',
            recaptchaV3IntervalId: null,
        }
    },
    mounted() {
        if (this.model.useCaptcha && this.model.useRecaptchaV3 && this.model.recaptchaSiteKey) {
            this.loadRecaptchaV3()
        }
    },
    beforeUnmount() {
        if (this.recaptchaV3IntervalId !== null) {
            clearInterval(this.recaptchaV3IntervalId)
            this.recaptchaV3IntervalId = null
        }
    },
    computed: {
        model() {
            return this.$config.model
        },
    },
    methods: {
        loadRecaptchaV3() {
            const siteKey = this.model.recaptchaSiteKey
            const existingScript = document.querySelector(`script[src*="recaptcha/api.js?render=${siteKey}"]`)
            if (!existingScript) {
                const script = document.createElement('script')
                script.src = `https://www.google.com/recaptcha/api.js?render=${siteKey}`
                script.onload = () => this.executeRecaptchaV3(siteKey)
                script.onerror = () => { /* Script failed to load — the form remains submittable without a token; the server will reject the request */ }
                document.head.appendChild(script)
            } else {
                if (window.grecaptcha) {
                    this.executeRecaptchaV3(siteKey)
                } else {
                    existingScript.addEventListener('load', () => this.executeRecaptchaV3(siteKey), { once: true })
                }
            }
        },
        executeRecaptchaV3(siteKey) {
            if (!window.grecaptcha) return
            window.grecaptcha.ready(() => {
                window.grecaptcha.execute(siteKey, { action: 'start' }).then(token => {
                    this.recaptchaV3Token = token
                    // Refresh the token every 90 seconds (v3 tokens expire after 2 minutes)
                    if (this.recaptchaV3IntervalId !== null) {
                        clearInterval(this.recaptchaV3IntervalId)
                    }
                    this.recaptchaV3IntervalId = setInterval(() => {
                        if (!window.grecaptcha) {
                            clearInterval(this.recaptchaV3IntervalId)
                            this.recaptchaV3IntervalId = null
                            return
                        }
                        window.grecaptcha.execute(siteKey, { action: 'start' }).then(t => {
                            this.recaptchaV3Token = t
                        }).catch(() => { /* Ignore token refresh failures */ })
                    }, 90000)
                }).catch(() => { /* Ignore token generation failures; the server will reject the request */ })
            })
        },
    },
}
</script>