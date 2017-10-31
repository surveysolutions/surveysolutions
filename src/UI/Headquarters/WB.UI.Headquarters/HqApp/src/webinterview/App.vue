<template>
    <Layout>
        <Navbar slot="header" />

        <div class="row" slot>
            <router-view name="sideBar">
            </router-view>
            <section class="questionnaire details-interview">
                <router-view></router-view>
            </section>
            <IdleTimeoutService />
        </div>

    </Layout>
</template>

<script lang="js">
    export default {
        name: 'app'
    }
</script>
