<template>
    <wb-question :question="$me" questionCssClassName="single-select-question">
        <div class="question-unit">
            <h5>Lorem ipsum dolor sit amet, consectetur adipiscing elit <small>Integer question</small></h5>
            <wb-instructions />
        </div>
    </wb-question>
</template>
<script lang="ts">
    import { entityDetails } from "components/mixins"

    export default {
        mixins: [entityDetails],
        name: 'Integer',
        props: ['entity']
    }
</script>
