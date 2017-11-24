<template>
    <HqLayout :hasFilter="false" :title="$t('Pages.MapList_Title')">        
        <div slot="headers">
            <div class="topic-with-button" >
                <h1>{{$t('Pages.MapList_Title')}}</h1>                
                    <label class="btn btn-success btn-file">
                        {{$t('Pages.MapList_Upload')}}
                        <input accept=".zip" ref="uploader" id="File" name="File" @change="onFileChange" type="file" value="" />
                    </label>
                </div>
                <div ref="status" ><p>{{statusMessage}}</p></div>
            <ol class="list-unstyled">
                <li>{{$t('Pages.MapList_UploadDescription')}} </li>
                <li>{{$t('Pages.MapList_UploadDescriptionExtra')}}</li>
            </ol>
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
            statusMessage: ''
        }        
    },
  mounted() {
    this.$refs.table.reload();
  },
  methods: {
      updateStatus(newMessage){
          this.statusMessage = this.$t("Pages.Map_Status") + ": " + newMessage;
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
        
        const fd = new FormData();
        fd.append("file", this.$refs.uploader.files[0]);
        
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
                    statusupdater(data);
                    reloader();                    
                },
                error : function(error){
                    statusupdater(error);
                }
            });            
    },

    contextMenuItems({ rowData }) {
      return [
        {
          name: this.$t("Common.Open"),
          callback: () => this.$store.dispatch("openMap", rowData.fileName)
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
