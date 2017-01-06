<template>
    <div class="question">
        <h5>Lorem ipsum dolor sit amet, consectetur adipiscing elit <small>Integer question</small></h5>
        <wb-instructions :entityId="entity.Identity"/>
    </div>
</template>
<script lang="ts">
    export default {
        name: 'Integer',
        props: ['entity']
    }
</script>
