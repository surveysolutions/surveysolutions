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
                                <h2>{{ $t('Pages.DownloadSupervisorPage_Welcome') }}</h2>
                                <div class="form-actions">
                                    <a :href="model.apkUrl" class="get-supervisor-app">
                                        <span>{{ $t('Pages.GetLatestSupervisorApp') }}</span>
                                        <span class="version">{{ $t('Pages.DownloadPage_Version') + ' ' +
                                            model.supervisorVersion }}</span>
                                        <span v-if="supervisorSize" class="version">{{ $t('Pages.DownloadPage_Size') +
                                            ' ' + formattedSupervisorSize }}</span>
                                    </a>
                                    <img v-if="model.supportQRCodeGeneration" id="download-qr" class="download-qr"
                                        alt="QR Code" width="250" height="250" :src="model.apkQRUrl" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>

<script>

import { humanFileSize } from '~/shared/helpers'

export default {
    data() {
        return {
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
        supervisorSize() {
            return this.model.supervisorApkSize
        },
        formattedSupervisorSize() {
            return this.supervisorSize ? humanFileSize(this.supervisorSize) : ''
        },
    },
}
</script>
