<template>
    <button type="button" class="btn-link button-flag" @click="setFlag">
    </button>
</template>
<script lang="js">
    import { entityPartial } from "~/webinterview/components/mixins"

    export default {
        mixins: [entityPartial],
        name: "wb-flag",
        methods:{
            setFlag(){
                const hasFlag = this.$store.getters.flags.indexOf(this.$me.id) > -1;
                this.$store.dispatch("setFlag", { questionId: this.$me.id, hasFlag: !hasFlag})
            }
        }
    }
</script>
