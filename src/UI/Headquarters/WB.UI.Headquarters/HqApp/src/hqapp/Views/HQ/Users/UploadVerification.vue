<template>
    <main>
        <div class="container">
            <div class="row">
                <div class="page-header">
                    <ol class="breadcrumb">
                        <li>
                            <a href="#">{{$t('MainMenu.TeamsAndRoles')}}</a>
                        </li>
                    </ol>
                    <h1>{{$t('UploadUsers.Title')}}</h1>
                    <p>
                        <h3>{{$t('UploadUsers.ImportingUserInfo')}} <br>some-file.tsv</h3>
                    </p>
                </div>
            </div>
            <div class="row">
                <div class="col-sm-7 col-xs-11 prefilled-data-info info-block">
                    <p>
                        <h1>
                            <span class="text-danger">{{$t('UploadUsers.VerificationFailed')}}</span> <br> {{$t('UploadUsers.NoCreatedUsers')}}
                        </h1>
                    </p>
                    <p>
                        <strong class="text-danger">[01]: Error</strong><br>
                        <span class="info-block">({{$t('UploadUsers.UserNameDescription')}})</span>
                    </p>
                    <p class="list-inline">
                        <input name="file" ref="uploader" v-show="false" accept=".tsv" type="file" @change="onFileChange" class="btn btn-default btn-lg btn-action-questionnaire" />
                        <button type="button" class="btn btn-success" @click="$refs.uploader.click()">{{$t('UploadUsers.ReUploadTabFile')}}</button>
                        <router-link class="btn btn-link" :to="{ name: 'upload'}">{{$t('UploadUsers.BackToImport')}}</router-link>
                    </p>
                </div>
            </div>
        </div>
    </main>
</template>


<script>
export default {
  computed: {
    config() {
      return this.$config.model;
    }
  },
  methods: {
    onFileChange(e) {
      const files = e.target.files || e.dataTransfer.files;

      if (!files.length) {
        return;
      }

      var formData = new FormData();
      formData.append("file", files[0]);

      this.$http
        .post(this.config.api.importUsersUrl, formData)
        .then(response => {})
        .catch(() => this.$router.push("upload"));
    }
  }
};
</script>
