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
                    <tr v-for="setting in settings"
                        v-bind:key="setting.key">
                        <td>{{ setting.key }}</td>
                        <td>{{ setting.value }}</td>
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
            settings: [],
        }
    },
    mounted() {
        const self = this
        this.$hq.ControlPanel.getConfiguration().then(response => {
            self.settings = response.data
        })
    },
}
</script>

<style></style>
