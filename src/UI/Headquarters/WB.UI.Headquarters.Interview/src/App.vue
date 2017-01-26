<template>
    <Layout>
        <Navbar slot="header" />
        <div class="row" slot>
            <Sidebar />
            <section class="questionnaire" :class="{'details-interview': showSidebar}">
                <router-view></router-view>
            </section>
        </div>
    </Layout>
</template>

<script lang="ts">
    export default {
        name: 'app',
        computed: {
            showSidebar() {
                return this.$store.state.sidebar != null && this.$store.state.sidebar.length > 0
            }
        }
    }
</script>

<style lang='scss'>
  @import "assets/styles"
</style>
