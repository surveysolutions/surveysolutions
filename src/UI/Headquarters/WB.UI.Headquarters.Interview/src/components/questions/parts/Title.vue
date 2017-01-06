<template>
    <h5>{{title}}</h5>
</template>
<script lang="ts">
    export default {
        name: "wb-title",
        props: ['entityId'],
        computed: {
            title() {
                const entity = this.$store.state.entityDetails[this.entityId]
                if (entity != null) return entity.title;
                return "";
            }
        }
    }
</script>
