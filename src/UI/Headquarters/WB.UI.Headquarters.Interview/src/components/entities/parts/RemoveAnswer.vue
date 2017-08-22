<template>
    <button v-if="$me.isAnswered" type="submit" class="btn btn-link btn-clear" @click="removeAnswer">
        <span></span>
    </button>
</template>
 <script lang="js">

    import { entityPartial } from "components/mixins"

    export default {
        mixins: [entityPartial],
        name: "wb-remove-answer",
        methods: {
            removeAnswer() {
                this.$store.dispatch("removeAnswer", this.$me.id)
                this.$emit('answerRemoved', this.$me.id)
            }
        }
    }

</script>
