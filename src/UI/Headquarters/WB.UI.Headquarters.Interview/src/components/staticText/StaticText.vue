<template>
    <div class="question static-text" v-if="$me">
        <div class="question-editor">
            <wb-text />
            <wb-attachment />            
        </div>
    </div>
</template>
<script lang="ts">
    import { entityDetails } from "components/mixins"

    export default {
        name: 'StaticText',
        mixins: [entityDetails],
        data: () => {
            return {
                text: ''
            }
        }
    }
</script>