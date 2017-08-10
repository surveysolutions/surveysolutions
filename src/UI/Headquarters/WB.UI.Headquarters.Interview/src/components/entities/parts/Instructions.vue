<template>
    <div v-if="$me.instructions && !$me.isDisabled" class="instructions-wrapper">
        <div class="information-block instruction" v-if="!$me.hideInstructions || shown">
            <h6>{{ $t("Instruction") }}</h6>
            <p>{{$me.instructions}}</p>
        </div>
        <button v-else type="button" class="btn btn-link" @click="show">{{ $t("InstructionShow") }}</button>
    </div>
</template>
<script lang="js">
    import { mapGetters } from "vuex"
    import { entityPartial } from "components/mixins"

    export default {
        name: "wb-instructions",
        mixins: [entityPartial],
        data: () => {
            return {
                shown: false,
            }
        },
        methods: {
            show: function () {
                this.shown = true
            }
        }
    }
</script>
