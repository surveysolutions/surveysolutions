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
                        <h1>{{$t('UploadUsers.UploadingAndVerifying')}}</h1>
                    </p>
                    <p>
                        <div class="cancelable-progress">
                            <div class="wrapper-progress">
                                <div class="progress" style="width:95%">
                                    <div class="progress-bar progress-bar-primary" role="progressbar" aria-valuenow="10" aria-valuemin="0" aria-valuemax="10" style="width: 50%">
                                        <span class="sr-only">{{$t('UploadUsers.UploadingAndVerifying')}}</span>
                                    </div>
                                </div>
                            </div>
                            <button type="button" class="btn btn-link" style="position:initial">
                                {{$t('UploadUsers.Cancel')}}
                            </button>
                        </div>
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
    cancelUpload() {}
  }
};
</script>
