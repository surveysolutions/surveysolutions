<template>
    <Layout>
        <template v-slot:header>
            <Navbar  />
        </template>
        <template v-slot>
            <div class="row" >
                <router-view name="sideBar"></router-view>
                <section class="questionnaire details-interview">
                    <router-view mode="web"></router-view>
                </section>
                <IdleTimeoutService />
            </div>
        </template>
    </Layout>
</template>

<script lang="js">

import '@/assets/css/markup.scss'
import '@/assets/css/markup-web-interview.scss'

import { onMounted } from 'vue'

export default {
    name: 'app',
    //TODO
    //fix layout instead of recalculation
    setup() {
        onMounted(() => {
            if (typeof window.ajustNoticeHeight === 'function') {
                window.ajustNoticeHeight()
            }
            if (typeof window.ajustDetailsPanelHeight === 'function') {
                window.ajustDetailsPanelHeight()
            }
        })
    },
}
</script>
