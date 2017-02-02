<template>
    <Layout>
        <Navbar slot="header" />
        <div class="row" slot>
            <router-view name="sideBar">
            </router-view>
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
                return this.$store.getters.hasSidebarData && this.$route.params.sectionId != null
            }
        }
    }
</script>

<style lang='scss'>
  @import "assets/styles"
</style>
