<template>
    <div class="question static-text" v-if="$me" :class="[{'hidden-question': $me.isDisabled}]">
        <div class="question-editor">
            <wb-title />
            <wb-attachment />
            <wb-validation />
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
