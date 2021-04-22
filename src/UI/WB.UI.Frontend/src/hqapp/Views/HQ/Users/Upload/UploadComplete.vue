<template>
    <div>
        <slot name="title">
            <h3>{{$t('UploadUsers.ImportingUserInfo')}} <br>{{fileName}}</h3>
        </slot>
        <div class="row">
            <div class="col-sm-7 col-xs-12 action-block preloading-done active-preloading">
                <div class="import-progress">
                    <h1>{{$t("UploadUsers.AllAccountsCreated", { total: totalCount })}}
                        <br/> {{$t('UploadUsers.InterviewersAndSupervisorsCount', {supervisorsCount: supervisorsCount, interviewersCount: interviewersCount})}}</h1>
                    {{$t('UploadUsers.CompleteDescription')}}
                </div>
                <div class="action-buttons">
                    <router-link class="btn btn-link"
                        :to="{ name: 'upload'}">
                        {{$t('UploadUsers.BackToImport')}}
                    </router-link>
                </div>
            </div>
        </div>
    </div>
</template>


<script>
export default {
    computed: {
        config() {
            return this.$config.model
        },
        fileName() {
            return this.$store.getters.upload.fileName
        },
        totalCount() {
            if (!this.$store.getters.upload.complete) return 0
            return this.supervisorsCount + this.interviewersCount
        },
        supervisorsCount() {
            if (!this.$store.getters.upload.complete) return 0
            return this.$store.getters.upload.complete.supervisorsCount
        },
        interviewersCount() {
            if (!this.$store.getters.upload.complete) return 0
            return this.$store.getters.upload.complete.interviewersCount
        },
    },
}
</script>
