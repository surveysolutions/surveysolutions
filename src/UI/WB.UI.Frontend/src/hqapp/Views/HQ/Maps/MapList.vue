<template>
    <HqLayout :hasFilter="false" :title="$t('Pages.MapList_Title')">        
        <div slot="headers">
            <div class="topic-with-button" >
                <h1>{{$t('Pages.MapList_Title')}}</h1>                
                    <label class="btn btn-success btn-file" v-if="actionsAlowed">
                        {{$t('Pages.MapList_Upload')}}
                        <input accept=".zip" ref="uploader" id="File" name="File" @change="onFileChange" type="file" value="" />
                    </label>
                </div>
                <div ref="status" ><p>{{statusMessage}}</p></div>
                <div ref="errors" class="alert alert-danger">
                    <div v-for="error in errorList" :key="error">{{error}}</div>
                </div>
            <ol class="list-unstyled">
                <li>{{$t('Pages.MapList_UploadDescription')}} </li>
                <li>{{$t('Pages.MapList_UploadDescriptionExtra')}}</li>
            </ol>
            <p>
                <a :href="$config.model.userMapsUrl">{{$t('Pages.MapList_UserMapsLink')}}</a>
            </p>
            <p>
                <a :href="$config.model.userMapLinkingUrl">{{$t('Pages.MapList_UserLinking')}}</a>
            </p>
        </div>
        <DataTables ref="table" 
            :tableOptions="tableOptions" 
            :contextMenuItems="contextMenuItems">
        </DataTables>

        <Confirm ref="confirmDiscard"
                 id="discardConfirm"
                 slot="modals">
            {{ $t("Pages.Map_DiscardConfirm") }}
        </Confirm>
    </HqLayout>
</template>

<script>
export default {
    data: function(){
        return {        
            statusMessage: '',
            errorList: []
        }        
    },
  mounted() {
    this.$refs.table.reload();
  },
  methods: {
      updateStatus(newMessage, errors){
          this.statusMessage = this.$t("Pages.Map_Status") + ": " + newMessage;
          if(errors != null){
            this.errorList = errors;
          }
          else
            this.errorList = [];
      },
      progressStyle() {
                return {
                    width: this.fileProgress + "%"
                }
            },
    reload() {
            this.$refs.table.reload();
        },
    onFileChange(e){
        const statusupdater = this.updateStatus;
        const reloader = this.reload;
        const uploadingMessage = this.$t("Pages.Map_Uploading");
        const uploadingErrorMessage = this.$t("Pages.Map_UploadingError");
        const uploadingSuccess = this.$t("Pages.Map_UploadingSuccess");
        const uploadingFileTooBig = this.$t("Pages.Map_UploadingFileTooBig");

        const fd = new FormData();
        var fileToUpload = this.$refs.uploader.files[0];
        
        var filesize = ((fileToUpload.size/1024)/1024).toFixed(4);

        if(filesize >= 1024){
            statusupdater(uploadingFileTooBig);
            return;
        }

        fd.append("", fileToUpload);
        
        $.ajax({
                url: this.$config.model.uploadMapsFileUrl,
                xhr() {
                    const xhr = $.ajaxSettings.xhr()
                    xhr.upload.onprogress = (e) => {                        
                        statusupdater(uploadingMessage + " " + parseInt((e.loaded / e.total) * 100) + "%");                        
                    }
                    return xhr
                },
                data: fd,
                processData: false,
                contentType: false,
                type: "POST",
                success: function(data) {
                    if(!data.isSuccess)
                       statusupdater(uploadingErrorMessage, data.errors);
                    else   
                       statusupdater(uploadingSuccess);
                    reloader();                    
                },
                error : function(error){
                    statusupdater(uploadingErrorMessage);
                }
            });  
        this.$refs.uploader.value = '';            
    },

    contextMenuItems({ rowData }) {
      return [
        {
          name: this.$t("Common.Open"),
          callback: () => window.location = window.input.settings.config.basePath + "Maps/Details?mapname=" + encodeURIComponent(rowData.fileName)           
        },
        {
          name: this.$t("Pages.MapList_DeleteMap"),
          callback: () => this.deleteMap(rowData.fileName)
        }
      ];
    },
    deleteMap(fileName) {
        const self = this;
            this.$refs.confirmDiscard.promt(ok => {
                if (ok) {
                    this.$http({
                        method: 'delete',
                        url: this.config.deleteMapLinkUrl,
                        data: {map: fileName}});

                     self.$refs.table.reload();
                }
            });
    }
  },
  computed: {
    config() {
      return this.$config.model;
    },
    actionsAlowed() {
            return !this.config.isObserver && !this.config.isObserving;
        },
    tableOptions() {
      var self = this;
      return {
        deferLoading: 0,
        columns: [
          {
            data: "fileName",
            name: "FileName",
            class: "title",
            title: this.$t("Pages.MapList_Name")
          },
          {
            data: "size",
            name: "Size",
            class: "parameters",
            title: this.$t("Pages.MapList_Size")
          },
          {
            data: "importDate",
            name: "ImportDate",
            class: "date",
            title: this.$t("Pages.MapList_Updated")
          }
        ],
        ajax: {
          url: this.$config.model.dataUrl,
          type: "GET"
        },
        responsive: false,
        order: [[0, "asc"]],
        sDom: 'rf<"table-with-scroll"t>ip'
      };
    }
  }
};
</script>
