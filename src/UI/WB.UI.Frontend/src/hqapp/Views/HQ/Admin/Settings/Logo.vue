<template>
    <div role="tabpanel" class="tab-pane page-preview-block" id="logo">
        <div class="row contain-input" data-suso="settings-page">
            <div class="col-sm-9">
                <h2>{{ $t('Settings.LogoSettings') }}</h2>
                <p>{{ $t('Settings.LogoSettings_Description') }}</p>
            </div>
            <form :action="$config.model.updateLogoUrl" method="post" enctype="multipart/form-data" class="col-sm-9"
                @submit.prevent="onLogoSubmit">
                <input name="__RequestVerificationToken" type="hidden" :value="this.$hq.Util.getCsrfCookie()" />
                <div class="block-filter" style="padding-left: 30px">
                    <div class="form-group">
                        <label for="companyLogo" style="font-weight: bold">
                            <span class="tick"></span>
                            {{ $t('Settings.Logo') }}
                            <p style="font-weight: normal">
                                {{ $t('Settings.LogoSettings_Description1') }}
                            </p>
                        </label>
                    </div>
                    <div class="form-group" :class="{ 'has-error': invalidImage }">
                        <input type="file" id="companyLogo" ref="logoRef" name="logo" @change="changedFile"
                            accept="image/gif, image/jpeg, image/png" />
                        <span class="help-block" v-if="invalidImage">{{
                            this.$t('Settings.LogoNotUpdated') }}</span>
                    </div>
                </div>
                <div class="block-filter" style="padding-left: 30px">
                    <button :disabled="files.length == 0" type="submit" class="btn btn-success">
                        {{ $t('Common.Save') }}
                    </button>
                </div>
            </form>
            <div class="col-sm-9">
                <div class="block-filter" style="padding-left: 30px">
                    <figure class="logo-wrapper">
                        <figcaption>
                            {{ $t('Settings.CurrentLogo') }}:
                        </figcaption>
                        <img class="logo extra-margin-bottom" ref="logoImage" :src="logoImageSrc" @error="logoError"
                            alt="logo image" />
                    </figure>
                </div>
                <div class="block-filter action-block" style="padding-left: 30px">
                    <form :action="$config.model.removeLogoUrl" method="post" @submit.prevent="onLogoRemove">
                        <input name="__RequestVerificationToken" type="hidden" :value="this.$hq.Util.getCsrfCookie()" />
                        <button type="submit" class="btn btn-danger">
                            {{ $t('Settings.RemoveLogo') }}
                        </button>
                    </form>
                </div>
            </div>
        </div>
    </div>
</template>
<script>

import { humanFileSize } from '~/shared/helpers'
import * as toastr from 'toastr'

export default {
    data() {
        return {
            files: [],
            logoImageSrcDate: "",
            invalidImage: false
        }
    },
    computed: {
        logoImageSrc() {
            if (this.logoImageSrcDate === "") {
                this.logoImageSrcDate = new Date().getTime()
            }
            return this.$config.model.logoUrl + "?d=" + this.logoImageSrcDate
        }
    },

    methods: {
        async onLogoSubmit() {
            const fileInput = this.$refs.logoRef;
            const file = fileInput?.files?.[0];
            const maxFileSize = 1024 * 1024 * 10; // 10 MB

            if (!file) {
                toastr.warning(this.$t('Settings.SelectFileWarning'));
                this.invalidImage = true;
                return;
            }

            if (file.size > maxFileSize) {
                toastr.error(this.$t('Settings.LogoSizeLimit', { size: humanFileSize(maxFileSize) }));
                this.invalidImage = true;
                return;
            }

            const formData = new FormData();
            formData.append('logo', file);

            await this.sendLogoRequest(
                this.$config.model.updateLogoUrl,
                formData,
                this.$t('Settings.LogoUploadSuccess'),
                this.$t('Settings.LogoUploadError')
            );

            this.$refs.logoRef.value = '';
            this.invalidImage = false;
        },

        async onLogoRemove() {
            const formData = new FormData();

            await this.sendLogoRequest(
                this.$config.model.removeLogoUrl,
                formData,
                this.$t('Settings.LogoRemoveSuccess'),
                this.$t('Settings.LogoRemoveError')
            );
        },

        async sendLogoRequest(url, formData, successMsg, errorMsg) {
            formData.append('__RequestVerificationToken', this.$hq.Util.getCsrfCookie());

            try {
                const response = await fetch(url, {
                    method: 'POST',
                    body: formData
                });

                if (!response.ok) {
                    throw new Error('Non-OK response');
                }

                toastr.info(successMsg);
            } catch (err) {
                console.error(errorMsg, err);
                toastr.error(errorMsg);
            }

            this.logoImageSrcDate = Date.now();
        },
        changedFile(e) {
            this.files = this.$refs.logoRef.files
        },
        logoError() {
            if (this.$refs.logoImage.src !== this.$config.model.defaultLogoUrl)
                this.$refs.logoImage.src = this.$config.model.defaultLogoUrl
        },
    },
}
</script>
