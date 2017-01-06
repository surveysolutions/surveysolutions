<template>
    <div>
        <nav>
            <ul class="nav nav-pills">
                <router-link tag="li" to="/" role="presentation">
                    <a>Start</a>
                </router-link>
                <router-link tag="li" to="/prefilled" role="presentation">
                    <a>Preffiled</a>
                </router-link>
            </ul>
        </nav>
        <main class="web-interview hold-transition">
            <div class="container-fluid">
                <div class="row">
                    <section class="questionnaire">
                        <div class="unit-section">
                            <router-view></router-view>
                        </div>
                    </section>
                </div>
            </div>
        </main>
    </div>
</template>
<script lang='ts'>
    export default {
        name: 'app'
    }
</script>
<style lang='scss'>
  @import "assets/styles"
</style>
