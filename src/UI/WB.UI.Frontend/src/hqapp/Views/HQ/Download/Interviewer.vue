<template>
    <main>
        <div class="container-fluid">
            <div class="row two-columns-form">
                <div class="col-md-6 col-sm-6 col-xs-12 left-column">
                    <div class="centered-box-table">
                        <div class="centered-box-table-cell">
                            <div class="retina headquarter">
                                Survey Solutions Headquarters
                            </div>
                            <p>
                                {{ $t('Pages.DownloadPage_Title') }}
                            </p>
                        </div>
                    </div>
                </div>
                <div class="col-md-6 col-sm-6 col-xs-12 right-column">
                    <div class="centered-box-table">
                        <div class="centered-box-table-cell">
                            <div class="hq-apps-wrapper">
                                <h2>{{ $t('Pages.DownloadPage_Welcome') }}</h2>
                                <div class="form-actions">
                                    <a :href="apkUrl"
                                        class="get-interviewer-app"
                                        id="dowload">
                                        <span>{{ $t('Pages.GetLatestApp') }}</span>
                                        <span class="version">{{ $t('Pages.DownloadPage_Version') + ' ' + interviewerVersion }}</span>
                                    </a>
                                    <img v-if="supportQRCodeGeneration"
                                        id="download-qr"
                                        alt="QR Code"
                                        width="250"
                                        height="250"
                                        :src="qrApkUrl"/>
                                    <p>
                                        <b class="error-text">{{ $t('Pages.CautionTitle') }}</b>
                                        {{ $t('Pages.GetEsriExtraDescription') }}
                                    </p>
                                </div>
                                <div class="form-group">
                                    <input class="checkbox-filter"
                                        id="all-answer-options"
                                        type="checkbox"
                                        checked=""
                                        v-model="smallApkSelected">
                                    <label for="all-answer-options">
                                        <span class="tick"></span>{{ $t('Pages.ExcludeEsriTitle') }}
                                    </label>
                                </div>
                                <div class="additional-info-block">
                                    <a href="http://support.mysurvey.solutions/interviewer-installation"
                                        target="_blank">
                                        {{ $t('Pages.DownloadPage_Instructions') }}
                                    </a>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="spacer"></div>
        </div>
    </main>
</template>

<script>

export default {
    data() {
        return {
            smallApkSelected: true,
        }
    },
    methods: {
    },
    computed: {
        model() {
            return this.$config.model
        },
        supportQRCodeGeneration() {
            return this.model.supportQRCodeGeneration
        },
        qrApkUrl() {
            if (this.smallApkSelected) {
                return this.model.smallApkQRUrl
            } else {
                return this.model.fullApkQRUrl
            }
        },
        apkUrl() {
            if (this.smallApkSelected) {
                return this.model.smallApkUrl
            } else {
                return this.model.fullApkUrl
            }
        },
        interviewerVersion() {
            if (this.smallApkSelected) {
                return this.model.smallApkVersion
            } else {
                return this.model.fullApkVersion
            }
        },
    },
}
</script>
