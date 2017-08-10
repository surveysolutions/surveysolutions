<template>
    <wb-question :question="$me">
        <h1 class="tape"><span>{{ $t("QuestionUnsupported") }}</span></h1>
    </wb-question>
</template>
<script lang="js">
    import { entityDetails } from "components/mixins"

    export default {
        name: "UNSUPPORTED",
        mixins: [entityDetails],
    }
</script>
<style lang="scss" scoped>
    $color: #fde252;
    .tape {
        font-size:2em;
        margin: 0 0 0;
        padding-top: 20px;
        padding-left: 40px;
        height: 80px;
        background-image:  linear-gradient(transparent 20px, $color 20px),
                           repeating-linear-gradient(45deg, transparent, transparent 35px,  $color 35px,  $color 73px);
        background-size: 100px,auto 100px,auto;
    }

</style>
