<template>
    <Layout>
        <Navbar slot="header" />
        <div class="row"
            slot>
            <router-view name="sideBar"></router-view>
            <section class="questionnaire details-interview">
                <router-view mode="web"></router-view>
            </section>
            <IdleTimeoutService />
        </div>
        <portal-target name="body"
            multiple >
        </portal-target>
    </Layout>
</template>

<script lang="js">

import '@/assets/css/markup.scss'
import '@/assets/css/markup-web-interview.scss'

export default {
    name: 'app',
}
</script>
