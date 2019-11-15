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
        name: 'app',
        // components: {
        //     signalr: window.CONFIG.NetCore 
        //         ? () => import('./components/signalr/core.signalr') 
        //         : () => import('./components/signalr/old.signalr')
        // }
    }
</script>
