<template>
    <div v-if="instructions && instructions.text">
        <div class="information-block instruction" v-if="!instructions.hide || shown">
            <h6>Instruction</h6>
            <p>{{instructions.text}}</p>
        </div>
        <div v-else>
            <button type="button" class="btn btn-link" @click="show">Show instruction</button>
        </div>
    </div>
</template>
<script lang="ts">
    import { mapGetters } from "vuex"

    export default {
        name: "wb-instructions",
        props: ['entityId'],
        computed: {
            instructions() {
                let entity = this.$store.state.entityDetails[this.entityId]
                if (entity != null) {
                    return {
                        text: entity.instructions,
                        hide: entity.hideInstructions
                    }
                }

                return null
            }
        },
        data: () => {
            return {
                shown: false,
            }
        },
        methods: {
            show: function() {
                this.shown = true
            }
        }
    }
</script>
