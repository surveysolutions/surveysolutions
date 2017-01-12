<template>
    <h5>{{$me.attachment}}</h5>
</template>
<script lang="ts">
    import { entityPartial } from "components/mixins"

    export default {
        mixins: [entityPartial],
        name: "wb-attachment"
    }
</script>
