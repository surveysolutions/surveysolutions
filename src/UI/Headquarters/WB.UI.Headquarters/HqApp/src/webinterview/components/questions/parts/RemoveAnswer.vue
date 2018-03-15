<template>
    <button v-if="$me.isAnswered && $me.acceptAnswer" tabindex="-1" type="submit" class="btn btn-link btn-clear" @click="removeAnswer">
        <span></span>
    </button>
</template>
 <script lang="js">

    import { entityPartial } from "~/webinterview/components/mixins"

    export default {
        mixins: [entityPartial],
        props: {
            onRemove: {
                default: null
            }
        },
        name: "wb-remove-answer",
        methods: {
            removeAnswer() {
                if(this.onRemove) {
                    this.onRemove()
                }
                else {
                    this.$store.dispatch("removeAnswer", this.$me.id)
                    this.$emit('answerRemoved', this.$me.id)
                }
            }
        }
    }

</script>
