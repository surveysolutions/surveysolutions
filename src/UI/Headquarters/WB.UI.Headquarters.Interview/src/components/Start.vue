<template>
    <div>
        <h1>Start page will be removed or replaced</h1>
        <label>Interview id: </label>
        <input id="interviewId" type="text" v-model="interviewId" />
        <br />
        <button @click="openInterview">Open</button>
    </div>
</template>
<script lang="ts">
    export default {
        name: 'start-view',
        data: () => {
            return {
                interviewId: "580eba69f1844f1892147342f568bb0f$1"
            }
        },
        methods: {
            openInterview() {
                if (this.interviewId) {
                    this.$router.push({
                        name: "prefilled",
                        params: { id: this.interviewId }
                    });
                }
            }
        }
    }
</script>
