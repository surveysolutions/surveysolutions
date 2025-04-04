<template>
    <div role="tabpanel" class="tab-pane page-preview-block" id="logo">
        <div class="row extra-margin-bottom">
            <div class="col-sm-9">
                <h2>{{ $t('Settings.LogoSettings') }}</h2>
                <p>{{ $t('Settings.LogoSettings_Description') }}</p>
                <p>{{ $t('Settings.LogoSettings_Description1') }}</p>
            </div>
            <form :action="$config.model.updateLogoUrl" method="post" enctype="multipart/form-data" class="col-sm-7"
                @submit="onLogoSubmit">
                <input name="__RequestVerificationToken" type="hidden" :value="this.$hq.Util.getCsrfCookie()" />
                <div class="block-filter">
                    <div class="form-group" :class="{ 'has-error': this.$config.model.invalidImage }">
                        <label for="companyLogo">
                            {{ $t('Settings.Logo') }}
                        </label>
                        <input type="file" id="companyLogo" ref="logoRef" name="logo" @change="changedFile"
                            accept="image/gif, image/jpeg, image/png" />
                        <span class="help-block" v-if="this.$config.model.invalidImage">{{
                            this.$t('Settings.LogoNotUpdated') }}</span>
                    </div>
                </div>
                <div class="block-filter">
                    <button :disabled="files.length == 0" type="submit" class="btn btn-success">
                        {{ $t('Common.Save') }}
                    </button>
                </div>
            </form>
            <div class="col-sm-9">
                <div class="block-filter">
                    <figure class="logo-wrapper">
                        <figcaption>
                            {{ $t('Settings.CurrentLogo') }}:
                        </figcaption>
                        <img class="logo extra-margin-bottom" ref="logoImage" :src="$config.model.logoUrl"
                            @error="logoError" alt="logo image" />
                    </figure>
                </div>
                <div class="block-filter action-block">
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
export default {
    data() {
        return {
            files: [],
        }
    },
    methods: {
        onLogoSubmit() {
            if (
                window.File &&
                window.FileReader &&
                window.FileList &&
                window.Blob
            ) {
                //get the file size and file type from file input field
                var fsize = this.$refs.logoRef.files[0].size

                if (fsize > 1024 * 1024 * 10) {
                    alert('Logo image size should be less than 10mb')
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
