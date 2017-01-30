<template>
    <wb-question :question="$me" questionCssClassName="single-select-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field answered">
                        <input type="text" class="field-to-fill" />
                        <wb-remove-answer />
                    </div>
                </div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="ts">
    import { entityDetails } from "components/mixins"

    export default {
        name: 'ComboboxQuestion',
        mixins: [entityDetails],
        data() {
            return {
                selected: null
            }
        },
        computed: {

        },
        methods: {
        },
        directives: {

        },
        components: {  }
    }
</script>
