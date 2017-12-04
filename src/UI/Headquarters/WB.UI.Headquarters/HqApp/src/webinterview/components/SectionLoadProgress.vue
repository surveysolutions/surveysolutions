<template>
    <transition name="slide-fade">
        <div v-if="visible" class="loading">
            <div>{{ $t("WebInterviewUI.LoadingWait") }}</div>
        </div>
    </transition>
</template>

<style lang='css' scoped>
.slide-fade-leave-active {
  transition: all 0.3s ease;
}
.slide-fade-leave-to {
  opacity: 0;
}
</style>

<script lang="js">
    import { debounce } from "lodash"
  
    export default {
        data() {
            return {
                visible: false
            }
        },
        watch: {
            isLoading() {
                this.setVisibility(this);
            },
           
            '$store.state.webinterview.connection.isDisconnected' (to) {
                if (to) {
                    this.visible = false
                }
            }
        },
        computed: {
            isLoading() {
                return this.$store.getters.loadingProgress;
            }
        },
        
        methods: {
            setVisibility: debounce((self) => {
                self.visible = self.isLoading
            }, 500)
        }
    }

</script>
