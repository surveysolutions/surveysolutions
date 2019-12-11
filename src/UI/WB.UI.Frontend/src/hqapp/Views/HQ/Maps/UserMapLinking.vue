<template>
    <HqLayout :hasFilter="false" >
        <div slot="headers">
                <ol class="breadcrumb">
                    <li>
                        <a :href="$config.model.mapsUrl">{{$t("Pages.MapList_Title")}}</a>
                    </li>
                    <li>
                        <a :href="$config.model.userMapsUrl">{{$t('Pages.MapList_UserMapsLink')}}</a>
                    </li>                    
                </ol>
                    <h1>{{$t("Pages.MapLinking_DescriptionTitle")}}</h1>

                    <p>{{$t("Pages.MapLinking_Description")}}</p>  
        </div>                
        <div class="row flex-row">
            <div class="flex-block">
                <div class="selection-box reset-margin">
                    <div class="block">
                        <h3>{{$t("Pages.MapLinking_UploadDescriptionTitle")}}</h3>
                        <p>{{$t("Pages.MapLinking_UploadDescription")}}</p>
                    </div>
                    <div>
                        <a :href="$config.model.downloadAllUrl">{{$t("Pages.MapLinking_DownloadExisting")}}</a>
                    <div class="info-block" v-if="actionsAlowed">                        
                            <label class="btn btn-success btn-file">
                                {{$t("Pages.MapLinking_UploadFile")}}
                            <input :accept="$config.model.fileExtension" ref="uploader" id="File" name="File" @change="onFileChange" type="file" value="" />
                            </label>                        
                    
                    </div>
                    <div class="info-block" v-if="actionsAlowed">
                     <div ref="status" ><p>{{statusMessage}}</p></div>
                    </div>
                    <div>                    
                        <p>{{$t("Pages.MapLinking_UploadFileDescription")}}</p>
                        <p>{{$t("Pages.MapLinking_UploadFileDescription1")}}</p>
                    </div>                        
                    </div>
                </div>
            </div>
        </div>
    </HqLayout>
</template>
<script>
export default {  
    data: function(){
        return {        
            statusMessage: ''
        }        
    }, 
    mounted() {},
    computed: {
        config(){
            return this.$config.model;
        },
        actionsAlowed() {
            return !this.config.isObserver && !this.config.isObserving;
        }
    },
    methods:{
        updateStatus(newMessage){
          this.statusMessage = this.$t("Pages.Map_Status") + ": " + newMessage;
      },
        onFileChange(e){
            const statusupdater = this.updateStatus;
            const uploadingMessage = this.$t("Pages.Map_Uploading");
            const uploadingErrorMessage = this.$t("Pages.Map_UploadingError");
            
            const fd = new FormData();
            fd.append("", this.$refs.uploader.files[0]);
        
            $.ajax({
                url: this.$config.model.uploadUrl,
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
                },
                error : function(error){
                    statusupdater(uploadingErrorMessage);
                }
            });  
    },

    }
}
</script>
