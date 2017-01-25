<template>
    <Layout>
        <Navbar slot="header" />
        <div class="row" slot>
            <Sidebar />
            <section class="questionnaire">
                <router-view></router-view>
            </section>
        </div>
    </Layout>
</template>

<script lang="ts">
    export default {
        name: 'app'
    }
</script>

<style lang='scss'>
  @import "assets/styles"
</style>
