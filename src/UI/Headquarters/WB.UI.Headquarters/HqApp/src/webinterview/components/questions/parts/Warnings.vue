<template>
    <div class="information-block text-warning" v-if="$me.validity.warnings.length > 0">
        <h6>{{ $t("WebInterviewUI.WarningsHeader") }}</h6>
        <template v-for="message in $me.validity.warnings">
            <p v-dateTimeFormatting v-html="message" :key="message"></p>
        </template>
    </div>
</template>
<script lang="js">
    import { entityPartial } from "~/webinterview/components/mixins"

    export default {
        mixins: [entityPartial],
        name: "wb-validation"
    }
</script>

