<template>
    <div>
        <span>Single options</span>
        <wb-instructions :entityId="entity.Identity" />
    </div>
</template>
<script lang="ts">
    export default {
        name: 'CategoricalSingle',
        props: ['entity']
    }
</script>
