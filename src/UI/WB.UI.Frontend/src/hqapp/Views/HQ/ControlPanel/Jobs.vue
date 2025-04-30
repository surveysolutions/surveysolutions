<template>
    <HqLayout>
        <div class="row">
            <table>
                <thead>
                    <tr>
                        <th>key</th>
                        <th>value</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="job in jobs" v-bind:key="job.key">
                        <td>{{ job.key }}</td>
                        <td>{{ job.value }}</td>
                    </tr>
                </tbody>
            </table>
        </div>
    </HqLayout>
</template>

<script>
export default {
    data() {
        return {
            jobs: [],
        }
    },
    mounted() {
        const self = this
        this.$hq.ControlPanel.getJobs().then(response => {
            self.jobs = response.data
        })
    },
}
</script>

<style></style>
