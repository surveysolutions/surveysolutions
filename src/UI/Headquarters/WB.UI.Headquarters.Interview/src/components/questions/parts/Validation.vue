<template>
    <div class="information-block text-danger" v-if="!$me.validity.isValid">
        <h6>Answer is invalid</h6>
        <template v-for="message in $me.validity.messages">
            <p>{{ message }}</p>
        </template>
    </div>
</template>
<script lang="ts">
    import { entityPartial } from "components/mixins"

    export default {
        mixins: [entityPartial],
        name: "wb-validation"
    }
</script>
