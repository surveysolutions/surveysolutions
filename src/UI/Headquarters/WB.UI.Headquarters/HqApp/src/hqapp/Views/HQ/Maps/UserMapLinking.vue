<template>
    <HqLayout :hasFilter="false" >

        <div slot="headers">
                <ol class="breadcrumb">
                    <li>
                        <a :href="$config.model.mapsUrl">{{$t("Dashboard.Maps")}}</a>
                    </li>
                </ol>
                    <h1>Manage user map links</h1>

                    <p>Upload file containing correspondence between maps and users</p>  
        </div>
                
        <div class="row flex-row">
            <div class="flex-block" style="margin-left:0px;">
                <div class="selection-box">
                    <div class="block">
                        <h3>Upload users mappings</h3>
                        <p>This will allow you to create map to user correspondence.</p>
                    </div>
                    <div>
                        <a :href="$config.model.downloadAllUrl">Download existing links</a>
                    <form :action="$config.model.uploadUrl" enctype="multipart/form-data" id="MapsUploadForm" method="post">
                        <label class="btn btn-success btn-file">
                            Upload .tsv file
                            <input accept=".tsv" id="File" name="File" onchange="this.form.submit()" type="file" value="" />
                        </label>
                    </form>
                    <div>                    
                        <p>Upload .tsv file with file mappings.</p>
                                <p>Invalid names and user names will be ignored.</p>
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
