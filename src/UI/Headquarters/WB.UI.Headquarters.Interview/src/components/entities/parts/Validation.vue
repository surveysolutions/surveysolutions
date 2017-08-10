<template>
    <div class="information-block text-danger" v-if="!$me.validity.isValid">
        <h6 v-if="$me.validity.errorMessage == null">{{ $t("AnswerIsInvalid") }}</h6>
        <h6 v-if="$me.validity.errorMessage">{{ $t("AnswerWasNotSaved") }}</h6>
        <template v-for="message in $me.validity.messages">
            <p v-dateTimeFormatting v-html="message"></p>
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
