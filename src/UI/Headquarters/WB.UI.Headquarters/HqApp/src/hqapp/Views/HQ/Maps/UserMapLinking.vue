<template>
    <HqLayout :hasFilter="false" >
        <div slot="headers">
                <ol class="breadcrumb">
                    <li>
                        <a :href="$config.model.mapsUrl">{{$t("Pages.MapList_Title")}}</a>
                    </li>
                </ol>
                    <h1>{{$t("Pages.MapLinking_DescriptionTitle")}}</h1>

                    <p>{{$t("Pages.MapLinking_Description")}}</p>  
        </div>                
        <div class="row flex-row">
            <div class="flex-block" style="margin-left:0px;">
                <div class="selection-box">
                    <div class="block">
                        <h3>{{$t("Pages.MapLinking_UploadDescriptionTitle")}}</h3>
                        <p>{{$t("Pages.MapLinking_UploadDescription")}}</p>
                    </div>
                    <div>
                        <a :href="$config.model.downloadAllUrl">{{$t("Pages.MapLinking_DownloadExisting")}}</a>
                    <form :action="$config.model.uploadUrl" enctype="multipart/form-data" id="MapsUploadForm" method="post">
                        <label class="btn btn-success btn-file">
                            {{$t("Pages.MapLinking_UploadFile")}}
                            <input accept=".tsv" id="File" name="File" onchange="this.form.submit()" type="file" value="" />
                        </label>
                    </form>
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
    mounted() {}
}
</script>
