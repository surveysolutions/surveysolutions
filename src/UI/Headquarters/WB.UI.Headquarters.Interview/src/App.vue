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
            <h3 class="text-muted">Web interview</h3>
        </div>
        <div>
            <router-view></router-view>
        </div>
        <footer class="footer">
            <p>&copy; 2017 World Bank, Inc.</p>
        </footer>
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
