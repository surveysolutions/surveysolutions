<template>
    <div class="image-zoom-box static-text-image" v-if="$me.attachmentContent != null"> 
        <img :src="'../../../api/Attachments/Content/' + $me.attachmentContent + '?maxSize=435'" alt="static-text-image" class="zoomImg"> 
            <div class="modal-img">
                <span class="close-zoomming-img">×</span>
                <img class="modal-img-content" src="" alt="">
                <span class="caption"></span>
            </div>
    </div>
</template>
<script lang="ts">
    import { entityPartial } from "components/mixins"

    export default {
        mixins: [entityPartial],
        name: "wb-attachment"
    }
</script>
