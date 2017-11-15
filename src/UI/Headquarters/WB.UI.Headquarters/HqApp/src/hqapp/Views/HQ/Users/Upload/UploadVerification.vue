<template>
    <div>
        <slot name="title">
            <h3>{{$t('UploadUsers.ImportingUserInfo')}} <br>{{fileName}}</h3>
        </slot>
        <div class="row">
            <div class="col-sm-7 col-xs-11 prefilled-data-info info-block">
                <p>
                    <h1>
                        <span class="text-danger">{{$t('UploadUsers.VerificationFailed')}}</span> <br> {{$t('UploadUsers.NoCreatedUsers')}}
                    </h1>
                </p>
                <p v-for="error in verificationErrors">
                    <strong class="text-danger">[{{$t('UploadUsers.Line')}}: {{error.line}}, {{$t('UploadUsers.Column')}}: {{error.column}}]: {{error.message}}</strong><br>
                    <span class="info-block">{{error.description}}</span><br>
                    <span v-if="error.recomendation" class="info-block">{{error.recomendation}}</span>
                </p>
                <br>
                <p class="list-inline">
                    <input name="file" ref="uploader" v-show="false" accept=".tsv, .txt" type="file" @change="onFileChange" class="btn btn-default btn-lg btn-action-questionnaire" />
                    <button type="button" class="btn btn-success" @click="$refs.uploader.click()">{{$t('UploadUsers.ReUploadTabFile')}}</button>
                    <router-link class="btn btn-link" :to="{ name: 'upload'}">{{$t('UploadUsers.BackToImport')}}</router-link>
                </p>
            </div>
        </div>
    </div>
</template>


<script>
import * as toastr from "toastr";

export default {
  computed: {
    config() {
      return this.$config.model;
    },
    fileName() {
      return this.$store.getters.upload.fileName;
    },
    verificationErrors() {
      return this.$store.getters.upload.verificationErrors;
    }
  },
  methods: {
    onFileChange(e) {
      const files = e.target.files || e.dataTransfer.files;

      if (!files.length) {
        return;
      }

      var file = files[0];
      var formData = new FormData();
      formData.append("file", files[0]);

      var self = this;

      this.$http
        .post(this.config.api.importUsersUrl, formData)
        .then(response => {
          self.$store.dispatch("setUploadFileName", file.name);

          const errors = response.data;

          if (errors.length > 0)
            self.$store.dispatch("setUploadVerificationErrors", errors);
          else self.$router.push({ name: "uploadprogress" });
        })
        .catch(e => {
          toastr.error(e.response.data.ExceptionMessage);
        });
    }
  }
};
</script>
