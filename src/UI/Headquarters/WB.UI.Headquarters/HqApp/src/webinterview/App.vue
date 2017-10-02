<template>
    <Layout>
        <Navbar slot="header" />
        <div class="row" slot>
            <router-view name="sideBar">
            </router-view>
            <section class="questionnaire details-interview">
                <router-view></router-view>
            </section>
        </div>
    </Layout>
</template>

<script lang="js">
    // import "toastr/build/toastr.css"
    import idle from "./IdleTimeout"

    export default {
        name: 'app',
        created() {
            idle.start()
        }
    }
</script>