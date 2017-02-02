<template>
    <Layout>
        <Navbar slot="header" />
        <div class="row" slot>
            <router-view name="sideBar">
            </router-view>
            <section class="questionnaire" :class="{'details-interview': showSidebar}">
                <router-view></router-view>
            </section>
            <idle-timeout></idle-timeout>
        </div>
    </Layout>
</template>

<script lang="ts">
    import IdleTimeout from "./components/IdleTimeout"

    export default {
        name: 'app',
        computed: {
            showSidebar() {
                return this.$store.getters.hasSidebarData && this.$route.params.sectionId != null
            }
        },
        components: { IdleTimeout }
    }
</script>

<style lang='scss'>
  @import "assets/styles"
</style>
