<template>
    <div>
        <h1>Start page will be removed or replaced</h1>
    </div>
</template>
<script lang="ts">
    export default {
        name: 'start-view',
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
