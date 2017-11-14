<template>
    <div class="container">
        <div class="row">
            <div class="page-header">
                <ol class="breadcrumb">
                    <li>
                        <a href="#">{{$t('MainMenu.TeamsAndRoles')}}</a>
                    </li>
                </ol>
                <h1>{{$t('UploadUsers.Title')}}</h1>
            </div>
        </div>

        <div class="row">
            <div class="col-sm-6 col-xs-10 prefilled-data-info info-block">
                <p>{{$t('UploadUsers.Description')}}
                    <a v-bind:href="config.api.supervisorsUrl">{{$t('UploadUsers.ManualUserCreateLink')}}</a>
                </p>
            </div>
        </div>
        <div class="row">
            <div class="col-sm-7 col-xs-11 prefilled-data-info info-block">
                <h3>{{$t('UploadUsers.RequiredData')}}</h3>
                <ul class="list-unstyled prefilled-data">
                    <li>
                        <p>{{$t('UploadUsers.UserName')}}
                            <span class="info-block">({{$t('UploadUsers.UserNameDescription')}})</span>
                        </p>
                    </li>
                    <li>
                        <p>{{$t('UploadUsers.Password')}}
                            <span class="info-block">({{$t('UploadUsers.PasswordDescription')}})</span>
                        </p>
                    </li>
                    <li>
                        <p>{{$t('UploadUsers.Role')}}
                            <span class="info-block">('supervisor' {{$t('UploadUsers.Or')}} 'interviewer')</span>
                        </p>
                    </li>
                    <li>
                        <p>{{$t('UploadUsers.AssignedTo')}}
                            <span class="info-block">({{$t('UploadUsers.AssignedToDescription')}})</span>
                        </p>
                    </li>
                </ul>
                <h3>{{$t('UploadUsers.OptionalInformation')}}</h3>
                <ul class="list-unstyled prefilled-data">
                    <li>
                        <p>{{$t('UploadUsers.FullName')}}</p>
                    </li>
                    <li>
                        <p>{{$t('UploadUsers.Email')}}</p>
                    </li>
                    <li>
                        <p>{{$t('UploadUsers.Phone')}}</p>
                    </li>
                </ul>
                <p>
                    <a class="btn btn-link" target="_blank" v-bind:href="config.api.importUsersTemplateUrl">{{$t('UploadUsers.DownloadTemplateLink')}}</a>
                </p>
                <p>
                    <input name="file" ref="uploader" v-show="false" accept=".tsv, .txt" type="file" @change="onFileChange" class="btn btn-default btn-lg btn-action-questionnaire" />
                    <button type="button" class="btn btn-success" @click="$refs.uploader.click()">{{$t('UploadUsers.UploadBtn')}}</button>
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
      formData.append("file", file);

      var self = this;

      this.$http
        .post(this.config.api.importUsersUrl, formData)
        .then(response => {
          self.$store.dispatch("setUploadFileName", file.name);

          const errors = response.data;

          if (errors.length == 0) self.$router.push({ name: "uploadprogress" });
          else {
            self.$store.dispatch("setUploadVerificationErrors", errors);
            self.$router.push({ name: "uploadverification" });
          }
        })
        .catch(e => {
          toastr.error(e.response.data.ExceptionMessage);
        });
    }
  }
};
</script>
