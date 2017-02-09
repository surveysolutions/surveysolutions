<template>
    <Layout>
        <Navbar slot="header" />
        <div class="row" slot>
            <router-view name="sideBar">
            </router-view>
            <section class="questionnaire details-interview">
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
        components: { IdleTimeout }
    }
</script>

<style lang='scss'>
  @import "assets/styles"
</style>
