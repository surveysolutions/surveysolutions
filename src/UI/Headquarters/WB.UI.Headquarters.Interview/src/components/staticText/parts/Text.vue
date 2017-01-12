<template>
    <h5>{{$me.title}}</h5>
</template>
<script lang="ts">
    import { entityPartial } from "components/mixins"

    export default {
        mixins: [entityPartial],
        name: "wb-text"
    }
</script>
