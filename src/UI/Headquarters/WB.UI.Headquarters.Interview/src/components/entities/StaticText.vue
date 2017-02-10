<template>
    <div class="question static-text" v-if="!$me.isLoading && !($me.isDisabled && $me.hideIfDisabled)"
        :class="[{'hidden-question': $me.isDisabled}]">
        <div class="question-editor">
            <div :class="[{'text-danger': !$me.validity.isValid}]">
                <wb-title />
            </div>
            <wb-attachment :contentId="$me.attachmentContent" customCssClass="static-text-image" v-if="$me.attachmentContent" />
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
