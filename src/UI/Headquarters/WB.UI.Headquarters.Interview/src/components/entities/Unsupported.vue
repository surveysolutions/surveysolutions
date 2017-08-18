<template>
    <wb-question :question="$me" isDisabled="true">
        <h1><span>{{ $t("QuestionUnsupported") }}</span></h1>
    </wb-question>
</template>
<script lang="js">
    import { entityDetails } from "components/mixins"

    export default {
        name: "UNSUPPORTED",
        mixins: [entityDetails],
    }
</script>
