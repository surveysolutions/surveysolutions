<template>
    <div>
        <div class="information-block instruction" v-if="!$me.hideInstructions && shown">
            <h6>Instruction</h6>
            <p>{{$me.instructions}}</p>
        </div>
        <div v-else>
            <button type="button" class="btn btn-link" @click="show">Show instruction</button>
        </div>
    </div>
</template>
<script lang="ts">
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
