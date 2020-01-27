<template>
    <div>
        <p v-if="model.serverUnderLoad">
            <strong>{{ $t('WebInterview.ServerUnderLoad') }}</strong>
        </p>

        <form method="post">
            <input name="__RequestVerificationToken"
                type="hidden"
                :value="this.$hq.Util.getCsrfCookie()" />

            <div
                v-if="model.useCaptcha && !model.serverUnderLoad && model.captchaErrors.length > 0"
                class="form-group has-error">
                <p>
                    <span
                        class="help-block"
                        v-for="captchaError in model.captchaErrors"
                        v-bind:key="captchaError"
                        v-text="captchaError"></span>
                </p>
            </div>

            <div v-if="model.useCaptcha && !model.serverUnderLoad"
                v-html="model.captchaHtml"
                class="form-group">
            </div>

            <div
                v-if="model.hasPassword"
                class="form-group"
                :class="{ 'has-error' : model.isPasswordInvalid }">
                <label class="font-bold primary-text">
                    {{ $t('WebInterview.EnterPasswordText') }}
                </label>
                <input
                    class="form-control"
                    type="password"
                    name="password"
                    placeholder="Enter the password"/>
                <span
                    v-if="model.isPasswordInvalid"
                    class="help-block">{{$t('WebInterview.InvalidPassword')}}</span>
            </div>
            <div v-if="model.serverUnderLoad"
                class="row-element">
                <button
                    class="btn btn-success btn-lg"
                    onclick="window.location.reload(); return false;">{{$t('WebInterview.RefreshPage')}}</button>
            </div>
            <div v-else
                class="row-element mb-20">
                <button class="btn btn-success btn-lg"
                    type="submit">
                    {{ buttonTitle }}
                </button>
            </div>

            <input
                v-if="model.hasPendingInterviewId"
                name="resume"
                class="btn btn-success btn-lg"
                type="submit"
                :value="$t('WebInterview.ResumeInterview')"/>
        </form>
    </div>
</template>

<script>

export default {
    props: {
        buttonTitle: null,
    },
    computed: {
        model() {
            return this.$config.model
        },
    },
}
</script>