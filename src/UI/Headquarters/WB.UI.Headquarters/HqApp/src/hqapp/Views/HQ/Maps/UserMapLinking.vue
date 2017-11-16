<template>
    <HqLayout :hasFilter="false" >
        <div class="row">
            <div class="col-sm-6 col-xs-10 prefilled-data-info info-block">
                <p>Upload file containing correspondence between maps and users</p>                
            </div>
        </div>

        <div class="row flex-row">
            <div >
                <div class="selection-box">
                    <div class="block">
                        <h3>Upload users mappings</h3>
                        <p>This will allow you to create map to user correspondence.</p>
                    </div>
                    <div>
                        <a :href="$config.model.downloadAllUrl">Download existing links</a>


                        <form action="/Maps/UploadMappings" enctype="multipart/form-data" id="MappingsUploadForm" method="post">
                            <label class="btn btn-success btn-file">
                                    Upload .tsv file
                                    <input accept=".tsv"  id="File" name="File" onchange="this.form.submit()" type="file" value="" />
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
    
    methods: {   
        
    },
    computed: {        
        config() {
            return this.$config.model;
        }
    }

}
</script>
