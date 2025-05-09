<template>
    <div role="tabpanel" class="tab-pane page-preview-block" id="logo">
        <div class="row contain-input" data-suso="settings-page">
            <div class="col-sm-9">
                <h2>{{ $t('Settings.LogoSettings') }}</h2>
                <p>{{ $t('Settings.LogoSettings_Description') }}</p>
            </div>
            <form :action="$config.model.updateLogoUrl" method="post" enctype="multipart/form-data" class="col-sm-9"
                @submit="onLogoSubmit">
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
                    <div class="form-group" :class="{ 'has-error': this.$config.model.invalidImage }">
                        <input type="file" id="companyLogo" ref="logoRef" name="logo" @change="changedFile"
                            accept="image/gif, image/jpeg, image/png" />
                        <span class="help-block" v-if="this.$config.model.invalidImage">{{
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
                        <img class="logo extra-margin-bottom" ref="logoImage" :src="$config.model.logoUrl"
                            @error="logoError" alt="logo image" />
                    </figure>
                </div>
                <div class="block-filter action-block" style="padding-left: 30px">
                    <form :action="$config.model.removeLogoUrl" method="post">
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

export default {
    data() {
        return {
            files: [],
        }
    },
    methods: {
        onLogoSubmit(e) {
            if (
                window.File &&
                window.FileReader &&
                window.FileList &&
                window.Blob
            ) {
                //get the file size and file type from file input field
                const fsize = this.$refs.logoRef.files[0].size
                const maxFileSize = 1024 * 1024 * 10

                if (fsize > maxFileSize) {
                    e.preventDefault();
                    alert(this.$t('Settings.LogoSizeLimit', { size: humanFileSize(maxFileSize) }))
                    return false
                } else {
                    return true
                }
            }
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
