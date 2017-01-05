<template>
    <div class="container">
        <div class="header clearfix">
            <nav>
                <ul class="nav nav-pills pull-right">
                    <router-link tag="li" to="/" role="presentation" activeClass="active">
                        <a>Start</a>
                    </router-link>
                    <router-link tag="li" to="/prefilled/580eba69f1844f1892147342f568bb0f$1" role="presentation" activeClass="active">
                        <a>Preffiled</a>
                    </router-link>
                </ul>
            </nav>
        </div>
        <div class="details-interview hold-transition">
            <div class="container-fluid">
                <div class="row">
                    <section class="questionnaire">
                        <div class="unit-section">
                            <router-view></router-view>
                        </div>
                    </section>
                </div>
            </div>
        </div>
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
