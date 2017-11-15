<template>
    <div>
        <slot name="title">
            <h3>{{$t('UploadUsers.ImportingUserInfo')}} <br>{{fileName}}</h3>
        </slot>
        <div class="row">
            <div class="col-sm-7 col-xs-11 prefilled-data-info info-block">
                <br/><br/><br/>
                <h1>{{$t("UploadUsers.AllAccountsCreated", { total: totalCount })}}
                    <br/> {{$t('UploadUsers.InterviewersAndSupervisorsCount', {supervisorsCount: supervisorsCount, interviewersCount: interviewersCount})}}</h1>
                {{$t('UploadUsers.CompleteDescription')}}
            </div>
        </div>
        <div class="row">
            <div class="col-sm-7 col-xs-11 prefilled-data-info info-block">
                <br/><br/>
                <p class="list-inline">
                    <a class="btn btn-primary" v-bind:href="config.api.interviewsUrl">{{$t('UploadUsers.Interviews')}}</a>
                    <a class="btn btn-primary" v-bind:href="config.api.supervisorsUrl">{{$t('UploadUsers.Supervisors')}}</a>
                    <router-link class="btn btn-link" :to="{ name: 'upload'}">{{$t('UploadUsers.BackToImport')}}</router-link>
                </p>
            </div>
        </div>
    </div>
</template>


<script>
export default {
  computed: {
    config() {
      return this.$config.model;
    },
    fileName() {
      return this.$store.getters.upload.fileName;
    },
    totalCount() {
      if (!this.$store.getters.upload.complete) return 0;
      return this.supervisorsCount + this.interviewersCount;
    },
    supervisorsCount() {
      if (!this.$store.getters.upload.complete) return 0;
      return this.$store.getters.upload.complete.supervisorsCount;
    },
    interviewersCount() {
      if (!this.$store.getters.upload.complete) return 0;
      return this.$store.getters.upload.complete.interviewersCount;
    }
  }
};
</script>
