<template>
    <div class="information-block text-danger" v-if="!$me.validity.isValid">
        <h6 v-if="$me.validity.errorMessage == null">Answer is invalid</h6>
        <h6 v-if="$me.validity.errorMessage">Answer was not saved</h6>
        <template v-for="message in $me.validity.messages">
            <p>{{ message }}</p>
        </template>
    </div>
</template>
<script lang="js">
    import { entityPartial } from "components/mixins"

    export default {
        mixins: [entityPartial],
        name: "wb-validation"
    }
</script>
